using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Facilities;

namespace TheDugout.Data.Configurations.Facilities
{
    public class YouthAcademyConfiguration : IEntityTypeConfiguration<YouthAcademy>
    {
        public void Configure(EntityTypeBuilder<YouthAcademy> builder)
        {
            builder.HasKey(ya => ya.Id);

            builder.HasOne(ya => ya.Team)
                   .WithOne(t => t.YouthAcademy)
                   .HasForeignKey<YouthAcademy>(ya => ya.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);
                
            builder.Property(ya => ya.Level)
                   .IsRequired();

        }
    }
}
