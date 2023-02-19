using Discord;
using Discord.WebSocket;
using Models;
using Models.Migrations;
using System.Text.RegularExpressions;

namespace Bot.SlashCommands.AccountCommandSpecific
{
    public static class ResponseHelper
    {
        public static SocketTextChannel? postChannel;
        public static DBContextFactory? dbContextFactory;
        static readonly Regex timeFormatChecker = new Regex("^((\\d{1,2})h|(\\d{1,2})m|(\\d{1,2})h(\\d{1,2})m)$");
        const int penaltyPercentage = 40;
        public static MessageComponent CreateButtons(params string[] buttonNames)
        {
            var componentBuilder = new ComponentBuilder();
            for(int i=0; i<buttonNames.Length; i++)
            {
                componentBuilder.WithButton(buttonNames[i], buttonNames[i], row: i);
            }
            return componentBuilder.Build();
        }       
            
        public static async Task<IMessage> GetVerificationRequestMessage(Request request)
        {
            return await postChannel.GetMessageAsync(request.Message.Reference.MessageId.Value);
        }

        static async Task HandleNegativeVerification(Request request, IMessage verificationRequestMessage)
        {
            var builder = verificationRequestMessage.Embeds.First().ToEmbedBuilder();
            var statusField = EmbedHelper.GetField(builder, HelperStrings.status);
            var verifiersField = EmbedHelper.GetField(builder, HelperStrings.verifiers);
            verifiersField.Value = HelperStrings.none;
            statusField.Value = "Insufficient evidence, please review and resubmit.";

            await postChannel.ModifyMessageAsync(request.Message.Reference.MessageId.Value, (m) =>
            {
                m.Embed = builder.Build();
                m.Components = new ComponentBuilder().Build();
            });

            await request.UpdateOriginalMessageAsync($"Could you write back to <@{verificationRequestMessage.MentionedUserIds.First()}> explaining the further evidence they should present?", null, null);
            return;
        }

        static async Task HandlePositiveVerificationSimple(Request request, IMessage verificationRequestMessage)
        {
            var builder = verificationRequestMessage.Embeds.First().ToEmbedBuilder();
            var statusField = EmbedHelper.GetField(builder, HelperStrings.status);
            var verifiersField = EmbedHelper.GetField(builder, HelperStrings.verifiers);
            var verifiers = ExtractUserMentions(verifiersField.Value.ToString());
            verifiers.Add(request.User.Mention);
            verifiersField.Value = string.Concat(verifiers);
            statusField.Value = HelperStrings.verified;

            await postChannel.ModifyMessageAsync(request.Message.Reference.MessageId.Value, (m) =>
            {
                m.Embed = builder.Build();
                m.Components = new ComponentBuilder().Build();
            });

            await request.UpdateOriginalMessageAsync("Thank you for your help", null, null);
            await AwardCurrencyAndSaveRecord(verificationRequestMessage.MentionedUserIds.First(), verificationRequestMessage.CreatedAt.Date, builder.Build().Fields, false);
        }
        static async Task HandlePositiveVerificationWithoutTimeEvidence(Request request, IMessage verificationRequestMessage)
        {
            var builder = verificationRequestMessage.Embeds.First().ToEmbedBuilder();
            var statusField = EmbedHelper.GetField(builder, HelperStrings.status);
            var verifiersField = EmbedHelper.GetField(builder, HelperStrings.verifiers);
            var verifiers = ExtractUserMentions(verifiersField.Value.ToString());
            verifiers.Add(request.User.Mention);
            verifiersField.Value = string.Concat(verifiers);
            var timeTakenFieldFromUser = EmbedHelper.GetField(builder, HelperStrings.timeTaken);
            var timeTakenFromUser = Encryptor.Decrypt(timeTakenFieldFromUser.Value.ToString());
            var timeTakenFromVerifier = ExtractTimeInHours(request.IncomingValue);

            var newTimeTaken = ConvertHoursToTimeTaken(CalculateAverageTime(ExtractTimeInHours(timeTakenFromUser), timeTakenFromVerifier, verifiers.Count));
            if (verifiers.Count != 2)
            {
                newTimeTaken = Encryptor.Encrypt(newTimeTaken);
            }

            timeTakenFieldFromUser.Value = newTimeTaken;

            if (verifiers.Count == 2)
            {
                statusField.Value = $"Due to lack of time evidence, there is a penalty of {penaltyPercentage}% on the currency added compared to the time taken.";
            }

            await postChannel.ModifyMessageAsync(request.Message.Reference.MessageId.Value, (m) =>
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
                await AwardCurrencyAndSaveRecord(verificationRequestMessage.MentionedUserIds.First(), verificationRequestMessage.CreatedAt.Date, builder.Build().Fields, true);
            }
        }

        public static async Task VerifySimple(Request request, IMessage verificationRequestMessage, bool verifyPositive)
        {
            var builder = verificationRequestMessage.Embeds.First().ToEmbedBuilder();
            var statusField = EmbedHelper.GetField(builder, HelperStrings.status);

            if (statusField.Value.ToString() != HelperStrings.verificationRequired)
            {
                await request.UpdateOriginalMessageAsync("Looks like the verification is already complete.", null, null);
                return;
            }

            switch (verifyPositive)
            {
                case true:
                    await HandlePositiveVerificationSimple(request, verificationRequestMessage);
                    break;
                case false:
                    await HandleNegativeVerification(request, verificationRequestMessage);
                    break;
            }
        }

        public static async Task VerifyWithoutTimeEvidence(Request request, bool verifyPositive, IMessage verificationRequestMessage = null)
        {
            if(verificationRequestMessage == null)
            {
                verificationRequestMessage = await GetVerificationRequestMessage(request);
            }

            var builder = verificationRequestMessage.Embeds.First().ToEmbedBuilder();
            var statusField = EmbedHelper.GetField(builder, HelperStrings.status);

            if (statusField.Value.ToString() != HelperStrings.verificationRequired)
            {
                await request.UpdateOriginalMessageAsync("Looks like the verification is already complete.", null, null);
                return;
            }

            switch (verifyPositive)
            {
                case true:
                    await HandlePositiveVerificationWithoutTimeEvidence(request, verificationRequestMessage);
                    break;
                case false:
                    await HandleNegativeVerification(request, verificationRequestMessage);
                    break;
            }
        }

        static string ConvertHoursToTimeTaken(float hours)
        {
            int numberOfHours = (int)hours;
            float fractionalPart = hours % numberOfHours;
            int minutes = (int)(fractionalPart * 60f);
            return $"{numberOfHours}h{minutes}m";
        }
        static List<string> ExtractUserMentions(string input)
        {
            List<string> result = new List<string>();
            var matchCollection = Regex.Matches(input, "<(?:@|@!)\\d+>");
            foreach(Match match in matchCollection)
            {
                result.Add(match.Groups[0].Value);
            }

            return result;
        }        
        public static async Task PostMessage(string userMention, IEmbed embed)
        {
            if (postChannel == null)
            {
                return;
            }

            string content = $"{userMention} is requesting credit for work. Click the button to verify the integrity of the data submitted.";

            var messageComponent = new ComponentBuilder()
            .WithButton(HelperStrings.verify, HelperStrings.verify).Build();

            var builder = new EmbedBuilder()
            .WithTitle(HelperStrings.verificationRequired);
            foreach (var field in embed.Fields)
            {
                builder.AddField(field.Name, field.Value);
            }

            builder.AddField(HelperStrings.status, HelperStrings.verificationRequired);
            builder.AddField(HelperStrings.verifiers, HelperStrings.none);

            if(builder.Fields.FirstOrDefault(field => field.Name == HelperStrings.timeEvidence).Value.ToString() == HelperStrings.noTimeEvidenceProvided)
            {
                var timeTakenField = builder.Fields.FirstOrDefault(field => field.Name == HelperStrings.timeTaken);
                timeTakenField.Value = Encryptor.Encrypt(timeTakenField.Value.ToString());
            }

            await postChannel.SendMessageAsync(content, components: messageComponent, embed: builder.Build());
        }
        static async Task AwardCurrencyAndSaveRecord(ulong userId, DateTime messageCreationDate, IEnumerable<EmbedField> embedFields, bool applyPenalty)
        {
            if(dbContextFactory == null)
            {
                Console.WriteLine("Error: DBContextFactory not set");
                return;
            }

            var verifiers = embedFields.FirstOrDefault(f => f.Name == HelperStrings.verifiers).Value ?? string.Empty;
            var record = CreateRecord(userId, verifiers, messageCreationDate, embedFields, applyPenalty);

            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = dbContextFactory.GetNewContext();
                await context.TaskCompletionRecords.AddAsync(record);

                var currencyOwner = await context.CurrencyOwners.FindAsync(userId);
                if (currencyOwner == null)
                {
                    currencyOwner = new CurrencyOwner()
                    {
                        Id = userId,
                        OCH = 0,
                        SJH = 0
                    };

                    context.CurrencyOwners.Add(currencyOwner);
                }

                switch (record.CurrencyName)
                {
                    case "Open Collective Hour (OCH)":
                        currencyOwner.OCH += record.CurrencyAwarded;
                        break;
                    case "Sky Jellies Hour (SJH)":
                        currencyOwner.SJH += record.CurrencyAwarded;
                        break;
                    default:
                        Console.WriteLine("Error: Record was added but faced error while adding the currency. Currency name is wrong");
                        break;
                }

                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
            
        }
        public static float ExtractTimeInHours(string incomingValue)
        {
            var match = timeFormatChecker.Match(incomingValue);
            if (!match.Success)
            {
                return 0;
            }

            if (match.Groups[2].Success)
            {
                return float.Parse(match.Groups[2].Value);
            }

            if (match.Groups[3].Success)
            {
                return float.Parse(match.Groups[3].Value) / 60f;
            }

            return float.Parse(match.Groups[4].Value) + float.Parse(match.Groups[5].Value) / 60f;
        }
        public static bool IsInTimeFormat(string input)
        {          
            return timeFormatChecker.IsMatch(input);
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

            record.CurrencyAwarded = ExtractTimeInHours(record.TimeTaken);
            if (applyPenalty)
            {
                record.CurrencyAwarded *= (100f - penaltyPercentage)/100f ;
            }
            record.RecordDate = createdAt.ToShortDateString();
            record.Verifiers = usersWhoApproved;

            return record;
        }

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
    }
}
