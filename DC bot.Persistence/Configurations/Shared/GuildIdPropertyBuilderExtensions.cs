using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Shared;

/// <summary>
/// Provides the shared PostgreSQL representation for unsigned Discord guild identifiers.
/// </summary>
internal static class GuildIdPropertyBuilderExtensions
{
    internal const string StoreType = "numeric(20,0)";

    /// <summary>
    /// Maps unsigned Discord snowflakes to the shared PostgreSQL storage representation.
    /// </summary>
    /// <param name = "builder">The builder value used by this storage contract.</param>
    /// <returns>The mapped, normalized, or configured value described above.</returns>
    internal static PropertyBuilder<ulong> HasGuildIdStorage(this PropertyBuilder<ulong> builder)
    {
        return builder.HasConversion<decimal>()
            .HasColumnType(StoreType);
    }
}
