using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.FirstName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(p => p.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasOne(p => p.Team)
       .WithMany(t => t.Players)
       .HasForeignKey(p => p.TeamId)
       .OnDelete(DeleteBehavior.SetNull); 


            builder.HasOne(p => p.GameSave)
       .WithMany(g => g.Players)   
       .HasForeignKey(p => p.GameSaveId)
       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
