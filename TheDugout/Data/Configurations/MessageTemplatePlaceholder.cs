using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Messages;

namespace TheDugout.Data.Configurations
{
    public class MessageTemplatePlaceholderConfiguration : IEntityTypeConfiguration<MessageTemplatePlaceholder>
    {
        public void Configure(EntityTypeBuilder<MessageTemplatePlaceholder> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(e => new { e.MessageTemplateId, e.Name })
                   .IsUnique();
        }
    }
}
