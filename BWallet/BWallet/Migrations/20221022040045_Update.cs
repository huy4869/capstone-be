using Microsoft.EntityFrameworkCore.Migrations;

namespace BWallet.Migrations
{
    public partial class Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventUserEventID",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventUserUserID",
                table: "User",
                type: "int",
                nullable: true);

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
                name: "IX_User_EventUserEventID_EventUserUserID",
                table: "User",
                columns: new[] { "EventUserEventID", "EventUserUserID" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_User_EventUser_EventUserEventID_EventUserUserID",
                table: "User",
                columns: new[] { "EventUserEventID", "EventUserUserID" },
                principalTable: "EventUser",
                principalColumns: new[] { "EventID", "UserID" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_EventUser_EventUserEventID_EventUserUserID",
                table: "Event");

            migrationBuilder.DropForeignKey(
                name: "FK_User_EventUser_EventUserEventID_EventUserUserID",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_EventUserEventID_EventUserUserID",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Event_EventUserEventID_EventUserUserID",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "EventUserEventID",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EventUserUserID",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EventUserEventID",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "EventUserUserID",
                table: "Event");
        }
    }
}
