using Microsoft.EntityFrameworkCore;
using TheDugout.Models;

namespace TheDugout.Data
{
    public class DugoutDbContext : DbContext
    {
        public DugoutDbContext(DbContextOptions<DugoutDbContext> options) : base(options)
        {
        }

        public DbSet<Bank> Banks { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<LeagueTemplate> LeagueTemplates { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<TeamTemplate> TeamTemplates { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<GameSave> GameSaves { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<MessageTemplatePlaceholder> MessageTemplatePlaceholders { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<SeasonEvent> SeasonEvents { get; set; }
        public DbSet<PlayerMatchStats> PlayerMatchStats { get; set; }
        public DbSet<PlayerSeasonStats> PlayerSeasonStats { get; set; }
        public DbSet<PlayerAttribute> PlayerAttributes { get; set; }
        public DbSet<Models.Attribute> Attributes { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<PositionWeight> PositionWeights { get; set; }  
        public DbSet<Fixture> Fixtures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DugoutDbContext).Assembly);
        }
    }
}
