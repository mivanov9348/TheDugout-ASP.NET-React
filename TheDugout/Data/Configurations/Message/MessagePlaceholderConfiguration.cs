using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Messages;

namespace TheDugout.Data.Configurations
{
    public class MessagePlaceholderConfiguration : IEntityTypeConfiguration<MessagePlaceholder>
    {
        public void Configure(EntityTypeBuilder<MessagePlaceholder> builder)
        {
            builder.HasKey(mp => mp.Id);

            builder.Property(mp => mp.Key)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(mp => mp.Description)
                .HasMaxLength(500);
        }
    }
}
