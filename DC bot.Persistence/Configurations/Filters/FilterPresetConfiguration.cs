using DC_bot.Configurations.Shared;
using DC_bot.Entities.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DC_bot.Configurations.Filters;

/// <summary>Maps guild-local versioned filter presets to PostgreSQL.</summary>
public sealed class FilterPresetConfiguration : IEntityTypeConfiguration<FilterPresetEntity>
{

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FilterPresetEntity> builder)
    {
        builder.ToTable("filter_preset");
        builder.HasKey(item => new { item.GuildId, item.Name });
        builder.Property(item => item.GuildId)
            .HasColumnName("guild_id")
            .HasGuildIdStorage()
            .ValueGeneratedNever();
        builder.Property(item => item.Name)
            .HasColumnName("name")
            .HasMaxLength(32);
        builder.Property(item => item.SchemaVersion)
            .HasColumnName("schema_version");
        builder.Property(item => item.DefinitionJson)
            .HasColumnName("definition")
            .HasColumnType("jsonb");
        builder.Property(item => item.UpdatedBy)
            .HasColumnName("updated_by")
            .HasGuildIdStorage();
        builder.Property(item => item.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");
    }
}
