using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.RegularExpressions;
using TheDugout.Models.Matches;

namespace TheDugout.Data.Configurations.Matches
{
    public class MatchEventConfiguration : IEntityTypeConfiguration<MatchEvent>
    {
        public void Configure(EntityTypeBuilder<MatchEvent> builder)
        {
            builder.HasKey(me => me.Id);

            builder.HasOne(me => me.Match)
                .WithMany(m => m.Events)
                .HasForeignKey(me => me.MatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.GameSave)
                .WithMany(gs => gs.MatchEvents)
                .HasForeignKey(m => m.GameSaveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(me => me.Team)
                .WithMany(t => t.MatchEvents) 
                .HasForeignKey(me => me.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(me => me.Player)
                .WithMany(p => p.MatchEvents) 
                .HasForeignKey(me => me.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(me => me.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(me => me.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(me => me.Outcome)
                .WithMany(o => o.MatchEvents) 
                .HasForeignKey(me => me.OutcomeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
