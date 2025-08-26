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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Country
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);
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

                entity.HasOne<League>()
                      .WithMany(l => l.Teams)
                      .HasForeignKey("LeagueId")
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Players)
                      .WithOne(p => p.Team)
                      .HasForeignKey(p => p.TeamId);
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Position).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.Team)
                      .WithMany(t => t.Players)
                      .HasForeignKey(e => e.TeamId);

                entity.HasOne(e => e.Country)
                      .WithMany(c => c.Players)
                      .HasForeignKey(e => e.CountryId);
            });

            // GameSave
            modelBuilder.Entity<GameSave>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.GameSaves)
                      .HasForeignKey(e => e.UserId)
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
        }

    }
}
