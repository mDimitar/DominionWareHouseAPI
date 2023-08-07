using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class removeCols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "NewColumn",
            table: "ProductsInShoppingCart");

            migrationBuilder.DropColumn(
                name: "ProductQuantity",
                table: "ProductsInShoppingCart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
