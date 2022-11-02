using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace G24_BWallet_Backend.Migrations
{
    public partial class Create2PaidTable2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaidDept",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    PaidProof = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalMoney = table.Column<double>(type: "double", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaidDept", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaidDept_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaidDept_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PaidDebtList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PaidId = table.Column<int>(type: "int", nullable: false),
                    DebtId = table.Column<int>(type: "int", nullable: false),
                    PaidAmount = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaidDebtList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaidDebtList_PaidDept_PaidId",
                        column: x => x.PaidId,
                        principalTable: "PaidDept",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaidDebtList_UserDept_DebtId",
                        column: x => x.DebtId,
                        principalTable: "UserDept",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PaidDebtList_DebtId",
                table: "PaidDebtList",
                column: "DebtId");

            migrationBuilder.CreateIndex(
                name: "IX_PaidDebtList_PaidId",
                table: "PaidDebtList",
                column: "PaidId");

            migrationBuilder.CreateIndex(
                name: "IX_PaidDept_EventId",
                table: "PaidDept",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_PaidDept_UserId",
                table: "PaidDept",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaidDebtList");

            migrationBuilder.DropTable(
                name: "PaidDept");
        }
    }
}
