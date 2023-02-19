using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class sixth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "TaskCompletionRecords");

            migrationBuilder.DropColumn(
                name: "PersonWhoApproved",
                table: "TaskCompletionRecords");

            migrationBuilder.AddColumn<ulong>(
                name: "UserId",
                table: "TaskCompletionRecords",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "UserWhoApprovedId",
                table: "TaskCompletionRecords",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TaskCompletionRecords");

            migrationBuilder.DropColumn(
                name: "UserWhoApprovedId",
                table: "TaskCompletionRecords");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TaskCompletionRecords",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PersonWhoApproved",
                table: "TaskCompletionRecords",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
