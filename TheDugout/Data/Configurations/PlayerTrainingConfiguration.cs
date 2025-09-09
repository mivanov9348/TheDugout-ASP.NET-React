using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class PlayerTrainingConfiguration : IEntityTypeConfiguration<PlayerTraining>
    {
        public void Configure(EntityTypeBuilder<PlayerTraining> builder)
        {
            builder.HasKey(pt => pt.Id);

            builder.HasOne(pt => pt.TrainingSession)
                   .WithMany(ts => ts.PlayerTrainings)
                   .HasForeignKey(pt => pt.TrainingSessionId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(pt => pt.Player)
                   .WithMany()
                   .HasForeignKey(pt => pt.PlayerId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(pt => pt.Attribute)
                   .WithMany()
                   .HasForeignKey(pt => pt.AttributeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(pt => pt.ChangeValue)
                   .IsRequired();
        }
    }
}
