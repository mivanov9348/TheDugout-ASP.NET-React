using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Matches;

namespace TheDugout.Data.Configurations.Matches
{
    public class EventOutcomeConfiguration : IEntityTypeConfiguration<EventOutcome>
    {
        public void Configure(EntityTypeBuilder<EventOutcome> builder)
        {
            builder.HasKey(eo => eo.Id);

            builder.HasOne(eo => eo.EventType)
                .WithMany(et => et.Outcomes)
                .HasForeignKey(eo => eo.EventTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(eo => eo.Name)
                .IsRequired()
                .HasMaxLength(100);

        }
    }

}
