using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.RegularExpressions;

namespace TheDugout.Data.Configurations.Matches
{
    public class MatchConfiguration : IEntityTypeConfiguration<Models.Matches.Match>
    {
        public void Configure(EntityTypeBuilder<Models.Matches.Match> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasOne(m => m.GameSave)
                .WithMany(gs => gs.Matches)
                .HasForeignKey(m => m.GameSaveId)
                .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(m => m.Fixture)
                .WithMany(f => f.Matches)
                .HasForeignKey(m => m.FixtureId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(m => m.Status)
                .HasConversion<int>();
        }
    }
}
