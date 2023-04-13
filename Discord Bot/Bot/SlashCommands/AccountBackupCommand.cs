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
        public string Name => name;
        public SlashCommandProperties Properties => properties;
        public AccountBackupCommand(DiscordSocketClient client)
        {
            this.client = client;
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
            var user = client.GetGuild(Settings.P1RepublicGuildId)?.GetUser(command.User.Id);
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
            using var context = DBContextFactory.GetNewContext();
            writer.WriteLine($"Backup {DateTime.Now}\n");
            Dictionary<ulong, PersonRecord> personRecords = new();
            List<Currency> allCurrencies = context.Currencies.AsNoTracking().ToList();
            List<CurrencyOwned> AllCurrencyOwned = context.CurrenciesOwned.AsNoTracking().ToList();

            foreach(var currencyOwned in AllCurrencyOwned)
            {
                if(personRecords.ContainsKey(currencyOwned.OwnerId))
                {
                    continue;
                }

                var user = await client.GetUserAsync(currencyOwned.OwnerId);
                var currenciesOwnedByUser = AllCurrencyOwned.Where(co => co.OwnerId == user.Id).ToList();
                personRecords.Add(currencyOwned.OwnerId, new PersonRecord(currencyOwned.OwnerId,
                    $"{user.Username}#{user.Discriminator}", currenciesOwnedByUser, allCurrencies));
                
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
                foreach(var currencyOwned in personRecord.CurrenciesOwned)
                {
                    writer.WriteLine($"{currencyOwned.Value} {currencyOwned.Key}");
                }
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
            public Dictionary<string, float> CurrenciesOwned { get; private set; } = new();
            public List<string[]> Records { get; private set; } = new();
            public PersonRecord(ulong Id, string Name, List<CurrencyOwned> currenciesOwned, List<Currency> currencies)
            {
                this.Id = Id;
                this.Name = Name;
                foreach(var owned in currenciesOwned)
                {
                    var currencyName = currencies.FirstOrDefault(c => c.Id == owned.CurrencyId).Name;
                    CurrenciesOwned.Add(currencyName, owned.Amount);
                }                
            }
            public void AddRecord(string[] record)
            {
                Records.Add(record);
            }
        }
    }
}
