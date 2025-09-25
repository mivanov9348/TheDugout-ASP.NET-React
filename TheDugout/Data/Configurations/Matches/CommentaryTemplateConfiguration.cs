using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Matches;

namespace TheDugout.Data.Configurations.Matches
{    public class CommentaryTemplateConfiguration : IEntityTypeConfiguration<CommentaryTemplate>
    {
        public void Configure(EntityTypeBuilder<CommentaryTemplate> builder)
        {
            builder.HasKey(ct => ct.Id);

            builder.Property(ct => ct.EventTypeCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ct => ct.OutcomeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ct => ct.Template)
                .IsRequired()
                .HasMaxLength(500);
        }
    }
}
