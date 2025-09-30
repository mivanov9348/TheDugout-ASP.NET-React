using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Cups;

namespace TheDugout.Data.Configurations
{
    public class CupRoundConfiguration : IEntityTypeConfiguration<CupRound>
    {
        public void Configure(EntityTypeBuilder<CupRound> builder)
        {
            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(cr => cr.Cup)
                .WithMany(c => c.Rounds)
                .HasForeignKey(cr => cr.CupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
