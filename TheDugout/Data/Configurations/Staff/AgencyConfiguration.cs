using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Staff;

namespace TheDugout.Data.Configurations
{
    public class AgencyTemplateConfiguration : IEntityTypeConfiguration<AgencyTemplate>
    {
        public void Configure(EntityTypeBuilder<AgencyTemplate> builder)
        {
            builder.ToTable("AgencyTemplates");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);            

            builder.Property(a => a.RegionCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);
        }
    }
}
