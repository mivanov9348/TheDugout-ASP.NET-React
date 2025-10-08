using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Competitions;

namespace TheDugout.Data.Configurations
{
    public class EuropeanCupPhaseConfiguration : IEntityTypeConfiguration<EuropeanCupPhase>
    {
        public void Configure(EntityTypeBuilder<EuropeanCupPhase> builder)
        {
            builder.ToTable("EuropeanCupPhases");

            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.EuropeanCup)
                   .WithMany(c => c.Phases)
                   .HasForeignKey(e => e.EuropeanCupId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.PhaseTemplate)
                   .WithMany()
                   .HasForeignKey(e => e.PhaseTemplateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Fixtures)
                   .WithOne(f => f.EuropeanCupPhase)
                   .HasForeignKey(f => f.EuropeanCupPhaseId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
