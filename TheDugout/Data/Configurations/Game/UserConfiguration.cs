using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Game;

namespace TheDugout.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Username)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(e => e.Email)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(u => u.CurrentSave)
                   .WithMany()
                   .HasForeignKey(u => u.CurrentSaveId)
                   .OnDelete(DeleteBehavior.Restrict); // ❌ вместо SetNull
        }
    }
}
