using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class DeleteRequiredInEvent10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_receipt_event_EventID",
                table: "receipt");

            migrationBuilder.RenameColumn(
                name: "EventID",
                table: "receipt",
                newName: "event");

            migrationBuilder.RenameIndex(
                name: "IX_receipt_EventID",
                table: "receipt",
                newName: "IX_receipt_event");

            migrationBuilder.AddForeignKey(
                name: "FK",
                table: "Receipt",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_receipt_event_event",
                table: "receipt");

            migrationBuilder.RenameColumn(
                name: "event",
                table: "receipt",
                newName: "EventID");

            migrationBuilder.RenameIndex(
                name: "IX_receipt_event",
                table: "receipt",
                newName: "IX_receipt_EventID");

            migrationBuilder.AddForeignKey(
                name: "FK_receipt_event_EventID",
                table: "Receipt",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
