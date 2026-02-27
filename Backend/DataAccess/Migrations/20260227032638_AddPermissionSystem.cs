using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Section = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "DisplayName", "Section" },
                values: new object[,]
                {
                    { 1, "Read", "Mahsulotlarni ko'rish", "Products" },
                    { 2, "Create", "Mahsulot qo'shish", "Products" },
                    { 3, "Update", "Mahsulotni tahrirlash", "Products" },
                    { 4, "Delete", "Mahsulotni o'chirish", "Products" },
                    { 5, "AddStock", "Ombor qo'shish", "Products" },
                    { 6, "Read", "Kategoriyalarni ko'rish", "Categories" },
                    { 7, "Create", "Kategoriya qo'shish", "Categories" },
                    { 8, "Update", "Kategoriyani tahrirlash", "Categories" },
                    { 9, "Delete", "Kategoriyani o'chirish", "Categories" },
                    { 10, "Create", "Sotuv qilish", "Sales" },
                    { 11, "ViewHistory", "Sotuvlar tarixini ko'rish", "Sales" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Section_Action",
                table: "Permissions",
                columns: new[] { "Section", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "Permissions");
        }
    }
}
