using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Leagues;

namespace TheDugout.Data.Configurations
{
    public class LeagueTemplateConfiguration : IEntityTypeConfiguration<LeagueTemplate>
    {
        public void Configure(EntityTypeBuilder<LeagueTemplate> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.LeagueCode)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.HasOne(e => e.Country)
                   .WithMany(c => c.LeagueTemplates)
                   .HasForeignKey(e => e.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
