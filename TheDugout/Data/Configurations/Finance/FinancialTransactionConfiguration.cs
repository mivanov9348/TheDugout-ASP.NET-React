namespace TheDugout.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Finance;
    public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
    {
        public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
        {
            builder.HasKey(ft => ft.Id);

            builder.Property(ft => ft.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(ft => ft.Fee)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(ft => ft.FromTeam)
                .WithMany()
                .HasForeignKey(ft => ft.FromTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.ToTeam)
                .WithMany()
                .HasForeignKey(ft => ft.ToTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.FromAgency)
                .WithMany(a => a.TransactionsFrom)
                .HasForeignKey(ft => ft.FromAgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.ToAgency)
                .WithMany(a => a.TransactionsTo)
                .HasForeignKey(ft => ft.ToAgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.Bank)
                .WithMany(b => b.Transactions)
                .HasForeignKey(ft => ft.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.GameSave)
               .WithMany(b => b.FinancialTransactions)
               .HasForeignKey(ft => ft.GameSaveId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
