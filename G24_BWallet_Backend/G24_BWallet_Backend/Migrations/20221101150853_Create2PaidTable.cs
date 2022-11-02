using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class Create2PaidTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserDept_ReceiptId",
                table: "UserDept",
                column: "ReceiptId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDept_Receipt_ReceiptId",
                table: "UserDept",
                column: "ReceiptId",
                principalTable: "Receipt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDept_Receipt_ReceiptId",
                table: "UserDept");

            migrationBuilder.DropIndex(
                name: "IX_UserDept_ReceiptId",
                table: "UserDept");
        }
    }
}
