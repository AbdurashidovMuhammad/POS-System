using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Name unique constraint
        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("IX_Categories_Name_Unique");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
