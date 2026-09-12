using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageCategoryColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "StorageCategories",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StorageCategories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "Color",
                value: null);

            migrationBuilder.UpdateData(
                table: "StorageCategories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "Color",
                value: null);

            migrationBuilder.UpdateData(
                table: "StorageCategories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "Color",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "StorageCategories");
        }
    }
}
