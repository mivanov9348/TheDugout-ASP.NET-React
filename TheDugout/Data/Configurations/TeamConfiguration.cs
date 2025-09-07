using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.ToTable("Teams");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Abbreviation)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(t => t.Balance)
                   .HasColumnType("decimal(18,2)");

            builder.HasOne(e => e.Template)
                   .WithMany()
                   .HasForeignKey(e => e.TemplateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Country)
                   .WithMany()
                   .HasForeignKey(e => e.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.GameSave)
                   .WithMany(gs => gs.Teams)
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.League)
                   .WithMany(l => l.Teams)
                   .HasForeignKey(e => e.LeagueId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Players)
                   .WithOne(p => p.Team)
                   .HasForeignKey(p => p.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.HomeFixtures)
                   .WithOne(f => f.HomeTeam)
                   .HasForeignKey(f => f.HomeTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.AwayFixtures)
                   .WithOne(f => f.AwayTeam)
                   .HasForeignKey(f => f.AwayTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.TransactionsFrom)
                   .WithOne(ft => ft.FromTeam)
                   .HasForeignKey(ft => ft.FromTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.TransactionsTo)
                   .WithOne(ft => ft.ToTeam)
                   .HasForeignKey(ft => ft.ToTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 🆕 връзка 1:1 между Team и TeamTactic
            builder.HasOne(t => t.TeamTactic)
                   .WithOne(tt => tt.Team)
                   .HasForeignKey<TeamTactic>(tt => tt.TeamId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
