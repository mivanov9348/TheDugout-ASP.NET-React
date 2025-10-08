using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Leagues;

namespace TheDugout.Data.Configurations
{
    public class LeagueConfiguration : IEntityTypeConfiguration<League>
    {
        public void Configure(EntityTypeBuilder<League> builder)
        {
            builder.ToTable("Leagues");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd(); 


            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Template)
                   .WithMany()
                   .HasForeignKey(e => e.TemplateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.GameSave)
                   .WithMany(gs => gs.Leagues)
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Season)
                   .WithMany(s => s.Leagues)
                   .HasForeignKey(e => e.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Country)
                   .WithMany()
                   .HasForeignKey(e => e.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Competition)
                .WithMany()
                .HasForeignKey(c => c.CompetitionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
