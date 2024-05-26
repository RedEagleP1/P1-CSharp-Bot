﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models;

#nullable disable

namespace Models.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240526172218_CurrencyResetUpdate")]
    partial class CurrencyResetUpdate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Models.Currency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Currencies");
                });

            modelBuilder.Entity("Models.CurrencyAwardLimit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<float>("AmountLeft")
                        .HasColumnType("float");

                    b.Property<ulong>("AwarderId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("CurrencyId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId");

                    b.ToTable("CurrencyAwardLimits");
                });

            modelBuilder.Entity("Models.CurrencyOwned", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<float>("Amount")
                        .HasColumnType("float");

                    b.Property<int>("CurrencyId")
                        .HasColumnType("int");

                    b.Property<ulong>("OwnerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId");

                    b.ToTable("CurrenciesOwned");
                });

            modelBuilder.Entity("Models.CurrencyReset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<bool>("Auto")
                        .HasColumnType("tinyint(1)");

                    b.Property<int?>("CurrencyId")
                        .HasColumnType("int");

                    b.Property<int>("DaysBetween")
                        .HasColumnType("int");

                    b.Property<int?>("DaysLeft")
                        .HasColumnType("int");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("GuildId");

                    b.ToTable("CurrencyResets");
                });

            modelBuilder.Entity("Models.GlobalVoiceCurrencyGain", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<float>("AmountGainedPerHourWhenMuteOrDeaf")
                        .HasColumnType("float");

                    b.Property<float>("AmountGainedPerHourWhenSpeaking")
                        .HasColumnType("float");

                    b.Property<int?>("CurrencyId")
                        .HasColumnType("int");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("GuildId");

                    b.ToTable("GlobalVoiceCurrencyGains");
                });

            modelBuilder.Entity("Models.Guild", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("IsCurrentlyJoined")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Models.MessageValidationSuccessTrack", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("LastRecordedPost")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("MessageValidationSuccessTracks");
                });

            modelBuilder.Entity("Models.Role", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Models.RoleCostAndReward", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<float>("Cost")
                        .HasColumnType("float");

                    b.Property<int?>("CostCurrencyId")
                        .HasColumnType("int");

                    b.Property<float>("Reward")
                        .HasColumnType("float");

                    b.Property<int?>("RewardCurrencyId")
                        .HasColumnType("int");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("CostCurrencyId");

                    b.HasIndex("RewardCurrencyId");

                    b.HasIndex("RoleId")
                        .IsUnique();

                    b.ToTable("RolesCostAndReward");
                });

            modelBuilder.Entity("Models.RoleForSale", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("RoleId")
                        .IsUnique();

                    b.ToTable("RolesForSale");
                });

            modelBuilder.Entity("Models.RoleMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("RoleId")
                        .IsUnique();

                    b.ToTable("RoleMessages");
                });

            modelBuilder.Entity("Models.RoleMessageAndSurveyRepeat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("RepeatAfterEvery_InDays")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("RepeatTime")
                        .HasColumnType("time(6)");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("RoleId")
                        .IsUnique();

                    b.ToTable("RoleMessageAndSurveyRepeats");
                });

            modelBuilder.Entity("Models.RoleMessageAndSurveyRepeated", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("LastRepeated")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("RoleId")
                        .IsUnique();

                    b.ToTable("RoleMessagesAndSurveysRepeated");
                });

            modelBuilder.Entity("Models.RoleSurvey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<bool>("AllTriggersShouldBeTrue")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("AllowOptionsMultiSelect")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("EndMessage")
                        .HasColumnType("longtext");

                    b.Property<bool>("HasConditionalTrigger")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<string>("InitialMessage")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("ParentSurveyId")
                        .HasColumnType("int");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("ParentSurveyId");

                    b.HasIndex("RoleId");

                    b.ToTable("RolesSurvey");
                });

            modelBuilder.Entity("Models.RoleSurveyOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong?>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("RoleSurveyId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("RoleSurveyId");

                    b.ToTable("RoleSurveyOptions");
                });

            modelBuilder.Entity("Models.RoleSurveyRoleSurveyTrigger", b =>
                {
                    b.Property<int>("RoleSurveyId")
                        .HasColumnType("int");

                    b.Property<int>("RoleSurveyOptionId")
                        .HasColumnType("int");

                    b.HasKey("RoleSurveyId", "RoleSurveyOptionId");

                    b.HasIndex("RoleSurveyOptionId");

                    b.ToTable("RoleSurveyRoleSurveyTriggers");
                });

            modelBuilder.Entity("Models.TaskCompletionRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<float>("CurrencyAwarded")
                        .HasColumnType("float");

                    b.Property<string>("CurrencyName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RecordDate")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TaskDate")
                        .HasColumnType("longtext");

                    b.Property<string>("TaskEvidence")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TaskType")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TimeTaken")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TimeTakenEvidence")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Verifiers")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("TaskCompletionRecords");
                });

            modelBuilder.Entity("Models.TextChannelMessageValidation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AmountGainedPerMessage")
                        .HasColumnType("int");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("ChannelName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("CurrencyId")
                        .HasColumnType("int");

                    b.Property<string>("DMStyleFailure")
                        .HasColumnType("longtext");

                    b.Property<string>("DMStyleSuccess")
                        .HasColumnType("longtext");

                    b.Property<int>("DelayBetweenAllowedMessageInMinutes")
                        .HasColumnType("int");

                    b.Property<string>("GPTCriteria")
                        .HasColumnType("longtext");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsEnabledCharacterCountCheck")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsEnabledPhraseCheck")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("MessageToSendFailure")
                        .HasColumnType("longtext");

                    b.Property<string>("MessageToSendSuccess")
                        .HasColumnType("longtext");

                    b.Property<int>("MinimumCharacterCount")
                        .HasColumnType("int");

                    b.Property<string>("PhrasesThatShouldExist")
                        .HasColumnType("longtext");

                    b.Property<ulong?>("RoleToGiveFailure")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("RoleToGiveSuccess")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("ShouldContainMedia")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("ShouldContainMediaURL")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("ShouldContainURL")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("ShouldDeleteMessageOnFailure")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("ShouldDeleteMessageOnSuccess")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("ShouldSendDMFailure")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("ShouldSendDMSuccess")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("UseGPT")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("GuildId");

                    b.HasIndex("RoleToGiveFailure");

                    b.HasIndex("RoleToGiveSuccess");

                    b.ToTable("TextChannelMessageValidation");
                });

            modelBuilder.Entity("Models.VoiceChannelCurrencyGain", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<float>("AmountGainedPerHourWhenMuteOrDeaf")
                        .HasColumnType("float");

                    b.Property<float>("AmountGainedPerHourWhenSpeaking")
                        .HasColumnType("float");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("ChannelName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("CurrencyId")
                        .HasColumnType("int");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("GuildId");

                    b.ToTable("VoiceChannelCurrencyGains");
                });

            modelBuilder.Entity("Models.VoiceChannelTrack", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong?>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("IsMuteOrDeafen")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastRecorded")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("VoiceChannelTracks");
                });

            modelBuilder.Entity("Models.CurrencyAwardLimit", b =>
                {
                    b.HasOne("Models.Currency", null)
                        .WithMany()
                        .HasForeignKey("CurrencyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.CurrencyOwned", b =>
                {
                    b.HasOne("Models.Currency", null)
                        .WithMany()
                        .HasForeignKey("CurrencyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.CurrencyReset", b =>
                {
                    b.HasOne("Models.Currency", null)
                        .WithMany()
                        .HasForeignKey("CurrencyId");

                    b.HasOne("Models.Guild", null)
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.GlobalVoiceCurrencyGain", b =>
                {
                    b.HasOne("Models.Currency", null)
                        .WithMany()
                        .HasForeignKey("CurrencyId");

                    b.HasOne("Models.Guild", null)
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.Role", b =>
                {
                    b.HasOne("Models.Guild", null)
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.RoleCostAndReward", b =>
                {
                    b.HasOne("Models.Currency", null)
                        .WithMany()
                        .HasForeignKey("CostCurrencyId");

                    b.HasOne("Models.Currency", null)
                        .WithMany()
                        .HasForeignKey("RewardCurrencyId");

                    b.HasOne("Models.Role", null)
                        .WithOne()
                        .HasForeignKey("Models.RoleCostAndReward", "RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.RoleForSale", b =>
                {
                    b.HasOne("Models.Role", null)
                        .WithOne()
                        .HasForeignKey("Models.RoleForSale", "RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.RoleMessage", b =>
                {
                    b.HasOne("Models.Role", null)
                        .WithOne()
                        .HasForeignKey("Models.RoleMessage", "RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.RoleMessageAndSurveyRepeat", b =>
                {
                    b.HasOne("Models.Role", null)
                        .WithOne()
                        .HasForeignKey("Models.RoleMessageAndSurveyRepeat", "RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.RoleMessageAndSurveyRepeated", b =>
                {
                    b.HasOne("Models.Role", null)
                        .WithOne()
                        .HasForeignKey("Models.RoleMessageAndSurveyRepeated", "RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.RoleSurvey", b =>
                {
                    b.HasOne("Models.RoleSurvey", null)
                        .WithMany()
                        .HasForeignKey("ParentSurveyId");

                    b.HasOne("Models.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.RoleSurveyOption", b =>
                {
                    b.HasOne("Models.RoleSurvey", null)
                        .WithMany()
                        .HasForeignKey("RoleSurveyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.RoleSurveyRoleSurveyTrigger", b =>
                {
                    b.HasOne("Models.RoleSurvey", null)
                        .WithMany()
                        .HasForeignKey("RoleSurveyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Models.RoleSurveyOption", null)
                        .WithMany()
                        .HasForeignKey("RoleSurveyOptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.TextChannelMessageValidation", b =>
                {
                    b.HasOne("Models.Currency", null)
                        .WithMany()
                        .HasForeignKey("CurrencyId");

                    b.HasOne("Models.Guild", null)
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Models.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleToGiveFailure");

                    b.HasOne("Models.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleToGiveSuccess");
                });

            modelBuilder.Entity("Models.VoiceChannelCurrencyGain", b =>
                {
                    b.HasOne("Models.Currency", null)
                        .WithMany()
                        .HasForeignKey("CurrencyId");

                    b.HasOne("Models.Guild", null)
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
