namespace TheDugout.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    public class AttributeConfiguration : IEntityTypeConfiguration<Models.Players.AttributeDefinition>
    {
        public void Configure(EntityTypeBuilder<Models.Players.AttributeDefinition> builder)
        {
            builder.ToTable("Attributes");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Code)
                   .IsRequired();

            builder.Property(a => a.Name)
                   .IsRequired();

            builder.HasIndex(a => a.Code)
                   .IsUnique(); 
        }
    }
}
