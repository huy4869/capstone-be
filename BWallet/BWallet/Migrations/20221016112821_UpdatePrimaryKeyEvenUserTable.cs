using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BWallet.Migrations
{
    public partial class UpdatePrimaryKeyEvenUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventUser",
                table: "EventUser");

            migrationBuilder.DropColumn(
                name: "EventUserID",
                table: "EventUser");
        }
    }
}
