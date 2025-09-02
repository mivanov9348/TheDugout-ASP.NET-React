using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class BankConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Balance)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasMany(b => b.Transactions)
                .WithOne(t => t.Bank)
                .HasForeignKey(t => t.BankId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
