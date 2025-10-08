using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Players;

namespace TheDugout.Data.Configurations
{
    public class PositionWeightConfiguration : IEntityTypeConfiguration<PositionWeight>
    {
        public void Configure(EntityTypeBuilder<PositionWeight> builder)
        {
            builder.ToTable("PositionWeights");

            builder.HasKey(pw => pw.Id);

            builder.Property(pw => pw.Weight)
                   .IsRequired()
                   .HasPrecision(4, 2); 

            builder.HasOne(pw => pw.Position)
                   .WithMany(p => p.Weights)
                   .HasForeignKey(pw => pw.PositionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pw => pw.Attribute)
                   .WithMany(a => a.PositionWeights)
                   .HasForeignKey(pw => pw.AttributeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pw => new { pw.PositionId, pw.AttributeId })
                   .IsUnique();
        }
    }
}
