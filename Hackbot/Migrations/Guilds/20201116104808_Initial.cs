using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackbot.Migrations.Guilds
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    P_KEY = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CaptainId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    InSearching = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.P_KEY);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    P_KEY = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Role = table.Column<string>(nullable: true),
                    Id = table.Column<long>(nullable: false),
                    GuildP_KEY = table.Column<ulong>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.P_KEY);
                    table.ForeignKey(
                        name: "FK_Members_Guilds_GuildP_KEY",
                        column: x => x.GuildP_KEY,
                        principalTable: "Guilds",
                        principalColumn: "P_KEY",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_GuildP_KEY",
                table: "Members",
                column: "GuildP_KEY");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
