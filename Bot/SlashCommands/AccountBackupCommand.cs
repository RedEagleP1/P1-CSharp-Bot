using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.SlashCommands
{
    public class AccountBackupCommand : ISlashCommand
    {
        const string name = "accountbackup";
        readonly SlashCommandProperties properties = CreateNewProperties();
        readonly DiscordSocketClient client;
        readonly DBContextFactory dbContextFactory;
        readonly Settings settings;
        public string Name => name;
        public SlashCommandProperties Properties => properties;
        public AccountBackupCommand(DiscordSocketClient client, DBContextFactory dbContextFactory, Settings settings)
        {
            this.client = client;
            this.dbContextFactory = dbContextFactory;
            this.settings = settings;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(true);
            _ = Task.Run(async () =>
            {
                try
                {
                    await HandleResponse(command);
                }
                catch (Exception e)
                {
                    await command.ModifyOriginalResponseAsync(response => response.Content = "Some error occured. Contact developer.");
                    Console.WriteLine(e.Message);
                }
            });            
        }

        async Task HandleResponse(SocketSlashCommand command)
        {
            var user = client.GetGuild(settings.P1RepublicGuildId)?.GetUser(command.User.Id);
            if (user == null || user.Roles.FirstOrDefault(role => role.Name == "Hours Treasurer") == null)
            {
                return;
            }

            using (var memoryStream = new MemoryStream())
            {
                await WriteAccountBackupToStream(memoryStream);
                await user.SendFileAsync(memoryStream, "testFile.txt");
            }


            await command.ModifyOriginalResponseAsync(response =>
            {
                response.Content = "File created and sent. Check your DM.";
            });

        }
        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Create an account backup as a text file.")
                .Build();
        }

        async Task WriteAccountBackupToStream(Stream stream)
        {
            using var writer = new StreamWriter(stream, leaveOpen: true);
            using var context = dbContextFactory.GetNewContext();
            writer.WriteLine($"Backup {DateTime.Now}\n");
            Dictionary<ulong, PersonRecord> personRecords = new();
            foreach(var currencyOwner in context.CurrencyOwners.AsNoTracking())
            {
                var user = await client.GetUserAsync(currencyOwner.Id);
                personRecords.Add(currencyOwner.Id, new PersonRecord(currencyOwner.Id,
                    $"{user.Username}#{user.Discriminator}", currencyOwner.OCH, currencyOwner.SJH));
            }
            foreach(var taskCompletionRecord in context.TaskCompletionRecords.AsNoTracking())
            {
                List<string> record = new();
                record.Add("\n");
                record.Add($"Currency Name: {taskCompletionRecord.CurrencyName}");
                record.Add($"Task Type: {taskCompletionRecord.TaskType}");
                record.Add($"Description: {taskCompletionRecord.Description}");
                record.Add($"Task Evidence: {taskCompletionRecord.TaskEvidence}");
                record.Add($"Time Taken: {taskCompletionRecord.TimeTaken}");
                record.Add($"Time Taken Evidence: {taskCompletionRecord.TimeTakenEvidence}");
                if(taskCompletionRecord.TaskDate != null)
                {
                    record.Add($"Task Date: {taskCompletionRecord.TaskDate}");
                }
                record.Add($"Currency Awarded: {taskCompletionRecord.CurrencyAwarded}");
                record.Add($"Record Date: {taskCompletionRecord.RecordDate}");
                record.Add($"Status: {taskCompletionRecord.Status}");
                record.Add($"Verifiers: {taskCompletionRecord.Verifiers}");

                if(!personRecords.TryGetValue(taskCompletionRecord.UserId, out var personRecord))
                {
                    Console.WriteLine("Error: A task completion record exists but there is no currency owned by the person.");
                    continue;
                }

                personRecord.AddRecord(record.ToArray());
            }

            foreach(var personRecord in personRecords.Values)
            {
                writer.WriteLine($"===============================   {personRecord.Name} , Id: <@{personRecord.Id}>    ===================================");
                writer.WriteLine($"{personRecord.OCH} OCH");
                writer.WriteLine($"{personRecord.SJH} SJH");
                foreach(var record in personRecord.Records)
                {
                    foreach(string field in record)
                    {
                        writer.WriteLine(field);
                    }
                }
            }

            await writer.FlushAsync();
        }

        class PersonRecord
        {
            public string Name { get; private set; }
            public ulong Id { get; private set; }
            public float OCH { get; private set; }
            public float SJH { get; private set; }
            public List<string[]> Records { get; private set; } = new();
            public PersonRecord(ulong Id, string Name, float OCH, float SJH)
            {
                this.Id = Id;
                this.Name = Name;
                this.OCH = OCH;
                this.SJH = SJH;                
            }
            public void AddRecord(string[] record)
            {
                Records.Add(record);
            }
        }
    }
}
