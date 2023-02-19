using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class eighth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasMessage",
                table: "Role");

            migrationBuilder.AddColumn<float>(
                name: "Cost",
                table: "Role",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "RoleSurvey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AllShouldBeTrue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InitialMessage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllowMultiSelect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EndMessage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentRoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ParentSurveyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleSurvey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleSurvey_Role_ParentRoleId",
                        column: x => x.ParentRoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleSurvey_RoleSurvey_ParentSurveyId",
                        column: x => x.ParentSurveyId,
                        principalTable: "RoleSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleRoleSurvey",
                columns: table => new
                {
                    ConditionRolesId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    PartOfRoleSurveysAsConditionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRoleSurvey", x => new { x.ConditionRolesId, x.PartOfRoleSurveysAsConditionId });
                    table.ForeignKey(
                        name: "FK_RoleRoleSurvey_Role_ConditionRolesId",
                        column: x => x.ConditionRolesId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleRoleSurvey_RoleSurvey_PartOfRoleSurveysAsConditionId",
                        column: x => x.PartOfRoleSurveysAsConditionId,
                        principalTable: "RoleSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleRoleSurvey1",
                columns: table => new
                {
                    OptionRolesId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    PartOfRoleSurveysAsOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRoleSurvey1", x => new { x.OptionRolesId, x.PartOfRoleSurveysAsOptionId });
                    table.ForeignKey(
                        name: "FK_RoleRoleSurvey1_Role_OptionRolesId",
                        column: x => x.OptionRolesId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleRoleSurvey1_RoleSurvey_PartOfRoleSurveysAsOptionId",
                        column: x => x.PartOfRoleSurveysAsOptionId,
                        principalTable: "RoleSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RoleRoleSurvey_PartOfRoleSurveysAsConditionId",
                table: "RoleRoleSurvey",
                column: "PartOfRoleSurveysAsConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleRoleSurvey1_PartOfRoleSurveysAsOptionId",
                table: "RoleRoleSurvey1",
                column: "PartOfRoleSurveysAsOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleSurvey_ParentRoleId",
                table: "RoleSurvey",
                column: "ParentRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleSurvey_ParentSurveyId",
                table: "RoleSurvey",
                column: "ParentSurveyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleRoleSurvey");

            migrationBuilder.DropTable(
                name: "RoleRoleSurvey1");

            migrationBuilder.DropTable(
                name: "RoleSurvey");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Role");

            migrationBuilder.AddColumn<bool>(
                name: "HasMessage",
                table: "Role",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
