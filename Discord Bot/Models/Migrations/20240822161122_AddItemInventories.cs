using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class AddItemInventories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemInventories",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    itemId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    userId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    guildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemInventories", x => x.id);
                    table.ForeignKey(
                        name: "FK_ItemInventories_Guilds_guildId",
                        column: x => x.guildId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventories_guildId",
                table: "ItemInventories",
                column: "guildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemInventories");
        }
    }
}
