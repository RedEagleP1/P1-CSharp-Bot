using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCurrentlyJoined = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LastPostedImageTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LastRecordedPost = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastPostedImageTracks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LastPostedMessageTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LastRecordedPost = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastPostedMessageTracks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaskCompletionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    TaskType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskEvidence = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeTaken = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeTakenEvidence = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskDate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyAwarded = table.Column<float>(type: "float", nullable: false),
                    Verifiers = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecordDate = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCompletionRecords", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TextChannelsCurrencyGainMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    AmountGainedPerMessage = table.Column<int>(type: "int", nullable: false),
                    DelayBetweenAllowedMessageInMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextChannelsCurrencyGainMessage", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VoiceChannelTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    IsMuteOrDeafen = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastRecorded = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceChannelTracks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CurrenciesOwned",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Amount = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrenciesOwned", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrenciesOwned_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CurrencyAwardLimits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    AmountLeft = table.Column<float>(type: "float", nullable: false),
                    AwarderId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyAwardLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyAwardLimits_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TextChannelsCurrencyGainImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    AmountGainedPerImagePost = table.Column<int>(type: "int", nullable: false),
                    DelayBetweenAllowedImagePostInMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextChannelsCurrencyGainImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextChannelsCurrencyGainImage_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VoiceChannelCurrencyGains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    AmountGainedPerHourWhenMuteOrDeaf = table.Column<float>(type: "float", nullable: false),
                    AmountGainedPerHourWhenSpeaking = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceChannelCurrencyGains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceChannelCurrencyGains_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoiceChannelCurrencyGains_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleMessageAndSurveyRepeats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RepeatTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    RepeatAfterEvery_InDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMessageAndSurveyRepeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMessageAndSurveyRepeats_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMessages_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleMessagesAndSurveysRepeated",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LastRepeated = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMessagesAndSurveysRepeated", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMessagesAndSurveysRepeated_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolesCostAndReward",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Cost = table.Column<float>(type: "float", nullable: false),
                    CostCurrencyId = table.Column<int>(type: "int", nullable: true),
                    Reward = table.Column<float>(type: "float", nullable: false),
                    RewardCurrencyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesCostAndReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesCostAndReward_Currencies_CostCurrencyId",
                        column: x => x.CostCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolesCostAndReward_Currencies_RewardCurrencyId",
                        column: x => x.RewardCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolesCostAndReward_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolesForSale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesForSale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesForSale_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolesSurvey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Index = table.Column<int>(type: "int", nullable: false),
                    HasConditionalTrigger = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllTriggersShouldBeTrue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InitialMessage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllowOptionsMultiSelect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EndMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ParentSurveyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesSurvey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesSurvey_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesSurvey_RolesSurvey_ParentSurveyId",
                        column: x => x.ParentSurveyId,
                        principalTable: "RolesSurvey",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleSurveyOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    RoleSurveyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleSurveyOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleSurveyOptions_RolesSurvey_RoleSurveyId",
                        column: x => x.RoleSurveyId,
                        principalTable: "RolesSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleSurveyRoleSurveyTriggers",
                columns: table => new
                {
                    RoleSurveyId = table.Column<int>(type: "int", nullable: false),
                    RoleSurveyOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleSurveyRoleSurveyTriggers", x => new { x.RoleSurveyId, x.RoleSurveyOptionId });
                    table.ForeignKey(
                        name: "FK_RoleSurveyRoleSurveyTriggers_RolesSurvey_RoleSurveyId",
                        column: x => x.RoleSurveyId,
                        principalTable: "RolesSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleSurveyRoleSurveyTriggers_RoleSurveyOptions_RoleSurveyOpt~",
                        column: x => x.RoleSurveyOptionId,
                        principalTable: "RoleSurveyOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CurrenciesOwned_CurrencyId",
                table: "CurrenciesOwned",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAwardLimits_CurrencyId",
                table: "CurrencyAwardLimits",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMessageAndSurveyRepeats_RoleId",
                table: "RoleMessageAndSurveyRepeats",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleMessages_RoleId",
                table: "RoleMessages",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleMessagesAndSurveysRepeated_RoleId",
                table: "RoleMessagesAndSurveysRepeated",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_GuildId",
                table: "Roles",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesCostAndReward_CostCurrencyId",
                table: "RolesCostAndReward",
                column: "CostCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesCostAndReward_RewardCurrencyId",
                table: "RolesCostAndReward",
                column: "RewardCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesCostAndReward_RoleId",
                table: "RolesCostAndReward",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesForSale_RoleId",
                table: "RolesForSale",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesSurvey_ParentSurveyId",
                table: "RolesSurvey",
                column: "ParentSurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesSurvey_RoleId",
                table: "RolesSurvey",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleSurveyOptions_RoleSurveyId",
                table: "RoleSurveyOptions",
                column: "RoleSurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleSurveyRoleSurveyTriggers_RoleSurveyOptionId",
                table: "RoleSurveyRoleSurveyTriggers",
                column: "RoleSurveyOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TextChannelsCurrencyGainImage_GuildId",
                table: "TextChannelsCurrencyGainImage",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceChannelCurrencyGains_CurrencyId",
                table: "VoiceChannelCurrencyGains",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceChannelCurrencyGains_GuildId",
                table: "VoiceChannelCurrencyGains",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrenciesOwned");

            migrationBuilder.DropTable(
                name: "CurrencyAwardLimits");

            migrationBuilder.DropTable(
                name: "LastPostedImageTracks");

            migrationBuilder.DropTable(
                name: "LastPostedMessageTracks");

            migrationBuilder.DropTable(
                name: "RoleMessageAndSurveyRepeats");

            migrationBuilder.DropTable(
                name: "RoleMessages");

            migrationBuilder.DropTable(
                name: "RoleMessagesAndSurveysRepeated");

            migrationBuilder.DropTable(
                name: "RolesCostAndReward");

            migrationBuilder.DropTable(
                name: "RolesForSale");

            migrationBuilder.DropTable(
                name: "RoleSurveyRoleSurveyTriggers");

            migrationBuilder.DropTable(
                name: "TaskCompletionRecords");

            migrationBuilder.DropTable(
                name: "TextChannelsCurrencyGainImage");

            migrationBuilder.DropTable(
                name: "TextChannelsCurrencyGainMessage");

            migrationBuilder.DropTable(
                name: "VoiceChannelCurrencyGains");

            migrationBuilder.DropTable(
                name: "VoiceChannelTracks");

            migrationBuilder.DropTable(
                name: "RoleSurveyOptions");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "RolesSurvey");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
