using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class FixOrgTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_OrganizationMembers_Id",
                table: "Organizations");

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "Organizations",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_GuildId",
                table: "Organizations",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Organizations_Id",
                table: "OrganizationMembers",
                column: "Id",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Guilds_GuildId",
                table: "Organizations",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Organizations_Id",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Guilds_GuildId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_GuildId",
                table: "Organizations");

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "Organizations",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_OrganizationMembers_Id",
                table: "Organizations",
                column: "Id",
                principalTable: "OrganizationMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
