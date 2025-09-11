using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class EuropeanCupConfiguration : IEntityTypeConfiguration<EuropeanCup>
    {
        public void Configure(EntityTypeBuilder<EuropeanCup> builder)
        {
            builder.ToTable("EuropeanCups");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.GameSaveId).IsRequired();
            builder.Property(e => e.TemplateId).IsRequired();

            builder.HasOne(e => e.Template)
                   .WithMany() // templates don't necessarily have collection of cups
                   .HasForeignKey(e => e.TemplateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.GameSave)
                   .WithMany() // add WithMany(gs => gs.EuropeanCups) if GameSave has nav
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Teams)
                   .WithOne(t => t.EuropeanCup)
                   .HasForeignKey(t => t.EuropeanCupId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Phases)
                   .WithOne(p => p.EuropeanCup)
                   .HasForeignKey(p => p.EuropeanCupId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Standings)
                   .WithOne(s => s.EuropeanCup)
                   .HasForeignKey(s => s.EuropeanCupId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
