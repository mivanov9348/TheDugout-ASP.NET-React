using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
    {
        public void Configure(EntityTypeBuilder<MessageTemplate> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Category)
                   .IsRequired();

            builder.Property(e => e.SubjectTemplate)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.BodyTemplate)
                   .IsRequired();

            builder.Property(e => e.PlaceholdersJson)
                   .HasColumnType("nvarchar(max)");

            builder.Property(e => e.Weight)
                   .HasDefaultValue(1);

            builder.Property(e => e.IsActive)
                   .HasDefaultValue(true);

            builder.Property(e => e.Language)
                   .HasMaxLength(10)
                   .HasDefaultValue("en");

            builder.HasMany(e => e.Messages)
                   .WithOne(m => m.MessageTemplate)
                   .HasForeignKey(m => m.MessageTemplateId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(e => e.Placeholders)
                   .WithOne(p => p.MessageTemplate)
                   .HasForeignKey(p => p.MessageTemplateId);

            builder.HasIndex(e => new { e.Category, e.IsActive });
        }
    }
}
