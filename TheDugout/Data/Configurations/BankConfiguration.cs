using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Finance;

namespace TheDugout.Data.Configurations
{
    public class BankConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.GameSave)
                .WithOne(gs => gs.Bank)
                .HasForeignKey<Bank>(b => b.GameSaveId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(b => b.Balance)
                .HasColumnType("decimal(18,2)");
        }
    }
}
