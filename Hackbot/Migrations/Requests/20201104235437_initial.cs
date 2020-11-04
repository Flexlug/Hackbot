using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackbot.Migrations.Requests
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    P_KEY = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    From = table.Column<long>(nullable: false),
                    To = table.Column<long>(nullable: false),
                    RequestingRole = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.P_KEY);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requests");
        }
    }
}
