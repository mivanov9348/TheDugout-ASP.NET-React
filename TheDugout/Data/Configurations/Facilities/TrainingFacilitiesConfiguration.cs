using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Facilities;

namespace TheDugout.Data.Configurations.Facilities
{
    public class TrainingFacilityConfiguration : IEntityTypeConfiguration<TrainingFacility>
    {
        public void Configure(EntityTypeBuilder<TrainingFacility> builder)
        {
            builder.HasKey(tf => tf.Id);

            builder.HasOne(tf => tf.Team)
                   .WithOne(t => t.TrainingFacility)
                   .HasForeignKey<TrainingFacility>(tf => tf.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(tf => tf.Level)
                   .IsRequired();

            builder.Property(tf => tf.TrainingQuality)
                   .IsRequired();
        }
    }
}
