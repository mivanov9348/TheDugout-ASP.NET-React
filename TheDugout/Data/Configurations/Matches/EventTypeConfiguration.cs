using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Matches;

namespace TheDugout.Data.Configurations.Matches
{
    public class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
    {
        public void Configure(EntityTypeBuilder<EventType> builder)
        {
            builder.HasKey(et => et.Id);

            builder.HasIndex(et => et.Code).IsUnique();

            builder.Property(et => et.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(et => et.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasMany(et => et.AttributeWeights)
                .WithOne(eaw => eaw.EventType)
                .HasForeignKey(eaw => eaw.EventTypeCode)
                .HasPrincipalKey(et => et.Code);
        }
    }

}
