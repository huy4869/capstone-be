using Capstone_API.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Capstone_API.Migrations
{
    public partial class Add_Receipts_UserDepts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "receipt",
                columns: table => new
                {
                    ReceiptID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    EventID = table.Column<int>(type: "int", nullable: false),

                    ReceiptName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReceiptPicture = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DivideType = table.Column<int>(type: "int", nullable: false),
                    ReceiptStatus = table.Column<int>(type: "int", nullable: false),
                    ReceiptAmount = table.Column<double>(type: "double", nullable: false),

                    
                    CreatedAt = table.Column<DateTime>(
                        type: "datetime(6)", 
                        nullable: false, 
                        defaultValue: DateTime.Now),
                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false,
                        defaultValue: DateTime.Now)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipt", x => x.ReceiptID);
                    table.ForeignKey(
                        name: "FK_Receipt_Event_EventID",
                        column: x => x.EventID,
                        principalTable: "Event",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipt_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_dept",
                columns: table => new 
                {
                    DeptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ReceiptID = table.Column<int>(type: "int", nullable: false),
                    
                    DeptStatus = table.Column<int>(type: "int", nullable: false),
                    Debit = table.Column<double>(type: "double", nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false,
                        defaultValue: DateTime.Now),
                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false,
                        defaultValue: DateTime.Now)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDept", x => x.DeptId);
                    table.ForeignKey(
                        name: "FK_UserDept_Receipt_ReceiptID",
                        column: x => x.ReceiptID,
                        principalTable: "Event",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDept_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_dept");
            migrationBuilder.DropTable(
                name: "receipt");
        }
    }
}
