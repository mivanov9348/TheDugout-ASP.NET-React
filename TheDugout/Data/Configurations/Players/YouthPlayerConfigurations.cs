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

            // One-to-one с Player
            builder.HasOne(y => y.Player)
                   .WithOne(p => p.YouthProfile)
                   .HasForeignKey<YouthPlayer>(y => y.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-many с YouthAcademy
            builder.HasOne(y => y.YouthAcademy)
                   .WithMany(a => a.YouthPlayers)
                   .HasForeignKey(y => y.YouthAcademyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(y => y.IsPromoted)
                   .IsRequired();
        }
    }

}
