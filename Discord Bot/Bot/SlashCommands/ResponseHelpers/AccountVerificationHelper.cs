using Discord;
using Discord.Rest;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public static class AccountVerificationHelper
    {
        const int penaltyPercentage = 40;
        public static bool HasVerified(ulong userId, Request request)
        {
            var verifiers = request.Message.Embeds.First().Fields.FirstOrDefault(field => field.Name == HelperStrings.verifiers).Value;
            var matchCollection = Regex.Matches(verifiers, "<(?:@|@!)(\\d+)>");
            foreach (Match match in matchCollection)
            {
                if (ulong.Parse(match.Groups[1].Value) == userId)
                {
                    return true;
                }
            }

            return false;
        }
        public static List<ulong> GetVerifiers(Request request)
        {
            var result = new List<ulong>();
            var verifiers = request.Message.Embeds.First().Fields.FirstOrDefault(field => field.Name == HelperStrings.verifiers).Value;
            var matchCollection = Regex.Matches(verifiers, "<(?:@|@!)(\\d+)>");
            foreach (Match match in matchCollection)
            {
                result.Add(ulong.Parse(match.Groups[1].Value));
            }

            return result;
        }        
        public static async Task HandleResponse_HasTimeEvidence(Request request)
        {
            if (IsVerified(request.ReferencedMessage))
            {
                await request.UpdateOriginalMessageAsync("Looks like the verification is already complete.", null, null);
                return;
            }

            var isAnswerPostive = request.IncomingValue == HelperStrings.yes;

            if (!isAnswerPostive)
            {
                await HandleNegativeVerification(request);
                return;
            }

            await HandlePositiveVerification_HasTimeEvidence(request);
            await DBReadWrite.LockReadWrite();
            try
            {
                var context = DBContextFactory.GetNewContext();
                var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == request.User.Id);
                if (trustOwned == null)
                {
                    trustOwned = new CurrencyOwned() { CurrencyId = trustCurrency.Id, OwnerId = request.User.Id, Amount = 0 };
                    context.CurrenciesOwned.Add(trustOwned);
                }
                trustOwned.Amount += Settings.AccountCommandSettings.Reward;
                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
        }
        public static async Task HandleResponse_NoTimeEvidence_NegativeVerification(Request request)
        {
            if (IsVerified(request.ReferencedMessage))
            {
                await request.UpdateOriginalMessageAsync("Looks like the verification is already complete.", null, null);
                return;
            }

            await HandleNegativeVerification(request);
            await DBReadWrite.LockReadWrite();
            try
            {
                var context = DBContextFactory.GetNewContext();
                var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == request.User.Id);
                if (trustOwned == null)
                {
                    trustOwned = new CurrencyOwned() { CurrencyId = trustCurrency.Id, OwnerId = request.User.Id, Amount = 0 };
                    context.CurrenciesOwned.Add(trustOwned);
                }
                trustOwned.Amount += Settings.AccountCommandSettings.Reward;
                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
        }
        public static async Task HandleResponse_NoTimeEvidence_PositiveVerification(Request request)
        {
            if (IsVerified(request.ReferencedMessage))
            {
                await request.UpdateOriginalMessageAsync("Looks like the verification is already complete.", null, null);
                return;
            }

            if (!FormatHelper.IsInTimeFormat(request.IncomingValue))
            {
                await request.RespondSeparatelyAsync("Time given was in the wrong format. Try again.", null, null, true);
                return;
            }

            if (FormatHelper.ExtractTimeInHours(request.IncomingValue) > 10)
            {
                await request.UpdateOriginalMessageAsync("We can't account for something that took so many hours without evidence.", null, null);
                return;
            }

            await HandlePositiveVerificationWithoutTimeEvidence(request);
            await DBReadWrite.LockReadWrite();
            try
            {
                var context = DBContextFactory.GetNewContext();
                var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == request.User.Id);
                if (trustOwned == null)
                {
                    trustOwned = new CurrencyOwned() { CurrencyId = trustCurrency.Id, OwnerId = request.User.Id, Amount = 0 };
                    context.CurrenciesOwned.Add(trustOwned);
                }
                trustOwned.Amount += Settings.AccountCommandSettings.Reward;
                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
        }
        static bool IsVerified(IMessage messageToVerify)
        {
            var status = messageToVerify.Embeds.First().Fields.FirstOrDefault(f => f.Name == HelperStrings.status).Value;
            if (status != HelperStrings.verificationRequired)
            {
                return true;
            }

            return false;
        }
        static async Task HandleNegativeVerification(Request request)
        {
            var builder = request.ReferencedMessage.Embeds.First().ToEmbedBuilder();
            var statusField = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.status);
            var verifiersField = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.verifiers);
            verifiersField.Value = HelperStrings.none;
            statusField.Value = "Insufficient evidence, please review and resubmit.";


            await request.Message.Channel.ModifyMessageAsync(request.ReferencedMessage.Id, (m) =>
            {
                m.Embed = builder.Build();
                m.Components = new ComponentBuilder().Build();
            });

            await request.UpdateOriginalMessageAsync($"Could you write back to <@{FormatHelper.ExtractUserMentions(request.ReferencedMessage.Content).FirstOrDefault()}> explaining the further evidence they should present?", null, null);
            return;
        }
        static async Task HandlePositiveVerification_HasTimeEvidence(Request request)
        {
            var builder = request.ReferencedMessage.Embeds.First().ToEmbedBuilder();
            var verifiersField = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.verifiers);
            var verifiers = FormatHelper.ExtractUserMentions(verifiersField.Value.ToString());
            verifiers.Add(request.User.Mention);
            verifiersField.Value = string.Concat(verifiers);
            var statusField = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.status);
            statusField.Value = HelperStrings.verified;

            await request.Message.Channel.ModifyMessageAsync(request.Message.Reference.MessageId.Value, (m) =>
            {
                m.Embed = builder.Build();
                m.Components = new ComponentBuilder().Build();
            });

            await request.UpdateOriginalMessageAsync("Thank you for your help", null, null);
            await AwardCurrencyAndSaveRecord(FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault(), request.ReferencedMessage.CreatedAt.Date, builder.Build().Fields, false);
        }
        static async Task HandlePositiveVerificationWithoutTimeEvidence(Request request)
        {
            var builder = request.ReferencedMessage.Embeds.First().ToEmbedBuilder();
            var statusField = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.status);
            var verifiersField = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.verifiers);
            var verifiers = FormatHelper.ExtractUserMentions(verifiersField.Value.ToString());
            verifiers.Add(request.User.Mention);
            verifiersField.Value = string.Concat(verifiers);
            var timeTakenFieldFromUser = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.timeTaken);
            var timeTakenFromUser = Encryptor.Decrypt(timeTakenFieldFromUser.Value.ToString());
            var timeTakenFromVerifier = FormatHelper.ExtractTimeInHours(request.IncomingValue);

            var newTimeTaken = ConvertHoursToTimeTaken(CalculateAverageTime(FormatHelper.ExtractTimeInHours(timeTakenFromUser), timeTakenFromVerifier, verifiers.Count));
            if (verifiers.Count != 2)
            {
                newTimeTaken = Encryptor.Encrypt(newTimeTaken);
            }

            timeTakenFieldFromUser.Value = newTimeTaken;

            if (verifiers.Count == 2)
            {
                statusField.Value = $"Due to lack of time evidence, there is a penalty of {penaltyPercentage}% on the currency added compared to the time taken.";
            }

            await request.Message.Channel.ModifyMessageAsync(request.Message.Reference.MessageId.Value, (m) =>
            {
                m.Embed = builder.Build();
                if (verifiers.Count == 2)
                {
                    m.Components = new ComponentBuilder().Build();
                }
            });

            await request.UpdateOriginalMessageAsync("Thank you for your help.", null, null);
            if (verifiers.Count == 2)
            {
                await AwardCurrencyAndSaveRecord(FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault(), request.ReferencedMessage.CreatedAt.Date, builder.Build().Fields, true);
            }
        }
        static float CalculateAverageTime(float previousTime, float verifierTime, int verifiersCount)
        {
            return verifiersCount switch
            {
                1 => (previousTime + verifierTime) / 2f,
                2 => ((previousTime * 2f) + verifierTime) / 3f,
                _ => 0
            };
        }
        static string ConvertHoursToTimeTaken(float hours)
        {
            int numberOfHours = (int)hours;
            float fractionalPart = hours % 1;
            int minutes = (int)(fractionalPart * 60f);
            return $"{numberOfHours}h{minutes}m";
        }
        public static async Task AwardCurrencyAndSaveRecord(ulong userId, DateTime messageCreationDate, IEnumerable<EmbedField> embedFields, bool applyPenalty)
        {
            var verifiers = embedFields.FirstOrDefault(f => f.Name == HelperStrings.verifiers).Value ?? string.Empty;
            var record = CreateRecord(userId, verifiers, messageCreationDate, embedFields, applyPenalty);

            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();
                await context.TaskCompletionRecords.AddAsync(record);

                var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == record.CurrencyName);
                var currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == currency.Id && co.OwnerId == userId);
                if (currencyOwned == null)
                {
                    currencyOwned = new() { CurrencyId = currency.Id, OwnerId = userId, Amount = 0 };
                    context.CurrenciesOwned.Add(currencyOwned);
                }

                currencyOwned.Amount += record.CurrencyAwarded;
                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }

        }
        static TaskCompletionRecord CreateRecord(ulong userId, string usersWhoApproved, DateTime createdAt, IEnumerable<EmbedField> fields, bool applyPenalty)
        {
            var record = new TaskCompletionRecord();
            record.UserId = userId;
            foreach (var field in fields)
            {
                switch (field.Name)
                {
                    case HelperStrings.description:
                        record.Description = field.Value;
                        break;
                    case HelperStrings.taskType:
                        record.TaskType = field.Value;
                        break;
                    case HelperStrings.taskEvidence:
                        record.TaskEvidence = field.Value;
                        break;
                    case HelperStrings.timeTaken:
                        record.TimeTaken = field.Value;
                        break;
                    case HelperStrings.timeEvidence:
                        record.TimeTakenEvidence = field.Value;
                        break;
                    case HelperStrings.date:
                        record.TaskDate = field.Value;
                        break;
                    case HelperStrings.currency:
                        record.CurrencyName = field.Value;
                        break;
                    case HelperStrings.status:
                        record.Status = field.Value;
                        break;
                }
            }

            record.CurrencyAwarded = FormatHelper.ExtractTimeInHours(record.TimeTaken);
            if (applyPenalty)
            {
                record.CurrencyAwarded *= (100f - penaltyPercentage) / 100f;
            }
            record.RecordDate = createdAt.ToShortDateString();
            record.Verifiers = usersWhoApproved;

            return record;
        }
    }
}
