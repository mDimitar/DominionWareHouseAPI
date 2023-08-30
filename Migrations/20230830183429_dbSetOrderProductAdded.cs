using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class dbSetOrderProductAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderProduct_Orders_OrderId",
                table: "OrderProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderProduct_Products_ProductId",
                table: "OrderProduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderProduct",
                table: "OrderProduct");

            migrationBuilder.RenameTable(
                name: "OrderProduct",
                newName: "ProductsInOrder");

            migrationBuilder.RenameIndex(
                name: "IX_OrderProduct_ProductId",
                table: "ProductsInOrder",
                newName: "IX_ProductsInOrder_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductsInOrder",
                table: "ProductsInOrder",
                columns: new[] { "OrderId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInOrder_Orders_OrderId",
                table: "ProductsInOrder",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInOrder_Products_ProductId",
                table: "ProductsInOrder",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInOrder_Orders_OrderId",
                table: "ProductsInOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInOrder_Products_ProductId",
                table: "ProductsInOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductsInOrder",
                table: "ProductsInOrder");

            migrationBuilder.RenameTable(
                name: "ProductsInOrder",
                newName: "OrderProduct");

            migrationBuilder.RenameIndex(
                name: "IX_ProductsInOrder_ProductId",
                table: "OrderProduct",
                newName: "IX_OrderProduct_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderProduct",
                table: "OrderProduct",
                columns: new[] { "OrderId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProduct_Orders_OrderId",
                table: "OrderProduct",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProduct_Products_ProductId",
                table: "OrderProduct",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
