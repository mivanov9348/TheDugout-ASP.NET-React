using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Competitions;

namespace TheDugout.Data.Configurations
{
    public class EuropeanCupTemplateConfiguration : IEntityTypeConfiguration<EuropeanCupTemplate>
    {
        public void Configure(EntityTypeBuilder<EuropeanCupTemplate> builder)
        {
            builder.ToTable("EuropeanCupTemplates");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(e => e.TeamsCount)
                   .IsRequired();

            builder.Property(e => e.LeaguePhaseMatchesPerTeam)
                   .IsRequired();           

            builder.HasMany(e => e.PhaseTemplates)
                   .WithOne()
                   .HasForeignKey("EuropeanCupTemplateId")
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
