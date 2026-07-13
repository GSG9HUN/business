using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Persistence.Configurations;

internal static class GuildIdPropertyBuilderExtensions
{
    internal const string StoreType = "numeric(20,0)";

    internal static PropertyBuilder<ulong> HasGuildIdStorage(this PropertyBuilder<ulong> builder)
    {
        return builder
            .HasConversion<decimal>()
            .HasColumnType(StoreType);
    }
}
