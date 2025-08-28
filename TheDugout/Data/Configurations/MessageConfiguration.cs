using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Subject)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Body)
                   .IsRequired();

            builder.Property(e => e.IsRead)
                   .HasDefaultValue(false);

            builder.Property(e => e.Date)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(m => m.GameSave)
                   .WithMany(gs => gs.Messages)
                   .HasForeignKey(m => m.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired(false);
        }
    }
}
