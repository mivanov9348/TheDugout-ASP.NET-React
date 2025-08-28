using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class TeamTemplateConfiguration : IEntityTypeConfiguration<TeamTemplate>
    {
        public void Configure(EntityTypeBuilder<TeamTemplate> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Abbreviation)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.HasOne(e => e.Country)
                   .WithMany(c => c.TeamTemplates)
                   .HasForeignKey(e => e.CountryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.League)
                   .WithMany(l => l.TeamTemplates)
                   .HasForeignKey(e => e.LeagueId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
