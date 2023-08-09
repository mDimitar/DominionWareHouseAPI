using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class switchNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInWarehouse_Products_ProductId",
                table: "ProductsInWarehouse");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInWarehouse_Warehouse_WarehouseId",
                table: "ProductsInWarehouse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductsInWarehouse",
                table: "ProductsInWarehouse");

            migrationBuilder.RenameTable(
                name: "ProductsInWarehouse",
                newName: "ProductsInWarehouses");

            migrationBuilder.RenameIndex(
                name: "IX_ProductsInWarehouse_ProductId",
                table: "ProductsInWarehouses",
                newName: "IX_ProductsInWarehouses_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductsInWarehouses",
                table: "ProductsInWarehouses",
                columns: new[] { "WarehouseId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInWarehouses_Products_ProductId",
                table: "ProductsInWarehouses",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInWarehouses_Warehouse_WarehouseId",
                table: "ProductsInWarehouses",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInWarehouses_Products_ProductId",
                table: "ProductsInWarehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductsInWarehouses_Warehouse_WarehouseId",
                table: "ProductsInWarehouses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductsInWarehouses",
                table: "ProductsInWarehouses");

            migrationBuilder.RenameTable(
                name: "ProductsInWarehouses",
                newName: "ProductsInWarehouse");

            migrationBuilder.RenameIndex(
                name: "IX_ProductsInWarehouses_ProductId",
                table: "ProductsInWarehouse",
                newName: "IX_ProductsInWarehouse_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductsInWarehouse",
                table: "ProductsInWarehouse",
                columns: new[] { "WarehouseId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInWarehouse_Products_ProductId",
                table: "ProductsInWarehouse",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsInWarehouse_Warehouse_WarehouseId",
                table: "ProductsInWarehouse",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
