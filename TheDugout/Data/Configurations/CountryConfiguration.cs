using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Code).IsUnique();
            builder.HasIndex(e => e.Name).IsUnique();

            builder.Property(e => e.Code)
                   .IsRequired()
                   .HasMaxLength(3);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(e => e.TeamTemplates)
                   .WithOne(t => t.Country)
                   .HasForeignKey(t => t.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.LeagueTemplates)
                   .WithOne(l => l.Country)
                   .HasForeignKey(l => l.CountryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Players)
                   .WithOne(p => p.Country)
                   .HasForeignKey(p => p.CountryId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
