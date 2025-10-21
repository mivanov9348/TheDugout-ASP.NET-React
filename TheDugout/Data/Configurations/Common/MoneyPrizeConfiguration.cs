namespace TheDugout.Data.Configurations.Common
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Common;

    public class MoneyPrizeConfiguration : IEntityTypeConfiguration<MoneyPrize>
    {
        public void Configure(EntityTypeBuilder<MoneyPrize> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Code).IsUnique();

            builder.Property(e => e.Code)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(e => e.Amount)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(e => e.Description)
                   .HasMaxLength(500);

            builder.Property(e => e.IsActive)
                   .HasDefaultValue(true);

        }
    }
}