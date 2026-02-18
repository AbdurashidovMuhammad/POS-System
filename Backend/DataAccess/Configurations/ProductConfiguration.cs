using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);

        // Table configuration with check constraint
        builder.ToTable(t => t.HasCheckConstraint("CK_Products_SellPrice_Positive", "\"SellPrice\" > 0"));

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Name unique constraint
        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("IX_Products_Name_Unique");

        builder.Property(e => e.barcode)
            .IsRequired()
            .HasMaxLength(100);

        // Barcode unique constraint
        builder.HasIndex(e => e.barcode)
            .IsUnique()
            .HasDatabaseName("IX_Products_Barcode_Unique");

        builder.Property(e => e.SellPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Unit_Type)
            .IsRequired();

        builder.Property(e => e.StockQuantity)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign key relationship with Category
        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
