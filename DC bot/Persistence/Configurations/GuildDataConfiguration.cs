using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Persistence.Configurations;

public class GuildDataConfiguration : IEntityTypeConfiguration<GuildDataEntity>
{
    public void Configure(EntityTypeBuilder<GuildDataEntity> builder)
    {
        builder.ToTable("guild_data");

        builder.HasKey(entity => entity.GuildId);

        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .ValueGeneratedNever();

        builder.Property(entity => entity.IsPremium)
            .HasColumnName("is_premium")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(entity => entity.PremiumUntilUtc)
            .HasColumnName("premium_until_utc");

        builder.Property(entity => entity.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
