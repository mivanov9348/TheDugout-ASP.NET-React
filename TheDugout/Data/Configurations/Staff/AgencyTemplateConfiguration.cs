using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Staff;

namespace TheDugout.Data.Configurations
{

    public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
    {
        public void Configure(EntityTypeBuilder<Agency> builder)
        {
            builder.ToTable("Agencies");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Budget)
                .HasColumnType("decimal(18,2)");

            builder.Property(a => a.TotalEarnings)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(a => a.Logo)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(a => a.AgencyTemplate)
                .WithMany()
                .HasForeignKey(a => a.AgencyTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.GameSave)
                .WithMany(gs => gs.Agencies)
                .HasForeignKey(a => a.GameSaveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Region)
                .WithMany(r => r.Agencies)
                .HasForeignKey(a => a.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

