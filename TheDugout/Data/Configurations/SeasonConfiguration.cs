using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class SeasonConfiguration : IEntityTypeConfiguration<Season>
    {
        public void Configure(EntityTypeBuilder<Season> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.StartDate).IsRequired();
            builder.Property(e => e.EndDate).IsRequired();
            builder.Property(e => e.CurrentDate).IsRequired();

            builder.HasOne(e => e.GameSave)
                   .WithMany(gs => gs.Seasons)
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Events)
                   .WithOne(ev => ev.Season)
                   .HasForeignKey(ev => ev.SeasonId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.PlayerStats)
                   .WithOne(ps => ps.Season)
                   .HasForeignKey(ps => ps.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.HasMany(s => s.Fixtures)
                    .WithOne(f => f.Season)
                    .HasForeignKey(f => f.SeasonId)
                    .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
