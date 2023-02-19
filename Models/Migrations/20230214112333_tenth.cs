using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class tenth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleRoleSurvey");

            migrationBuilder.DropTable(
                name: "RoleRoleSurvey1");

            migrationBuilder.RenameColumn(
                name: "AllowMultiSelect",
                table: "RoleSurvey",
                newName: "AllowOptionsMultiSelect");

            migrationBuilder.RenameColumn(
                name: "AllShouldBeTrue",
                table: "RoleSurvey",
                newName: "AllTriggersShouldBeTrue");

            migrationBuilder.AlterColumn<string>(
                name: "EndMessage",
                table: "RoleSurvey",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleSurveyOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleSurveyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleSurveyOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleSurveyOption_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleSurveyOption_RoleSurvey_RoleSurveyId",
                        column: x => x.RoleSurveyId,
                        principalTable: "RoleSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleSurveyRoleSurveyOption",
                columns: table => new
                {
                    RoleSurveysThatHaveThisOptionAsTriggerId = table.Column<int>(type: "int", nullable: false),
                    SurveyTriggerOptionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleSurveyRoleSurveyOption", x => new { x.RoleSurveysThatHaveThisOptionAsTriggerId, x.SurveyTriggerOptionsId });
                    table.ForeignKey(
                        name: "FK_RoleSurveyRoleSurveyOption_RoleSurvey_RoleSurveysThatHaveThi~",
                        column: x => x.RoleSurveysThatHaveThisOptionAsTriggerId,
                        principalTable: "RoleSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleSurveyRoleSurveyOption_RoleSurveyOption_SurveyTriggerOpt~",
                        column: x => x.SurveyTriggerOptionsId,
                        principalTable: "RoleSurveyOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RoleSurveyOption_RoleId",
                table: "RoleSurveyOption",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleSurveyOption_RoleSurveyId",
                table: "RoleSurveyOption",
                column: "RoleSurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleSurveyRoleSurveyOption_SurveyTriggerOptionsId",
                table: "RoleSurveyRoleSurveyOption",
                column: "SurveyTriggerOptionsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleSurveyRoleSurveyOption");

            migrationBuilder.DropTable(
                name: "RoleSurveyOption");

            migrationBuilder.RenameColumn(
                name: "AllowOptionsMultiSelect",
                table: "RoleSurvey",
                newName: "AllowMultiSelect");

            migrationBuilder.RenameColumn(
                name: "AllTriggersShouldBeTrue",
                table: "RoleSurvey",
                newName: "AllShouldBeTrue");

            migrationBuilder.UpdateData(
                table: "RoleSurvey",
                keyColumn: "EndMessage",
                keyValue: null,
                column: "EndMessage",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "EndMessage",
                table: "RoleSurvey",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
        }
    }
}
