namespace TheDugout.Data.Configurations.Players
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Players;

    public class YouthPlayerConfiguration : IEntityTypeConfiguration<YouthPlayer>
    {
        public void Configure(EntityTypeBuilder<YouthPlayer> builder)
        {
            builder.HasKey(y => y.Id);

            builder.HasOne(y => y.Player)
                   .WithOne(p => p.YouthProfile)
                   .HasForeignKey<YouthPlayer>(y => y.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(y => y.YouthAcademy)
                   .WithMany(a => a.YouthPlayers)
                   .HasForeignKey(y => y.YouthAcademyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(y => y.GameSave)
                   .WithMany(a => a.YouthPlayers)
                   .HasForeignKey(y => y.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(y => y.IsPromoted)
                   .IsRequired();
        }
    }

}
