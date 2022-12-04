using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class UpdateActivityNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_ActivityIcon_ActivityIconId",
                table: "Activity");

            migrationBuilder.AlterColumn<int>(
                name: "ActivityIconId",
                table: "Activity",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_ActivityIcon_ActivityIconId",
                table: "Activity",
                column: "ActivityIconId",
                principalTable: "ActivityIcon",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_ActivityIcon_ActivityIconId",
                table: "Activity");

            migrationBuilder.AlterColumn<int>(
                name: "ActivityIconId",
                table: "Activity",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_ActivityIcon_ActivityIconId",
                table: "Activity",
                column: "ActivityIconId",
                principalTable: "ActivityIcon",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
