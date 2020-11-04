using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackbot.Migrations
{
    public partial class userId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
    name: "Admins",
    columns: table => new
    {
        Id = table.Column<ulong>(nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
        UserId = table.Column<int>(nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Admins", x => x.Id);
    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
