using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Common;

namespace TheDugout.Data.Configurations.Common
{
    public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
    {
        public void Configure(EntityTypeBuilder<Competition> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Type)
                   .IsRequired();

            builder.HasOne(c => c.Season)
                   .WithMany(s => s.Competitions)
                   .HasForeignKey(c => c.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Matches)
                   .WithOne(m => m.Competition)
                   .HasForeignKey(m => m.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.PlayerStats) 
                   .WithOne(ps => ps.Competition)
                   .HasForeignKey(ps => ps.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
