using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class AddAutomations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Automation",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automation_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IdAuto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutomationId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    AutomationId1 = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    AutomationId2 = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    AutomationId3 = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdAuto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdAuto_Automation_AutomationId1",
                        column: x => x.AutomationId1,
                        principalTable: "Automation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdAuto_Automation_AutomationId2",
                        column: x => x.AutomationId2,
                        principalTable: "Automation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdAuto_Automation_AutomationId3",
                        column: x => x.AutomationId3,
                        principalTable: "Automation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IdAuto_Guilds_AutomationId",
                        column: x => x.AutomationId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InfoAuto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutomationId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    AutomationId1 = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    AutomationId2 = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    AutomationId3 = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoAuto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfoAuto_Automation_AutomationId1",
                        column: x => x.AutomationId1,
                        principalTable: "Automation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InfoAuto_Automation_AutomationId2",
                        column: x => x.AutomationId2,
                        principalTable: "Automation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InfoAuto_Automation_AutomationId3",
                        column: x => x.AutomationId3,
                        principalTable: "Automation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InfoAuto_Guilds_AutomationId",
                        column: x => x.AutomationId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Automation_GuildId",
                table: "Automation",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_IdAuto_AutomationId",
                table: "IdAuto",
                column: "AutomationId");

            migrationBuilder.CreateIndex(
                name: "IX_IdAuto_AutomationId1",
                table: "IdAuto",
                column: "AutomationId1");

            migrationBuilder.CreateIndex(
                name: "IX_IdAuto_AutomationId2",
                table: "IdAuto",
                column: "AutomationId2");

            migrationBuilder.CreateIndex(
                name: "IX_IdAuto_AutomationId3",
                table: "IdAuto",
                column: "AutomationId3");

            migrationBuilder.CreateIndex(
                name: "IX_InfoAuto_AutomationId",
                table: "InfoAuto",
                column: "AutomationId");

            migrationBuilder.CreateIndex(
                name: "IX_InfoAuto_AutomationId1",
                table: "InfoAuto",
                column: "AutomationId1");

            migrationBuilder.CreateIndex(
                name: "IX_InfoAuto_AutomationId2",
                table: "InfoAuto",
                column: "AutomationId2");

            migrationBuilder.CreateIndex(
                name: "IX_InfoAuto_AutomationId3",
                table: "InfoAuto",
                column: "AutomationId3");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdAuto");

            migrationBuilder.DropTable(
                name: "InfoAuto");

            migrationBuilder.DropTable(
                name: "Automation");
        }
    }
}
