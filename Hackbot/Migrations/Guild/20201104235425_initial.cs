using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackbot.Migrations.Guild
{
    public partial class initial : Migration
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
                name: "Member",
                columns: table => new
                {
                    P_KEY = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Role = table.Column<int>(nullable: false),
                    Id = table.Column<long>(nullable: false),
                    GuildP_KEY = table.Column<ulong>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Member", x => x.P_KEY);
                    table.ForeignKey(
                        name: "FK_Member_Guilds_GuildP_KEY",
                        column: x => x.GuildP_KEY,
                        principalTable: "Guilds",
                        principalColumn: "P_KEY",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Member_GuildP_KEY",
                table: "Member",
                column: "GuildP_KEY");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Member");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
