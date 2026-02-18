using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.HasKey(e => e.Id);

        // Table configuration with check constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_SaleItems_Quantity_Positive", "\"Quantity\" > 0");
            t.HasCheckConstraint("CK_SaleItems_UnitPrice_Positive", "\"UnitPrice\" > 0");
        });

        builder.Property(e => e.Quantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        // Foreign key relationship with Sale
        builder.HasOne(e => e.Sale)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(e => e.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key relationship with Product
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reporting queries aggregate by product within a sale
        builder.HasIndex(e => new { e.SaleId, e.ProductId });
    }
}
