namespace TheDugout.Data.Configurations.Facilities
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Facilities;
    public class YouthAcademyConfiguration : IEntityTypeConfiguration<YouthAcademy>
    {
        public void Configure(EntityTypeBuilder<YouthAcademy> builder)
        {
            builder.HasKey(ya => ya.Id);

            builder.HasOne(ya => ya.Team)
                   .WithOne(t => t.YouthAcademy)
                   .HasForeignKey<YouthAcademy>(ya => ya.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.GameSave)
                   .WithMany() 
                   .HasForeignKey(l => l.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(ya => ya.Level)
                   .IsRequired();

        }
    }
}
