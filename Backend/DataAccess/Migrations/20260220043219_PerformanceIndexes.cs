using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockMovements_UserId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_ProductBatches_ProductId_RemainingQuantity",
                table: "ProductBatches");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_UserId_Type_Date",
                table: "StockMovements",
                columns: new[] { "UserId", "MovementType", "MovementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_FIFO",
                table: "ProductBatches",
                columns: new[] { "ProductId", "RemainingQuantity", "ReceivedAt" });

            // Case-insensitive prefix search uchun functional indexlar (ILike / LOWER() ni qo'llab-quvvatlaydi)
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Products_Name_Lower"" ON ""Products"" (LOWER(""Name"") text_pattern_ops);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Products_Barcode_Lower"" ON ""Products"" (LOWER(""barcode"") text_pattern_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockMovements_UserId_Type_Date",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_ProductBatches_FIFO",
                table: "ProductBatches");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_UserId",
                table: "StockMovements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_ProductId_RemainingQuantity",
                table: "ProductBatches",
                columns: new[] { "ProductId", "RemainingQuantity" });

            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Products_Name_Lower"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Products_Barcode_Lower"";");
        }
    }
}
