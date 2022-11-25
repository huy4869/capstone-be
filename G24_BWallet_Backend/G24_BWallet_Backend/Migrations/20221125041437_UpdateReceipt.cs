using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class UpdateReceipt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptPicture",
                table: "Receipt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptPicture",
                table: "Receipt",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
