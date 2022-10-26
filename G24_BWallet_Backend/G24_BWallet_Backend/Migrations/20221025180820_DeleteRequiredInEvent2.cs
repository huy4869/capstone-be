using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class DeleteRequiredInEvent2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_receipt_Event_EventID",
                table: "receipt");

            migrationBuilder.DropForeignKey(
                name: "FK_receipt_User_UserID",
                table: "receipt");

            migrationBuilder.DropIndex(
                name: "IX_receipt_EventID",
                table: "receipt");

            migrationBuilder.DropIndex(
                name: "IX_receipt_UserID",
                table: "receipt");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "receipt",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "receipt",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "receipt",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "receipt",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_receipt_EventID",
                table: "receipt",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_receipt_UserID",
                table: "receipt",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_receipt_Event_EventID",
                table: "receipt",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_receipt_User_UserID",
                table: "receipt",
                column: "UserID",
                principalTable: "User",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
