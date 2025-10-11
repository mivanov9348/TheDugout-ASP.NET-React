using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Messages;

namespace TheDugout.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Subject)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.Body)
                .IsRequired();

            builder.Property(m => m.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(m => m.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(m => m.Category)
                .HasConversion<string>() 
                .IsRequired();

            builder.HasOne(p => p.GameSave)
                   .WithMany(g => g.Messages)
                   .HasForeignKey(p => p.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.MessageTemplate)
                .WithMany()
                .HasForeignKey(m => m.MessageTemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
