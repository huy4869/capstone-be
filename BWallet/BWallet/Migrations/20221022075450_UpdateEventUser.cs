using Microsoft.EntityFrameworkCore.Migrations;

namespace BWallet.Migrations
{
    public partial class UpdateEventUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_EventUser_EventUserEventID_EventUserUserID",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_EventUserEventID_EventUserUserID",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EventUserEventID",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EventUserUserID",
                table: "User");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_User_EventUserEventID_EventUserUserID",
                table: "User",
                columns: new[] { "EventUserEventID", "EventUserUserID" });

            migrationBuilder.AddForeignKey(
                name: "FK_User_EventUser_EventUserEventID_EventUserUserID",
                table: "User",
                columns: new[] { "EventUserEventID", "EventUserUserID" },
                principalTable: "EventUser",
                principalColumns: new[] { "EventID", "UserID" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
