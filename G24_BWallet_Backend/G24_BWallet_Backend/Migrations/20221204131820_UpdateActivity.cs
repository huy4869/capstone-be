using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class UpdateActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ActivityIconId",
                table: "Activity",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_ActivityIconId",
                table: "Activity",
                column: "ActivityIconId");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_UserID",
                table: "Activity",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_ActivityIcon_ActivityIconId",
                table: "Activity",
                column: "ActivityIconId",
                principalTable: "ActivityIcon",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_User_UserID",
                table: "Activity",
                column: "UserID",
                principalTable: "User",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_ActivityIcon_ActivityIconId",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_User_UserID",
                table: "Activity");

            migrationBuilder.DropIndex(
                name: "IX_Activity_ActivityIconId",
                table: "Activity");

            migrationBuilder.DropIndex(
                name: "IX_Activity_UserID",
                table: "Activity");

            migrationBuilder.AlterColumn<string>(
                name: "ActivityIconId",
                table: "Activity",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
