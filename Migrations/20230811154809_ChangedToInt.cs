using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorksAt",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "WorksAtWarehouse",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorksAtWarehouse",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "WorksAt",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
