using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class ProductBatchConfiguration : IEntityTypeConfiguration<ProductBatch>
{
    public void Configure(EntityTypeBuilder<ProductBatch> builder)
    {
        builder.HasKey(e => e.Id);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ProductBatches_BuyPrice_Positive", "\"BuyPrice\" > 0");
            t.HasCheckConstraint("CK_ProductBatches_OriginalQuantity_Positive", "\"OriginalQuantity\" > 0");
            t.HasCheckConstraint("CK_ProductBatches_RemainingQuantity_NonNegative", "\"RemainingQuantity\" >= 0");
        });

        builder.Property(e => e.BuyPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.OriginalQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.RemainingQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.ReceivedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
