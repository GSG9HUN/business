using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Persistence.Configurations;

public class GuildPremiumAuditConfiguration : IEntityTypeConfiguration<GuildPremiumAuditEntity>
{
    public void Configure(EntityTypeBuilder<GuildPremiumAuditEntity> builder)
    {
        builder.ToTable("guild_premium_audit");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.Property(entity => entity.GuildId)
            .HasColumnName("guild_id")
            .IsRequired();

        builder.Property(entity => entity.ChangedByUserId)
            .HasColumnName("changed_by_user_id");

        builder.Property(entity => entity.OldIsPremium)
            .HasColumnName("old_is_premium")
            .IsRequired();

        builder.Property(entity => entity.NewIsPremium)
            .HasColumnName("new_is_premium")
            .IsRequired();

        builder.Property(entity => entity.ChangedAtUtc)
            .HasColumnName("changed_at_utc")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(entity => entity.Note)
            .HasColumnName("note");

        builder.HasIndex(entity => new { entity.GuildId, entity.ChangedAtUtc });

        builder.HasOne(entity => entity.Guild)
            .WithMany(guild => guild.PremiumAuditEntries)
            .HasForeignKey(entity => entity.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
