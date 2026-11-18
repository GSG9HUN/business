using System.Collections.Concurrent;
using System.Text;
using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Interface.Service.Presentation.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Presentation;

/// <summary>Owns expiring, caller-bound Discord pagination panels for read-only results.</summary>
/// <param name="responses">Creates bounded status embeds.</param>
/// <param name="localization">Resolves navigation labels and failures in the guild language.</param>
/// <param name="logger">Records failed page loads and message edits.</param>
public sealed class DiscordPaginationService(
    IResponseBuilder responses,
    ILocalizationService localization,
    ILogger<DiscordPaginationService> logger) : IAsyncDisposable
{
    private const string Prefix = "music-page:";
    private const int MaxBodyLength = 3500;
    private const int MaxActivePanels = 512;
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(10);
    private readonly ConcurrentDictionary<string, Panel> _panels = new();
    private readonly ConcurrentDictionary<string, Panel> _cleanup = new();
    private readonly ConcurrentDictionary<(ulong Guild, ulong User), string> _latest = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly object _ownershipLock = new();

    /// <summary>Retains bounded UI ownership and serializes page edits against expiry.</summary>
    private sealed class Panel(
        string id,
        ulong guild,
        ulong channel,
        ulong owner,
        PaginationRequest request,
        PaginationPage page,
        CancellationTokenSource lifetime)
    {
        public readonly string Id = id;
        public readonly ulong Guild = guild;
        public readonly ulong Channel = channel;
        public readonly ulong Owner = owner;
        public readonly PaginationRequest Request = request;
        public readonly DateTimeOffset Expires = DateTimeOffset.UtcNow + Lifetime;
        public readonly SemaphoreSlim Gate = new(1, 1);
        public readonly CancellationTokenSource LifetimeToken = lifetime;
        public readonly CancellationToken Token = lifetime.Token;
        public PaginationPage Page = page;
        public DiscordMessage? Message;
        public Task Expiry = Task.CompletedTask;
    }

    /// <summary>Builds snapshot pages constrained by both item count and formatted text length.</summary>
    public static PaginationRequest FromItems(string title, IReadOnlyList<string> items, int initialPage = 1, int pageSize = 10)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        var pages = new List<string>();
        var text = new StringBuilder();
        var count = 0;

        foreach (var item in items)
        {
            var line = DiscordTextSanitizer.Truncate(item, MaxBodyLength);

            if (count > 0
                && (count >= pageSize
                    || text.Length + line.Length + Environment.NewLine.Length > MaxBodyLength))
            {
                pages.Add(text.ToString());
                text.Clear();
                count = 0;
            }

            if (text.Length > 0)
            {
                text.AppendLine();
            }

            text.Append(line);
            count++;
        }

        if (text.Length > 0 || pages.Count == 0)
        {
            pages.Add(text.ToString());
        }

        return Snapshot(title, pages, initialPage, items.Count);
    }

    /// <summary>Splits continuous text at line boundaries when possible without losing surrogate pairs or text.</summary>
    public static PaginationRequest FromText(string title, string text, int initialPage = 1, int pageSize = 1600)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 2);
        pageSize = Math.Min(pageSize, MaxBodyLength);
        var pages = new List<string>();
        var start = 0;

        while (start < text.Length)
        {
            var length = Math.Min(pageSize, text.Length - start);

            if (start + length < text.Length)
            {
                var newline = text.LastIndexOf('\n', start + length - 1, length);

                if (newline >= start + length / 2)
                {
                    length = newline - start + 1;
                }

                if (char.IsHighSurrogate(text[start + length - 1])
                    && char.IsLowSurrogate(text[start + length]))
                {
                    length--;
                }
            }

            pages.Add(text.Substring(start, length));
            start += length;
        }

        if (pages.Count == 0)
        {
            pages.Add(string.Empty);
        }

        return Snapshot(title, pages, initialPage, null);
    }

    /// <summary>Creates a clamped request over immutable formatted snapshot pages.</summary>
    private static PaginationRequest Snapshot(string title, IReadOnlyList<string> pages, int initialPage, int? total)
    {
        return new PaginationRequest((number, token) =>
        {
            token.ThrowIfCancellationRequested();
            var page = Math.Clamp(number, 1, pages.Count);
            return Task.FromResult(new PaginationPage(title, pages[page - 1], page, pages.Count, total));
        }, initialPage);
    }

    /// <summary>Sends one page and registers buttons only when additional pages exist.</summary>
    public async Task SendAsync(IDiscordMessage context, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guild = context.Channel.Guild.Id;
        var page = request.InitialContent ?? await request.LoadPageAsync(request.InitialPage, cancellationToken);
        var key = (guild, context.Author.Id);

        if (page.PageCount <= 1)
        {
            await context.RespondAsync(BuildEmbed(guild, page)).WaitAsync(cancellationToken);
            return;
        }

        var panel = new Panel(Guid.NewGuid().ToString("N"), guild, context.Channel.Id, context.Author.Id,
            request, page, CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token));

        lock (_ownershipLock)
        {
            if (_latest.TryGetValue(key, out var old) && _panels.TryGetValue(old, out var previous))
            {
                Retire(previous);
            }

            _panels[panel.Id] = panel;
            _latest[key] = panel.Id;

            if (_panels.Count > MaxActivePanels)
            {
                var oldest = _panels.Values.OrderBy(value => value.Expires)
                    .First();
                Retire(oldest);
            }
        }

        try
        {
            panel.Message = await context.RespondWithComponentsAsync(BuildMessage(panel));
            _cleanup[panel.Id] = panel;
            panel.Expiry = ExpireAsync(panel);

            if (cancellationToken.IsCancellationRequested)
            {
                Retire(panel);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            Retire(panel);

            if (panel.Message is null)
            {
                panel.LifetimeToken.Dispose();
            }

            throw;
        }
        catch (Exception exception)
        {
            Retire(panel);
            panel.LifetimeToken.Dispose();
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogDebug(exception, "Pagination embed failed; returning bounded text. Guild={GuildId}", guild);
            await context.RespondAsync(BuildTextFallback(guild, page)).WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    /// <summary>Processes only this component's IDs and validates caller, guild, channel and message binding.</summary>
    public async Task OnComponentInteractionAsync(DiscordClient client, ComponentInteractionCreatedEventArgs args)
    {
        if (!args.Id.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return;
        }

        var parts = args.Id[Prefix.Length..].Split(':');

        if (parts.Length != 2 || parts[1] is not ("previous" or "next"))
        {
            await DenyAsync(args, LocalizationKeys.PaginationExpired);
            return;
        }

        if (!_panels.TryGetValue(parts[0], out var panel))
        {
            await DenyAsync(args, LocalizationKeys.PaginationExpired);
            return;
        }

        if (args.User.Id != panel.Owner
            || args.Guild?.Id != panel.Guild
            || args.Channel.Id != panel.Channel
            || args.Message.Id != panel.Message?.Id)
        {
            await DenyAsync(args, LocalizationKeys.PaginationDenied);
            return;
        }

        if (panel.Expires <= DateTimeOffset.UtcNow
            || panel.Token.IsCancellationRequested)
        {
            Retire(panel);
            await DenyAsync(args, LocalizationKeys.PaginationExpired);
            return;
        }

        if (!await panel.Gate.WaitAsync(0))
        {
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            return;
        }

        try
        {
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(panel.Token);
            deadline.CancelAfter(OperationTimeout);
            var number = Math.Clamp(panel.Page.Number + (parts[1] == "next" ? 1 : -1), 1, Math.Max(1, panel.Page.PageCount));
            var page = await panel.Request.LoadPageAsync(number, deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();

            if (panel.Expires <= DateTimeOffset.UtcNow)
            {
                Retire(panel);
                return;
            }

            var previousPage = panel.Page;
            panel.Page = page;
            try
            {
                await args.Message.ModifyAsync(BuildMessage(panel));
            }
            catch
            {
                panel.Page = previousPage;
                throw;
            }
        }
        catch (OperationCanceledException) when (panel.Token.IsCancellationRequested)
        {
            // Expiry or replacement owns final button disabling.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Pagination update failed. Guild={GuildId}", panel.Guild);
            await args.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(responses.CreateEmbed(panel.Guild, localization.Get(panel.Guild, LocalizationKeys.PaginationError),
                    DiscordColor.Red, LocalizationKeys.ResponseErrorTitle))
                .AsEphemeral());
        }
        finally
        {
            panel.Gate.Release();
        }
    }

    /// <summary>Creates the single bounded embed and localized navigation footer.</summary>
    private DiscordEmbed BuildEmbed(ulong guild, PaginationPage page)
    {
        var footer = page.TotalItems.HasValue
            ? localization.Get(guild, LocalizationKeys.PaginationFooter, page.Number, page.PageCount, page.TotalItems.Value)
            : localization.Get(guild, LocalizationKeys.PaginationTextFooter, page.Number, page.PageCount);
        return new DiscordEmbedBuilder(responses.CreateEmbed(guild,
                string.IsNullOrEmpty(page.Description) ? localization.Get(guild, LocalizationKeys.PaginationEmpty) : page.Description))
            .WithTitle(DiscordTextSanitizer.Truncate(page.Title, 256))
            .WithFooter(footer)
            .Build();
    }

    /// <summary>Preserves the initial page and navigation totals within Discord's plain-text message limit.</summary>
    private string BuildTextFallback(ulong guild, PaginationPage page)
    {
        var title = DiscordTextSanitizer.Truncate(page.Title, 256);
        var footer = page.TotalItems.HasValue
            ? localization.Get(guild, LocalizationKeys.PaginationFooter, page.Number, page.PageCount, page.TotalItems.Value)
            : localization.Get(guild, LocalizationKeys.PaginationTextFooter, page.Number, page.PageCount);
        footer = DiscordTextSanitizer.Truncate(footer, 200);
        var description = string.IsNullOrEmpty(page.Description)
            ? localization.Get(guild, LocalizationKeys.PaginationEmpty) : page.Description;
        return title + "\n" + DiscordTextSanitizer.Truncate(description, 2000 - title.Length - footer.Length - 2)
            + "\n" + footer;
    }

    /// <summary>Builds the same navigation layout for active and expired pages.</summary>
    private DiscordMessageBuilder BuildMessage(Panel panel, bool disabled = false)
    {
        var builder = new DiscordMessageBuilder().AddEmbed(BuildEmbed(panel.Guild, panel.Page));

        if (panel.Page.PageCount > 1)
        {
            builder.AddActionRowComponent([
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, Prefix + panel.Id + ":previous",
                    localization.Get(panel.Guild, LocalizationKeys.PaginationPrevious), disabled || panel.Page.Number <= 1),
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, Prefix + panel.Id + ":next",
                    localization.Get(panel.Guild, LocalizationKeys.PaginationNext), disabled || panel.Page.Number >= panel.Page.PageCount)
            ]);
        }

        return builder;
    }

    /// <summary>Returns a localized ephemeral rejection without revealing another caller's data.</summary>
    private Task DenyAsync(ComponentInteractionCreatedEventArgs args, string key)
    {
        return args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(responses.CreateEmbed(args.Guild?.Id,
                args.Guild is null ? localization.Get(key) : localization.Get(args.Guild.Id, key),
                DiscordColor.Gold, LocalizationKeys.ResponseWarningTitle))
                .AsEphemeral());
    }

    /// <summary>Removes ownership immediately and asks the existing expiry task to disable its message.</summary>
    private void Retire(Panel panel)
    {
        lock (_ownershipLock)
        {
            if (_panels.TryRemove(panel.Id, out _))
            {
                ((ICollection<KeyValuePair<(ulong Guild, ulong User), string>>)_latest)
                    .Remove(new((panel.Guild, panel.Owner), panel.Id));
                panel.LifetimeToken.Cancel();
            }
        }
    }

    /// <summary>Disables expired/replaced buttons and releases retained callbacks.</summary>
    private async Task ExpireAsync(Panel panel)
    {
        try
        {
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(0, (panel.Expires - DateTimeOffset.UtcNow).TotalMilliseconds)),
                    panel.Token);
            }
            catch (OperationCanceledException)
            {
                // Replacement and shutdown also end ownership.
            }

            Retire(panel);

            if (_shutdown.IsCancellationRequested)
            {
                return;
            }

            await panel.Gate.WaitAsync();
            try
            {
                using var deadline = new CancellationTokenSource(OperationTimeout);

                if (panel.Message is not null)
                {
                    await panel.Message.ModifyAsync(BuildMessage(panel, true)).WaitAsync(deadline.Token);
                }
            }
            finally
            {
                panel.Gate.Release();
            }
        }
        catch (Exception exception)
        {
            logger.LogDebug(exception, "Pagination expiry edit failed. Guild={GuildId}", panel.Guild);
        }
        finally
        {
            panel.LifetimeToken.Dispose();
            _cleanup.TryRemove(panel.Id, out _);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        var pending = _cleanup.Values.ToArray();
        _shutdown.Cancel();
        await Task.WhenAll(pending.Select(panel => panel.Expiry));
        _panels.Clear();
        _latest.Clear();
        _shutdown.Dispose();
    }
}
