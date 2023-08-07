using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class addedDBSetForProdsInSc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInShoppingCart_Products_ProductId",
                table: "ProductsInShoppingCart");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInShoppingCart_ShoppingCart_ShoppingCartId",
                table: "ProductsInShoppingCart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductsInShoppingCart",
                table: "ProductsInShoppingCart");

            migrationBuilder.RenameTable(
                name: "ProductsInShoppingCart",
                newName: "ProductsInShoppingCarts");

            migrationBuilder.RenameIndex(
                name: "IX_ProductsInShoppingCart_ShoppingCartId",
                table: "ProductsInShoppingCarts",
                newName: "IX_ProductsInShoppingCarts_ShoppingCartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductsInShoppingCarts",
                table: "ProductsInShoppingCarts",
                columns: new[] { "ProductId", "ShoppingCartId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInShoppingCarts_Products_ProductId",
                table: "ProductsInShoppingCarts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInShoppingCarts_ShoppingCart_ShoppingCartId",
                table: "ProductsInShoppingCarts",
                column: "ShoppingCartId",
                principalTable: "ShoppingCart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInShoppingCarts_Products_ProductId",
                table: "ProductsInShoppingCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInShoppingCarts_ShoppingCart_ShoppingCartId",
                table: "ProductsInShoppingCarts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductsInShoppingCarts",
                table: "ProductsInShoppingCarts");

            migrationBuilder.RenameTable(
                name: "ProductsInShoppingCarts",
                newName: "ProductsInShoppingCart");

            migrationBuilder.RenameIndex(
                name: "IX_ProductsInShoppingCarts_ShoppingCartId",
                table: "ProductsInShoppingCart",
                newName: "IX_ProductsInShoppingCart_ShoppingCartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductsInShoppingCart",
                table: "ProductsInShoppingCart",
                columns: new[] { "ProductId", "ShoppingCartId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInShoppingCart_Products_ProductId",
                table: "ProductsInShoppingCart",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInShoppingCart_ShoppingCart_ShoppingCartId",
                table: "ProductsInShoppingCart",
                column: "ShoppingCartId",
                principalTable: "ShoppingCart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
