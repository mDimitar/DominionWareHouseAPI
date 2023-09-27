using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class newtest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_ReceivedGoodsBy_UserId_ProductId",
                table: "ReceivedGoodsBy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$6sX4iHGj/i3yLHlK0DvPpeEbw.bV7BXQOA0q.SnZpvAu3GrCSSZ5G");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedGoodsBy_UserId",
                table: "ReceivedGoodsBy",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceivedGoodsBy_UserId",
                table: "ReceivedGoodsBy");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ReceivedGoodsBy_UserId_ProductId",
                table: "ReceivedGoodsBy",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$dVkxVlkkzeU8SORB1aBTyeFn.hrKfpITSlI5uPDrU3pFuiPZ6gMa6");
        }
    }
}
