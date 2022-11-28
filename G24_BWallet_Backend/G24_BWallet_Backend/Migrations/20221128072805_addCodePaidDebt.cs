using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class addCodePaidDebt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PaidDept",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Invite_EventID",
                table: "Invite",
                column: "EventID");

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_Event_EventID",
                table: "Invite",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invite_Event_EventID",
                table: "Invite");

            migrationBuilder.DropIndex(
                name: "IX_Invite_EventID",
                table: "Invite");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PaidDept");
        }
    }
}
