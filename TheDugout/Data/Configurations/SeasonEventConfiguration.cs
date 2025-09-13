using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Seasons;

namespace TheDugout.Data.Configurations
{
    public class SeasonEventConfiguration : IEntityTypeConfiguration<SeasonEvent>
    {
        public void Configure(EntityTypeBuilder<SeasonEvent> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Date).IsRequired();

            builder.Property(e => e.Type)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(e => e.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.HasOne(e => e.Season)
                   .WithMany(s => s.Events)
                   .HasForeignKey(e => e.SeasonId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
