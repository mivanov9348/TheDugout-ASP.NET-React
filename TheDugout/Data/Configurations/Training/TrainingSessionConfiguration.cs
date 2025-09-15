using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Training;

namespace TheDugout.Data.Configurations
{
    public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
    {
        public void Configure(EntityTypeBuilder<TrainingSession> builder)
        {
            builder.HasKey(ts => ts.Id);

            builder.HasOne(ts => ts.GameSave)
                   .WithMany(gs => gs.TrainingSessions)
                   .HasForeignKey(ts => ts.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ts => ts.Team)
                   .WithMany(t => t.TrainingSessions)
                   .HasForeignKey(ts => ts.TeamId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(ts => ts.Season)
                   .WithMany(s => s.TrainingSessions)
                   .HasForeignKey(ts => ts.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(ts => ts.Date)
                   .IsRequired();
        }
    }
}
