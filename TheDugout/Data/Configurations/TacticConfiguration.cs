using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class TacticConfiguration : IEntityTypeConfiguration<Tactic>
    {
        public void Configure(EntityTypeBuilder<Tactic> builder)
        {
            builder.ToTable("Tactics");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(t => t.Defenders)
                   .IsRequired();

            builder.Property(t => t.Midfielders)
                   .IsRequired();

            builder.Property(t => t.Forwards)
                   .IsRequired();
        }
    }
}
