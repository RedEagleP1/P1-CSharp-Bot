using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class twelvth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_Guilds_GuildId",
                table: "Role");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurvey_Role_ParentRoleId",
                table: "RoleSurvey");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurveyOption_Role_RoleId",
                table: "RoleSurveyOption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Role",
                table: "Role");

            migrationBuilder.RenameTable(
                name: "Role",
                newName: "Roles");

            migrationBuilder.RenameIndex(
                name: "IX_Role_GuildId",
                table: "Roles",
                newName: "IX_Roles_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Guilds_GuildId",
                table: "Roles",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurvey_Roles_ParentRoleId",
                table: "RoleSurvey",
                column: "ParentRoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurveyOption_Roles_RoleId",
                table: "RoleSurveyOption",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Guilds_GuildId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurvey_Roles_ParentRoleId",
                table: "RoleSurvey");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurveyOption_Roles_RoleId",
                table: "RoleSurveyOption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Role");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_GuildId",
                table: "Role",
                newName: "IX_Role_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                table: "Role",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_Guilds_GuildId",
                table: "Role",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurvey_Role_ParentRoleId",
                table: "RoleSurvey",
                column: "ParentRoleId",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurveyOption_Role_RoleId",
                table: "RoleSurveyOption",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
