using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Matches;

namespace TheDugout.Data.Configurations.Matches
{
    public class EventAttributeWeightConfiguration : IEntityTypeConfiguration<EventAttributeWeight>
    {
        public void Configure(EntityTypeBuilder<EventAttributeWeight> builder)
        {
            builder.HasKey(eaw => eaw.Id);

            builder.Property(eaw => eaw.EventTypeCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(eaw => eaw.AttributeCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(eaw => eaw.Weight)
                .IsRequired();

            builder.HasOne(eaw => eaw.EventType)
                .WithMany(et => et.AttributeWeights)
                .HasForeignKey(eaw => eaw.EventTypeCode)
                .HasPrincipalKey(et => et.Code);
        }
    }
}
