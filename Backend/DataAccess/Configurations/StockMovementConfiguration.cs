using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(e => e.Id);

        // Table configuration with check constraint
        builder.ToTable(t => t.HasCheckConstraint("CK_StockMovements_Quantity_Positive", "\"Quantity\" > 0"));

        builder.Property(e => e.Quantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.MovementType)
            .IsRequired();

        builder.Property(e => e.MovementDate)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign key relationship with Product
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign key relationship with User
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.MovementType);
        builder.HasIndex(e => new { e.ProductId, e.MovementType, e.MovementDate });
        // Report filtering by user + date range
        builder.HasIndex(e => new { e.UserId, e.MovementType, e.MovementDate })
            .HasDatabaseName("IX_StockMovements_UserId_Type_Date");
    }
}
