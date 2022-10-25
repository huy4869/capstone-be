using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class DeleteColumnFriend : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_EventUser_EventUserEventID_EventUserUserID",
                table: "Event");

            migrationBuilder.DropIndex(
                name: "IX_Event_EventUserEventID_EventUserUserID",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "EventStatus",
                table: "Friend");

            migrationBuilder.DropColumn(
                name: "EventUserEventID",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "EventUserUserID",
                table: "Event");

            migrationBuilder.AddForeignKey(
                name: "FK_EventUser_Event_EventID",
                table: "EventUser",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventUser_Event_EventID",
                table: "EventUser");

            migrationBuilder.AddColumn<int>(
                name: "EventStatus",
                table: "Friend",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EventUserEventID",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventUserUserID",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_EventUserEventID_EventUserUserID",
                table: "Event",
                columns: new[] { "EventUserEventID", "EventUserUserID" });

            migrationBuilder.AddForeignKey(
                name: "FK_Event_EventUser_EventUserEventID_EventUserUserID",
                table: "Event",
                columns: new[] { "EventUserEventID", "EventUserUserID" },
                principalTable: "EventUser",
                principalColumns: new[] { "EventID", "UserID" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
