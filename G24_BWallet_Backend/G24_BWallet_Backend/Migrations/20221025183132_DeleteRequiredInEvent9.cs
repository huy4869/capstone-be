using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class DeleteRequiredInEvent9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventUser_Event_EventID",
                table: "EventUser");

            migrationBuilder.DropForeignKey(
                name: "FK_receipt_Event_EventID",
                table: "receipt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Event",
                table: "Event");

            migrationBuilder.RenameTable(
                name: "Event",
                newName: "event");

            migrationBuilder.AddPrimaryKey(
                name: "PK_event",
                table: "event",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EventUser_event_EventID",
                table: "EventUser",
                column: "EventID",
                principalTable: "event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_receipt_event_EventID",
                table: "receipt",
                column: "EventID",
                principalTable: "event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventUser_event_EventID",
                table: "EventUser");

            migrationBuilder.DropForeignKey(
                name: "FK_receipt_event_EventID",
                table: "receipt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_event",
                table: "event");

            migrationBuilder.RenameTable(
                name: "event",
                newName: "Event");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Event",
                table: "Event",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EventUser_Event_EventID",
                table: "EventUser",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_receipt_Event_EventID",
                table: "receipt",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
