using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class RemoveMaxMembersFromOrgsAndLegionsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxMembers",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "MaxMembers",
                table: "Legions");

            migrationBuilder.AlterColumn<int>(
                name: "MaxOrgsPerLegion",
                table: "TeamSettings",
                type: "int",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "MaxMembersPerOrg",
                table: "TeamSettings",
                type: "int",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "MaxOrgsPerLegion",
                table: "TeamSettings",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<ulong>(
                name: "MaxMembersPerOrg",
                table: "TeamSettings",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MaxMembers",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxMembers",
                table: "Legions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
