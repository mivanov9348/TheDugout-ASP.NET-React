using Microsoft.EntityFrameworkCore;
using TheDugout.Models;

namespace TheDugout.Data
{
    public class DugoutDbContext : DbContext
    {
        public DugoutDbContext(DbContextOptions<DugoutDbContext> options) : base(options)
        {
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<LeagueTemplate> LeagueTemplates { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<TeamTemplate> TeamTemplates { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<GameSave> GameSaves { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<SeasonEvent> SeasonEvents { get; set; }
        public DbSet<PlayerMatchStats> PlayerMatchStats { get; set; }
        public DbSet<PlayerSeasonStats> PlayerSeasonStats { get; set; }
        public DbSet<PlayerAttributes> PlayerAttributes { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Country
                modelBuilder.Entity<Country>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.HasIndex(e => e.Code).IsUnique();
                    entity.HasIndex(e => e.Name).IsUnique();

                    entity.Property(e => e.Code)
                          .IsRequired()
                          .HasMaxLength(3);

                    entity.Property(e => e.Name)
                          .IsRequired()
                          .HasMaxLength(100);

                    entity.HasMany(e => e.TeamTemplates)
          .WithOne(t => t.Country)
          .HasForeignKey(t => t.CountryId)
          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasMany(e => e.LeagueTemplates)
          .WithOne(l => l.Country)
          .HasForeignKey(l => l.CountryId)
          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasMany(e => e.Players)
                          .WithOne(p => p.Country)
                          .HasForeignKey(p => p.CountryId)
                          .OnDelete(DeleteBehavior.SetNull);
                });


                // LeagueTemplate
                modelBuilder.Entity<LeagueTemplate>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Name)
                          .IsRequired()
                          .HasMaxLength(100);
                    entity.Property(e => e.LeagueCode)
                          .IsRequired()
                          .HasMaxLength(10);

                    entity.HasOne(e => e.Country)
                          .WithMany(c => c.LeagueTemplates)
                          .HasForeignKey(e => e.CountryId)
                          .OnDelete(DeleteBehavior.Cascade);

                });

                // League
                modelBuilder.Entity<League>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.HasOne(e => e.Template)
                          .WithMany()
                          .HasForeignKey(e => e.TemplateId)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.GameSave)
                          .WithMany(gs => gs.Leagues)
                          .HasForeignKey(e => e.GameSaveId)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.Country)
                          .WithMany()
                          .HasForeignKey(e => e.CountryId)
                          .OnDelete(DeleteBehavior.Restrict);
                });

                // TeamTemplate
                modelBuilder.Entity<TeamTemplate>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.Property(e => e.Name)
                          .IsRequired()
                          .HasMaxLength(100);

                    entity.Property(e => e.Abbreviation)
                          .IsRequired()
                          .HasMaxLength(10);

                    entity.HasOne(e => e.Country)
                          .WithMany(c => c.TeamTemplates)
                          .HasForeignKey(e => e.CountryId)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.League)
          .WithMany(l => l.TeamTemplates)
          .HasForeignKey(e => e.LeagueId)
          .OnDelete(DeleteBehavior.Restrict);
                });


                // Team
                modelBuilder.Entity<Team>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                    entity.Property(e => e.Abbreviation).IsRequired().HasMaxLength(10);

                    entity.HasOne(e => e.Template)
                          .WithMany()
                          .HasForeignKey(e => e.TemplateId)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.Country)
                          .WithMany()
                          .HasForeignKey(e => e.CountryId)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.GameSave)
                          .WithMany(gs => gs.Teams)
                          .HasForeignKey(e => e.GameSaveId)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.League)
                          .WithMany(l => l.Teams)
                          .HasForeignKey(e => e.LeagueId)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasMany(e => e.Players)
      .WithOne(p => p.Team)
      .HasForeignKey(p => p.TeamId)
      .OnDelete(DeleteBehavior.Restrict);
                });

            // Player

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Team)
                      .WithMany(t => t.Players)
                      .HasForeignKey(e => e.TeamId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Country)
                      .WithMany(c => c.Players)
                      .HasForeignKey(e => e.CountryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.GameSave)
                      .WithMany(gs => gs.Players)
                      .HasForeignKey(e => e.GameSaveId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Attributes)
                      .WithOne(a => a.Player)
                      .HasForeignKey<PlayerAttributes>(a => a.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.MatchStats)
                      .WithOne(ms => ms.Player)
                      .HasForeignKey(ms => ms.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.SeasonStats)
                      .WithOne(ss => ss.Player)
                      .HasForeignKey(ss => ss.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Position)
                      .WithMany(p => p.Players)
                      .HasForeignKey(e => e.PositionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PlayerAttributes
            modelBuilder.Entity<PlayerAttributes>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.HasOne(e => e.Player)
                          .WithOne(p => p.Attributes)
                          .HasForeignKey<PlayerAttributes>(e => e.PlayerId)
                          .OnDelete(DeleteBehavior.Cascade);
                });

                // PlayerMatchStats
                modelBuilder.Entity<PlayerMatchStats>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.HasOne(e => e.Player)
                          .WithMany(p => p.MatchStats)
                          .HasForeignKey(e => e.PlayerId)
                          .OnDelete(DeleteBehavior.Cascade);
                });

            // PlayerSeasonStats
            modelBuilder.Entity<PlayerSeasonStats>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Player)
                      .WithMany(p => p.SeasonStats)
                      .HasForeignKey(e => e.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Season)
                      .WithMany(s => s.PlayerStats)
                      .HasForeignKey(e => e.SeasonId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            // Position
            modelBuilder.Entity<Position>(entity =>
                {
                    entity.HasKey(p => p.Id);

                    entity.Property(p => p.Code).IsRequired().HasMaxLength(5);

                    entity.Property(p => p.Name).IsRequired().HasMaxLength(50);

                    entity.HasMany(p => p.Players)
                          .WithOne(pl => pl.Position)
                          .HasForeignKey(pl => pl.PositionId)
                          .OnDelete(DeleteBehavior.Restrict);
                });


                // GameSave
                modelBuilder.Entity<GameSave>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.HasOne(e => e.User)
                          .WithMany(u => u.GameSaves)
                          .HasForeignKey(e => e.UserId)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.UserTeam)
                          .WithMany()
                          .HasForeignKey(e => e.UserTeamId)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasMany(e => e.Messages)
                          .WithOne(m => m.GameSave)
                          .HasForeignKey(m => m.GameSaveId)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasMany(e => e.Seasons)
                          .WithOne(s => s.GameSave)
                          .HasForeignKey(s => s.GameSaveId)
                          .OnDelete(DeleteBehavior.Cascade);
                });

            //Season
            modelBuilder.Entity<Season>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.CurrentDate).IsRequired();

                entity.HasOne(e => e.GameSave)
                      .WithMany(gs => gs.Seasons)
                      .HasForeignKey(e => e.GameSaveId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Events)
                      .WithOne(ev => ev.Season)
                      .HasForeignKey(ev => ev.SeasonId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.PlayerStats)
                      .WithOne(ps => ps.Season)
                      .HasForeignKey(ps => ps.SeasonId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });


            // SeasonEvent
            modelBuilder.Entity<SeasonEvent>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.Property(e => e.Date).IsRequired();

                    entity.Property(e => e.Type)
                          .IsRequired()
                          .HasMaxLength(50);

                    entity.Property(e => e.Description)
                          .IsRequired()
                          .HasMaxLength(500);

                    entity.HasOne(e => e.Season)
                          .WithMany(s => s.Events)
                          .HasForeignKey(e => e.SeasonId)
                          .OnDelete(DeleteBehavior.Cascade);
                });

                // User
                modelBuilder.Entity<User>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Username)
                          .IsRequired()
                          .HasMaxLength(50);
                    entity.Property(e => e.Email)
                          .IsRequired()
                          .HasMaxLength(100);
                });

                // Message
                modelBuilder.Entity<Message>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.Property(e => e.Subject)
                          .IsRequired()
                          .HasMaxLength(100);

                    entity.Property(e => e.Body)
                          .IsRequired();

                    entity.Property(e => e.IsRead)
                          .HasDefaultValue(false);

                    entity.Property(e => e.Date)
                          .HasDefaultValueSql("GETUTCDATE()");

                    entity.HasOne(m => m.GameSave)
                          .WithMany(gs => gs.Messages)
                          .HasForeignKey(m => m.GameSaveId)
                          .OnDelete(DeleteBehavior.Cascade)
                          .IsRequired(false);
                });
            }

    }
}
