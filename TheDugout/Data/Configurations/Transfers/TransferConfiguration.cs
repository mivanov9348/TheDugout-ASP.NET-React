namespace TheDugout.Data.Configurations.Transfers
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Transfers;

    public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
    {
        public void Configure(EntityTypeBuilder<Transfer> builder)
        {
            builder.HasKey(t => t.Id);

            builder.HasOne(t => t.GameSave)
                .WithMany()
                .HasForeignKey(t => t.GameSaveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Season)
                .WithMany()
                .HasForeignKey(t => t.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Player)
                .WithMany()
                .HasForeignKey(t => t.PlayerId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(t => t.FromTeam)
                .WithMany()
                .HasForeignKey(t => t.FromTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ToTeam)
                .WithMany()
                .HasForeignKey(t => t.ToTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
