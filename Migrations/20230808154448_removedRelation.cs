using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class removedRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInShoppingCarts_Orders_OrderId",
                table: "ProductsInShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_ProductsInShoppingCarts_OrderId",
                table: "ProductsInShoppingCarts");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "ProductsInShoppingCarts");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalSum",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalSum",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "ProductsInShoppingCarts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductsInShoppingCarts_OrderId",
                table: "ProductsInShoppingCarts",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInShoppingCarts_Orders_OrderId",
                table: "ProductsInShoppingCarts",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
