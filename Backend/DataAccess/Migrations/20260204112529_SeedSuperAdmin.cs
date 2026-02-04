using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "IsActive", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2026, 2, 4, 0, 0, 0, 0, DateTimeKind.Utc), true, "$2a$11$TjYDrGYAX3yCCQwH2yE7XuU5tWgDTl7DhwU3uWxOZxOyW5ukOeQOK", 1, "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
