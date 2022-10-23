using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Capstone_API.Migrations
{
    public partial class Add_Friend : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventUser_Event_EventID",
                table: "EventUser");

            migrationBuilder.DropForeignKey(
                name: "FK_EventUser_User_UserID",
                table: "EventUser");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Account_AccountID",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventUser",
                table: "EventUser");

            migrationBuilder.DropIndex(
                name: "IX_EventUser_EventID",
                table: "EventUser");

            migrationBuilder.DropIndex(
                name: "IX_EventUser_UserID",
                table: "EventUser");

            migrationBuilder.DropColumn(
                name: "EventUserID",
                table: "EventUser");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "User",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "EventID",
                table: "Event",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "AccountID",
                table: "Account",
                newName: "ID");

            migrationBuilder.AlterColumn<int>(
                name: "AccountID",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "EventUser",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "EventUser",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventUser",
                table: "EventUser",
                columns: new[] { "EventID", "UserID" });

            migrationBuilder.CreateTable(
                name: "Friend",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UserFriendID = table.Column<int>(type: "int", nullable: false),
                    EventStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friend", x => new { x.UserID, x.UserFriendID });
                    table.ForeignKey(
                        name: "FK_Friend_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Account_AccountID",
                table: "User",
                column: "AccountID",
                principalTable: "Account",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Account_AccountID",
                table: "User");

            migrationBuilder.DropTable(
                name: "Friend");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventUser",
                table: "EventUser");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "User",
                newName: "UserID");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Event",
                newName: "EventID");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Account",
                newName: "AccountID");

            migrationBuilder.AlterColumn<int>(
                name: "AccountID",
                table: "User",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "EventUser",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "EventUser",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EventUserID",
                table: "EventUser",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventUser",
                table: "EventUser",
                column: "EventUserID");

            migrationBuilder.CreateIndex(
                name: "IX_EventUser_EventID",
                table: "EventUser",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_EventUser_UserID",
                table: "EventUser",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_EventUser_Event_EventID",
                table: "EventUser",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "EventID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventUser_User_UserID",
                table: "EventUser",
                column: "UserID",
                principalTable: "User",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Account_AccountID",
                table: "User",
                column: "AccountID",
                principalTable: "Account",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
