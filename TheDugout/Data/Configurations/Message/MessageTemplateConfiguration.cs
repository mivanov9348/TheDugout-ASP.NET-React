using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Messages;

namespace TheDugout.Data.Configurations
{
    public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
    {
        public void Configure(EntityTypeBuilder<MessageTemplate> builder)
        {
            builder.HasKey(mt => mt.Id);

            builder.Property(mt => mt.SubjectTemplate)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(mt => mt.BodyTemplate)
                .IsRequired();

            builder.Property(mt => mt.Category)
                .HasConversion<string>()
                .IsRequired();
           
        }
    }
}
