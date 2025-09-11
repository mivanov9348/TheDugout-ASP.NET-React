using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;
using System;

namespace TheDugout.Data.Configurations
{
    public class EuropeanCupMatchConfiguration : IEntityTypeConfiguration<EuropeanCupMatch>
    {
        public void Configure(EntityTypeBuilder<EuropeanCupMatch> builder)
        {
            builder.ToTable("EuropeanCupMatches");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Leg).HasDefaultValue(0);
            builder.Property(e => e.Status).HasDefaultValue(MatchStatus.Scheduled);
            builder.Property(e => e.MatchDate).IsRequired(false);

            builder.HasOne(e => e.Phase)
                   .WithMany(p => p.Matches)
                   .HasForeignKey(e => e.PhaseId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Home team relation
            builder.HasOne(e => e.HomeCupTeam)
                   .WithMany()
                   .HasForeignKey(e => e.HomeCupTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Away team relation
            builder.HasOne(e => e.AwayCupTeam)
                   .WithMany()
                   .HasForeignKey(e => e.AwayCupTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            // useful indexes for queries
            builder.HasIndex(e => e.PhaseId);
            builder.HasIndex(e => e.HomeCupTeamId);
            builder.HasIndex(e => e.AwayCupTeamId);
        }
    }
}
