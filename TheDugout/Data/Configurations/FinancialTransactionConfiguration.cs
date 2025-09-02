using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
    {
        public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(255);

            builder.Property(t => t.Date)
                .IsRequired();

            builder.Property(t => t.Type)
                .HasConversion<int>() 
                .IsRequired();

            builder.HasOne(t => t.Team)
                .WithMany(team => team.Transactions)
                .HasForeignKey(t => t.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Bank)
                .WithMany(bank => bank.Transactions)
                .HasForeignKey(t => t.BankId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
