using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class CreatePaid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_dept_Receipt_ReceiptID",
                table: "user_dept");

            migrationBuilder.DropForeignKey(
                name: "FK_user_dept_User_UserID",
                table: "user_dept");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_dept",
                table: "user_dept");

            migrationBuilder.DropIndex(
                name: "IX_user_dept_ReceiptID",
                table: "user_dept");

            migrationBuilder.DropIndex(
                name: "IX_user_dept_UserID",
                table: "user_dept");

            migrationBuilder.RenameTable(
                name: "user_dept",
                newName: "UserDept");

            migrationBuilder.RenameColumn(
                name: "ReceiptID",
                table: "Receipt",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "UserDept",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ReceiptID",
                table: "UserDept",
                newName: "ReceiptId");

            migrationBuilder.RenameColumn(
                name: "Debit",
                table: "UserDept",
                newName: "DebtLeft");

            migrationBuilder.RenameColumn(
                name: "DeptId",
                table: "UserDept",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserDept",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptId",
                table: "UserDept",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Debt",
                table: "UserDept",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDept",
                table: "UserDept",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDept",
                table: "UserDept");

            migrationBuilder.DropColumn(
                name: "Debt",
                table: "UserDept");

            migrationBuilder.RenameTable(
                name: "UserDept",
                newName: "user_dept");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Receipt",
                newName: "ReceiptID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_dept",
                newName: "UserID");

            migrationBuilder.RenameColumn(
                name: "ReceiptId",
                table: "user_dept",
                newName: "ReceiptID");

            migrationBuilder.RenameColumn(
                name: "DebtLeft",
                table: "user_dept",
                newName: "Debit");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_dept",
                newName: "DeptId");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "user_dept",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptID",
                table: "user_dept",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_dept",
                table: "user_dept",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_user_dept_ReceiptID",
                table: "user_dept",
                column: "ReceiptID");

            migrationBuilder.CreateIndex(
                name: "IX_user_dept_UserID",
                table: "user_dept",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_user_dept_Receipt_ReceiptID",
                table: "user_dept",
                column: "ReceiptID",
                principalTable: "Receipt",
                principalColumn: "ReceiptID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_user_dept_User_UserID",
                table: "user_dept",
                column: "UserID",
                principalTable: "User",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
