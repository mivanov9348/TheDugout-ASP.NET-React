using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Common;
using TheDugout.Models.Cups;

namespace TheDugout.Data.Configurations
{
    public class CupConfiguration : IEntityTypeConfiguration<Cup>
    {
        public void Configure(EntityTypeBuilder<Cup> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Template)
                   .WithMany()
                   .HasForeignKey(c => c.TemplateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.GameSave)
                   .WithMany(gs => gs.Cups)
                   .HasForeignKey(c => c.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Season)
                   .WithMany(s => s.Cups)
                   .HasForeignKey(c => c.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Competition)
                   .WithOne(cu => cu.Cup)
                   .HasForeignKey<Competition>(c => c.CupId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Country)
                   .WithMany()
                   .HasForeignKey(c => c.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);
           
        }

    }
}
