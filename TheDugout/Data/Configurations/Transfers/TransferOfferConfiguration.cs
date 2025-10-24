using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Transfers;

public class TransferOfferConfiguration : IEntityTypeConfiguration<TransferOffer>
{
    public void Configure(EntityTypeBuilder<TransferOffer> builder)
    {
        builder.ToTable("TransferOffers");

        builder.HasKey(to => to.Id);

        builder.Property(to => to.OfferAmount)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(to => to.Status)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(to => to.CreatedAt)
               .IsRequired();

        builder.HasOne(to => to.GameSave)
               .WithMany(gs => gs.TransferOffers)
               .HasForeignKey(to => to.GameSaveId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(to => to.FromTeam)
               .WithMany(t => t.SentTransferOffers)
               .HasForeignKey(to => to.FromTeamId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(to => to.ToTeam)
               .WithMany(t => t.ReceivedTransferOffers)
               .HasForeignKey(to => to.ToTeamId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(to => to.Player)
               .WithMany(p => p.TransferOffers)
               .HasForeignKey(to => to.PlayerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(to => to.Season)
              .WithMany(p => p.TransferOffers)
              .HasForeignKey(to => to.SeasonId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
