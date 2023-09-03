using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LastPostedImageTracks");

            migrationBuilder.DropTable(
                name: "LastPostedMessageTracks");

            migrationBuilder.DropTable(
                name: "TextChannelsCurrencyGainImage");

            migrationBuilder.DropTable(
                name: "TextChannelsCurrencyGainMessage");

            migrationBuilder.CreateTable(
                name: "MessageValidationSuccessTracks",
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
                    table.PrimaryKey("PK_MessageValidationSuccessTracks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TextChannelMessageValidation",
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
                    DelayBetweenAllowedMessageInMinutes = table.Column<int>(type: "int", nullable: false),
                    MinimumCharacterCount = table.Column<int>(type: "int", nullable: false),
                    IsEnabledCharacterCountCheck = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PhrasesThatShouldExist = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabledPhraseCheck = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShouldContainURL = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShouldContainMediaURL = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShouldContainMedia = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShouldDeleteMessageOnSuccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShouldDeleteMessageOnFailure = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MessageToSendSuccess = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShouldSendDMSuccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MessageToSendFailure = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShouldSendDMFailure = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RoleToGiveSuccess = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    RoleToGiveFailure = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UseGPT = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GPTCriteria = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DMStyleSuccess = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DMStyleFailure = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextChannelMessageValidation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextChannelMessageValidation_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TextChannelMessageValidation_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TextChannelMessageValidation_Roles_RoleToGiveFailure",
                        column: x => x.RoleToGiveFailure,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TextChannelMessageValidation_Roles_RoleToGiveSuccess",
                        column: x => x.RoleToGiveSuccess,
                        principalTable: "Roles",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TextChannelMessageValidation_CurrencyId",
                table: "TextChannelMessageValidation",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_TextChannelMessageValidation_GuildId",
                table: "TextChannelMessageValidation",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_TextChannelMessageValidation_RoleToGiveFailure",
                table: "TextChannelMessageValidation",
                column: "RoleToGiveFailure");

            migrationBuilder.CreateIndex(
                name: "IX_TextChannelMessageValidation_RoleToGiveSuccess",
                table: "TextChannelMessageValidation",
                column: "RoleToGiveSuccess");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageValidationSuccessTracks");

            migrationBuilder.DropTable(
                name: "TextChannelMessageValidation");

            migrationBuilder.CreateTable(
                name: "LastPostedImageTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LastRecordedPost = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
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
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LastRecordedPost = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastPostedMessageTracks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TextChannelsCurrencyGainImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AmountGainedPerImagePost = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    DelayBetweenAllowedImagePostInMinutes = table.Column<int>(type: "int", nullable: false),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                name: "TextChannelsCurrencyGainMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AmountGainedPerMessage = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    DelayBetweenAllowedMessageInMinutes = table.Column<int>(type: "int", nullable: false),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextChannelsCurrencyGainMessage", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TextChannelsCurrencyGainImage_GuildId",
                table: "TextChannelsCurrencyGainImage",
                column: "GuildId");
        }
    }
}
