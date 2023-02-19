using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class seventh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserWhoApprovedId",
                table: "TaskCompletionRecords");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TaskCompletionRecords",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Verifiers",
                table: "TaskCompletionRecords",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TaskCompletionRecords");

            migrationBuilder.DropColumn(
                name: "Verifiers",
                table: "TaskCompletionRecords");

            migrationBuilder.AddColumn<ulong>(
                name: "UserWhoApprovedId",
                table: "TaskCompletionRecords",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
        }
    }
}
