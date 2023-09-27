using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class TryFixRGB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceivedGoodsBy",
                table: "ReceivedGoodsBy");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ReceivedGoodsBy",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ReceivedGoodsBy_UserId_ProductId",
                table: "ReceivedGoodsBy",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceivedGoodsBy",
                table: "ReceivedGoodsBy",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$p73pKGRcbl2E5UD/6A1HjO6ZCxUUHMOvqbTsYoHk7LKQAK20zBSzy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_ReceivedGoodsBy_UserId_ProductId",
                table: "ReceivedGoodsBy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceivedGoodsBy",
                table: "ReceivedGoodsBy");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ReceivedGoodsBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceivedGoodsBy",
                table: "ReceivedGoodsBy",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$yP33awnyWGBEFJirPNUsYOTCOK0zJD91gXTmNk4.V.RrwKN8kFzYW");
        }
    }
}
