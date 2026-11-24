namespace DC_bot.Interface.Service.Presentation.Models;

/// <summary>Describes a bounded page loader without creating a player or changing playback state.</summary>
/// <param name="LoadPageAsync">Loads a requested one-based page within its captured owner scope.</param>
/// <param name="InitialPage">The requested starting page.</param>
/// <param name="InitialContent">An optional already loaded first response.</param>
public sealed record PaginationRequest(
    Func<int, CancellationToken, Task<PaginationPage>> LoadPageAsync,
    int InitialPage = 1,
    PaginationPage? InitialContent = null);