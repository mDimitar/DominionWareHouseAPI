using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemovedProductPriceForSelling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductPriceForSelling",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "ProductPriceForSelling",
                table: "ProductsInWarehouses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductPriceForSelling",
                table: "ProductsInWarehouses");

            migrationBuilder.AddColumn<int>(
                name: "ProductPriceForSelling",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
