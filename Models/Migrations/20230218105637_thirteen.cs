using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class thirteen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurvey_Roles_ParentRoleId",
                table: "RoleSurvey");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurvey_RoleSurvey_ParentSurveyId",
                table: "RoleSurvey");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurveyOption_RoleSurvey_RoleSurveyId",
                table: "RoleSurveyOption");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurveyRoleSurveyOption_RoleSurvey_RoleSurveysThatHaveThi~",
                table: "RoleSurveyRoleSurveyOption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleSurvey",
                table: "RoleSurvey");

            migrationBuilder.RenameTable(
                name: "RoleSurvey",
                newName: "RolesSurvey");

            migrationBuilder.RenameIndex(
                name: "IX_RoleSurvey_ParentSurveyId",
                table: "RolesSurvey",
                newName: "IX_RolesSurvey_ParentSurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleSurvey_ParentRoleId",
                table: "RolesSurvey",
                newName: "IX_RolesSurvey_ParentRoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolesSurvey",
                table: "RolesSurvey",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolesSurvey_Roles_ParentRoleId",
                table: "RolesSurvey",
                column: "ParentRoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolesSurvey_RolesSurvey_ParentSurveyId",
                table: "RolesSurvey",
                column: "ParentSurveyId",
                principalTable: "RolesSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurveyOption_RolesSurvey_RoleSurveyId",
                table: "RoleSurveyOption",
                column: "RoleSurveyId",
                principalTable: "RolesSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurveyRoleSurveyOption_RolesSurvey_RoleSurveysThatHaveTh~",
                table: "RoleSurveyRoleSurveyOption",
                column: "RoleSurveysThatHaveThisOptionAsTriggerId",
                principalTable: "RolesSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolesSurvey_Roles_ParentRoleId",
                table: "RolesSurvey");

            migrationBuilder.DropForeignKey(
                name: "FK_RolesSurvey_RolesSurvey_ParentSurveyId",
                table: "RolesSurvey");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurveyOption_RolesSurvey_RoleSurveyId",
                table: "RoleSurveyOption");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSurveyRoleSurveyOption_RolesSurvey_RoleSurveysThatHaveTh~",
                table: "RoleSurveyRoleSurveyOption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolesSurvey",
                table: "RolesSurvey");

            migrationBuilder.RenameTable(
                name: "RolesSurvey",
                newName: "RoleSurvey");

            migrationBuilder.RenameIndex(
                name: "IX_RolesSurvey_ParentSurveyId",
                table: "RoleSurvey",
                newName: "IX_RoleSurvey_ParentSurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_RolesSurvey_ParentRoleId",
                table: "RoleSurvey",
                newName: "IX_RoleSurvey_ParentRoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleSurvey",
                table: "RoleSurvey",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurvey_Roles_ParentRoleId",
                table: "RoleSurvey",
                column: "ParentRoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurvey_RoleSurvey_ParentSurveyId",
                table: "RoleSurvey",
                column: "ParentSurveyId",
                principalTable: "RoleSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurveyOption_RoleSurvey_RoleSurveyId",
                table: "RoleSurveyOption",
                column: "RoleSurveyId",
                principalTable: "RoleSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSurveyRoleSurveyOption_RoleSurvey_RoleSurveysThatHaveThi~",
                table: "RoleSurveyRoleSurveyOption",
                column: "RoleSurveysThatHaveThisOptionAsTriggerId",
                principalTable: "RoleSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
