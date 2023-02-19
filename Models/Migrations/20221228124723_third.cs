using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class third : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyCollection");

            migrationBuilder.CreateTable(
                name: "CurrencyAwarders",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    OCH = table.Column<float>(type: "float", nullable: false),
                    SJH = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyAwarders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CurrencyOwners",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    OCH = table.Column<float>(type: "float", nullable: false),
                    SJH = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyOwners", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyAwarders");

            migrationBuilder.DropTable(
                name: "CurrencyOwners");

            migrationBuilder.CreateTable(
                name: "CurrencyCollection",
                columns: table => new
                {
                    OwnerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    OCH = table.Column<float>(type: "float", nullable: false),
                    SJH = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyCollection", x => x.OwnerId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
