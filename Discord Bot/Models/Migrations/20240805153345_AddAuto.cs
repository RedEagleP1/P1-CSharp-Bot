using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class AddAuto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Automations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automations_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IdAutos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutomationId = table.Column<int>(type: "int", nullable: true),
                    AutomationId1 = table.Column<int>(type: "int", nullable: true),
                    AutomationId2 = table.Column<int>(type: "int", nullable: true),
                    AutomationId3 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdAutos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdAutos_Automations_AutomationId",
                        column: x => x.AutomationId,
                        principalTable: "Automations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdAutos_Automations_AutomationId1",
                        column: x => x.AutomationId1,
                        principalTable: "Automations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdAutos_Automations_AutomationId2",
                        column: x => x.AutomationId2,
                        principalTable: "Automations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdAutos_Automations_AutomationId3",
                        column: x => x.AutomationId3,
                        principalTable: "Automations",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Automations_GuildId",
                table: "Automations",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_IdAutos_AutomationId",
                table: "IdAutos",
                column: "AutomationId");

            migrationBuilder.CreateIndex(
                name: "IX_IdAutos_AutomationId1",
                table: "IdAutos",
                column: "AutomationId1");

            migrationBuilder.CreateIndex(
                name: "IX_IdAutos_AutomationId2",
                table: "IdAutos",
                column: "AutomationId2");

            migrationBuilder.CreateIndex(
                name: "IX_IdAutos_AutomationId3",
                table: "IdAutos",
                column: "AutomationId3");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdAutos");

            migrationBuilder.DropTable(
                name: "Automations");
        }
    }
}
