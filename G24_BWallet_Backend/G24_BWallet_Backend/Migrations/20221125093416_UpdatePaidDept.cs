using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class UpdatePaidDept : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidProof",
                table: "PaidDept");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaidProof",
                table: "PaidDept",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
