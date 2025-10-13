namespace TheDugout.Data.Configurations.Common
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Common;

    public class GameSettingsConfiguration : IEntityTypeConfiguration<GameSetting>
    {
        public void Configure(EntityTypeBuilder<GameSetting> builder)
        {
            builder.ToTable("GameSettings");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Key)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Value)
                .IsRequired()
                .HasMaxLength(255);

        }
    }
}
