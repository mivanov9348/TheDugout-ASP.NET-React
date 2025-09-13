using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Competitions;

namespace TheDugout.Data.Configurations
{
    public class EuropeanCupPhaseTemplateConfiguration : IEntityTypeConfiguration<EuropeanCupPhaseTemplate>
    {
        public void Configure(EntityTypeBuilder<EuropeanCupPhaseTemplate> builder)
        {
            builder.ToTable("EuropeanCupPhaseTemplates");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Order)
                   .IsRequired();

            builder.Property(e => e.IsKnockout)
                   .HasDefaultValue(false);

            builder.Property(e => e.IsTwoLegged)
                   .HasDefaultValue(false);

            builder.HasIndex(e => e.Order);
        }
    }
}
