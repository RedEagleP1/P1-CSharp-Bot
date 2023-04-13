using Discord;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public static class ReviewVerificationHelper
    {
        static bool IsVerified(IMessage messageToVerify)
        {
            var status = messageToVerify.Embeds.First().Fields.FirstOrDefault(f => f.Name == HelperStrings.status).Value;
            if (status != HelperStrings.verificationRequired)
            {
                return true;
            }

            return false;
        }
        public static async Task<bool> HandleResponse_NegativeVerification(Request request, string content)
        {
            if (IsVerified(request.ReferencedMessage))
            {
                await request.UpdateOriginalMessageAsync("Looks like the verification is already complete.", null, null);
                return false;
            }

            await HandleNegativeVerification(request, content);
            await DBReadWrite.LockReadWrite();
            try
            {
                var context = DBContextFactory.GetNewContext();
                var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == request.User.Id);
                if(trustOwned == null)
                {
                    trustOwned = new CurrencyOwned() { CurrencyId = trustCurrency.Id, OwnerId = request.User.Id, Amount = 0 };
                    context.CurrenciesOwned.Add(trustOwned);
                }
                trustOwned.Amount += Settings.ReviewCommandSettings.Reward;
                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
            return true;
        }

        public static async Task<bool> HandleResponse_PositiveVerification(Request request, string content)
        {
            if (IsVerified(request.ReferencedMessage))
            {
                await request.UpdateOriginalMessageAsync("Looks like the verification is already complete.", null, null);
                return false;
            }

            await HandlePositiveVerification(request, content);
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
                trustOwned.Amount += Settings.ReviewCommandSettings.Reward;
                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
            return true;
        }

        static async Task HandleNegativeVerification(Request request, string content)
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

            await request.UpdateOriginalMessageAsync(content, null, null);
        }

        public static float GetCurrencyToAward(Request request)
        {
            var taskType = MessageComponentAndEmbedHelper.GetField(request.Embed.ToEmbedBuilder(), "Task Type").Value.ToString();
            float currencyToAward = 0;

            var ratingRewards = taskType == "Academy Exam" ? Settings.ReviewCommandSettings.AcademyTaskRatingRewards : Settings.ReviewCommandSettings.NonAcademyTaskRatingRewards;

            switch (request.IncomingValue[..2])
            {
                case "3)":
                    currencyToAward = ratingRewards.Three;
                    break;
                case "4)":
                    currencyToAward = ratingRewards.Four;
                    break;
                case "5)":
                    currencyToAward = ratingRewards.Five;
                    break;
            }

            return currencyToAward;
        }

        static async Task HandlePositiveVerification(Request request, string content)
        {
            var builder = request.ReferencedMessage.Embeds.First().ToEmbedBuilder();
            var statusField = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.status);
            var verifiersField = MessageComponentAndEmbedHelper.GetField(builder, HelperStrings.verifiers);
            verifiersField.Value = request.User.Mention;
            statusField.Value = HelperStrings.verified;

            await request.Message.Channel.ModifyMessageAsync(request.ReferencedMessage.Id, (m) =>
            {
                m.Embed = builder.Build();
                m.Components = new ComponentBuilder().Build();
            });

            await request.UpdateOriginalMessageAsync(content, null, null);
        }
    }
}
