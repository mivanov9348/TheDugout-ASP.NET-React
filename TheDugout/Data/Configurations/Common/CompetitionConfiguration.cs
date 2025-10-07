using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Common;

namespace TheDugout.Data.Configurations.Common
{
    public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
    {
        public void Configure(EntityTypeBuilder<Competition> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
       .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Type)
                   .IsRequired();

            builder.HasOne(e => e.Season)
                   .WithMany()
                   .HasForeignKey(e => e.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.GameSave)
                   .WithMany()
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
