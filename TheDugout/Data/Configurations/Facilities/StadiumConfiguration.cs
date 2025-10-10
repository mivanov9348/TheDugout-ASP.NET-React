namespace TheDugout.Data.Configurations.Facilities
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Facilities;
    public class StadiumConfiguration : IEntityTypeConfiguration<Stadium>
    {
        public void Configure(EntityTypeBuilder<Stadium> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Team)
                   .WithOne(t => t.Stadium)
                   .HasForeignKey<Stadium>(s => s.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.GameSave)
                   .WithMany() // няма колекция в GameSave
                   .HasForeignKey(l => l.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.Level)
                   .IsRequired();

            builder.Property(s => s.Capacity)
                   .IsRequired();

            builder.Property(s => s.TicketPrice)
                   .HasColumnType("decimal(10,2)")
                   .IsRequired();
        }
    }
}