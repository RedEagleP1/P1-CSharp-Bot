using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class AddGlobalVoiceCurrencyGainsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalVoiceCurrencyGains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    AmountGainedPerHourWhenMuteOrDeaf = table.Column<float>(type: "float", nullable: false),
                    AmountGainedPerHourWhenSpeaking = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalVoiceCurrencyGains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalVoiceCurrencyGains_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GlobalVoiceCurrencyGains_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalVoiceCurrencyGains_CurrencyId",
                table: "GlobalVoiceCurrencyGains",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalVoiceCurrencyGains_GuildId",
                table: "GlobalVoiceCurrencyGains",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalVoiceCurrencyGains");
        }
    }
}
