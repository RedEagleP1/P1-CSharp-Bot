using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.SlashCommands.ResponseHelpers
{
    public static class AccountCommandResponseProvider
    {
        static List<IResponse> responses = CreateResponses();
        public static async Task HandleResponse(Request request)
        {
            foreach(var response in responses)
            {
                if(response.ShouldRespond(request))
                {
                    await response.HandleResponse(request);
                    return;
                }
            }
        }

        public static async Task HandleFirstResponse(SocketSlashCommand command)
        {
            if(command.Data.Name != "account")
            {
                return;
            }

            await DBReadWrite.LockReadWrite();
            try
            {
                var context = DBContextFactory.GetNewContext();
                var trustCurrency = await context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Name == "Trust");
                var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == command.User.Id);
                if (trustOwned == null || trustOwned.Amount < Settings.AccountCommandSettings.Cost)
                {
                    await command.ModifyOriginalResponseAsync((mp) =>
                    {
                        mp.Content = $"/Account cost {Settings.AccountCommandSettings.Cost} Trust. You don't have enough. \nYou can learn how to earn Trust in https://trello.com/c/QHLYcAKQ.";
                    });

                    return;
                }

                trustOwned.Amount -= Settings.AccountCommandSettings.Cost;
                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }

            await command.ModifyOriginalResponseAsync((mp) =>
            {
                mp.Content = "Which version of hours are you accounting for?";
                mp.Components = MessageComponentAndEmbedHelper.CreateButtons("Sky Jellies Hour (SJH)");
                mp.Embed = new EmbedBuilder().WithTitle("Account").Build();
            });
        }

        static List<IResponse> CreateResponses()
        {
            var responses = new List<IResponse>();

            var askTaskType = new StandardResponse()
                .WithContent("For any account entry to be entered there must be at least one witness. " +
                         "Make sure the content you submit is accurate. " +
                         "First of all, we need to determine if you’re submitting a single task or a whole day of work")
                .WithButtons(HelperStrings.singleTask, HelperStrings.wholeDay)
                .WithFieldToAdd(HelperStrings.currency)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldDoesNotExist(HelperStrings.currency));

            responses.Add(askTaskType);

            var askDescription_SingleTask = new StandardResponse()
                .WithContent("Describe the task you accomplished in one sentence.")
                .WithButtons(HelperStrings.addDescription)
                .WithFieldToAdd(HelperStrings.taskType)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.currency)
                .MakeSureFieldDoesNotExist(HelperStrings.taskType)
                .MakeSureIncomingValueMatches(HelperStrings.singleTask));

            responses.Add(askDescription_SingleTask);

            var askDescription_WholeDay = new StandardResponse()
                .WithContent("Describe the tasks you accomplished that day. This does not need to be a comprehensive list.")
                .WithButtons(HelperStrings.addDescription)
                .WithFieldToAdd(HelperStrings.taskType)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.currency)
                .MakeSureFieldDoesNotExist(HelperStrings.taskType)
                .MakeSureIncomingValueMatches(HelperStrings.wholeDay));

            responses.Add(askDescription_WholeDay);

            ModalLauncher.AddModal(HelperStrings.addDescription, HelperStrings.description);

            var askTaskEvidence_SingleTask = new StandardResponse()
                .WithContent("Provide evidence that you completed the task." +
                                "This should be in the form of a hyperlink starting with HTTP and can link to an image, a Trello card, or a webpage")
                .WithButtons(HelperStrings.addTaskEvidence)
                .WithFieldToAdd(HelperStrings.description)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.singleTask)
                .MakeSureFieldExist(HelperStrings.taskType)
                .MakeSureFieldDoesNotExist(HelperStrings.description)
                .MakeSureModalTitleMatches(HelperStrings.description));

            responses.Add(askTaskEvidence_SingleTask);

            var askTaskEvidence_WholeDay = new StandardResponse()
                .WithContent("Provide evidence that you completed at least a few of the tasks mentioned. " +
                                "You should reply with hyperlinks starting with HTTP and can link to images, a Trello card, or a webpage.")
                .WithButtons(HelperStrings.addTaskEvidence)
                .WithFieldToAdd(HelperStrings.description)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.wholeDay)
                .MakeSureFieldExist(HelperStrings.taskType)
                .MakeSureFieldDoesNotExist(HelperStrings.description)
                .MakeSureModalTitleMatches(HelperStrings.description));

            responses.Add(askTaskEvidence_WholeDay);

            ModalLauncher.AddModal(HelperStrings.addTaskEvidence, HelperStrings.taskEvidence);

            var askTimeTaken = new StandardResponse()
                .WithContent("How long did the task take you? Remember we only expect 75% accuracy." +
                                "\nMake sure to follow the correct time format or your submission will be ignored until you input the correct format. Correct formats include:" +
                                "\n2h, 15h, 9m, 30m, 2h9m, 20h4m, 12h30m, 1h30m")
                .WithButtons(HelperStrings.addTimeTaken)
                .WithFieldToAdd(HelperStrings.taskEvidence)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.description)
                .MakeSureFieldDoesNotExist(HelperStrings.taskEvidence)
                .MakeSureModalTitleMatches(HelperStrings.taskEvidence));

            responses.Add(askTimeTaken);

            ModalLauncher.AddModal(HelperStrings.addTimeTaken, HelperStrings.timeTaken, placeHolder: "Examples -> 2h40m, 30m, 1h");

            var askTimeTakenEvidence_SingleTask = new StandardResponse()
                .WithContent("Provide evidence using a time stamp <#849364268494618634>, DeskTime Screenshot or activity watch. " +
                                "Respond to this post with a link to the timestamp, or a link to an image if you use DeskTime." +
                                $"\nIf you didn't clock your hours, select {HelperStrings.iDontHaveTimeEvidence}")
                .WithButtons(HelperStrings.addTimeEvidence, HelperStrings.iDontHaveTimeEvidence)
                .WithFieldToAdd(HelperStrings.timeTaken)
                .EnsureFormat(FormatHelper.timeFormatChecker)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.taskEvidence)
                .MakeSureFieldDoesNotExist(HelperStrings.timeTaken)
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.singleTask));

            responses.Add(askTimeTakenEvidence_SingleTask);

            var askTimeTakenEvidence_WholeDay = new StandardResponse()
                .WithContent("Provide evidence using a time stamp <#849364268494618634>, DeskTime Screenshot or activity watch. " +
                                "Respond to this post with a link to the timestamp, or a link to an image if you use DeskTime.")
                .WithButtons(HelperStrings.addTimeEvidence)
                .WithFieldToAdd(HelperStrings.timeTaken)
                .EnsureFormat(FormatHelper.timeFormatChecker)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.taskEvidence)
                .MakeSureFieldDoesNotExist(HelperStrings.timeTaken)
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.wholeDay));

            responses.Add(askTimeTakenEvidence_WholeDay);

            ModalLauncher.AddModal(HelperStrings.addTimeEvidence, HelperStrings.timeEvidence);

            var tooManyHoursWithoutEvidence_SingleTask = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    var content = "Looks like you are claiming too many hours without any evidence. Try dividing the task into smaller tasks and try again.";
                    await request.UpdateOriginalMessageAsync(content, null, null);
                    await DBReadWrite.LockReadWrite();
                    try
                    {
                        var context = DBContextFactory.GetNewContext();
                        var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                        var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == request.User.Id);
                        trustOwned.Amount += Settings.AccountCommandSettings.Cost;
                        await context.SaveChangesAsync();
                    }
                    finally
                    {
                        DBReadWrite.ReleaseLock();
                    }
                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.timeTaken)
                .MakeSureFieldDoesNotExist(HelperStrings.timeEvidence)
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.singleTask)
                .AddCustomCondition((request) =>
                {
                    return request.HasEmbedField(HelperStrings.timeTaken, out var timeTaken) && FormatHelper.ExtractTimeInHours(timeTaken) > 10;
                }));

            responses.Add(tooManyHoursWithoutEvidence_SingleTask);

            var askDetailedDescription_SingleTask = new StandardResponse()
                .WithContent("Please describe your task one more time in more detail. This will replace your original description that you added before.")
                .WithButtons(HelperStrings.addDetailedDescription)
                .WithFieldToAdd(HelperStrings.timeEvidence)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.timeTaken)
                .MakeSureFieldDoesNotExist(HelperStrings.timeEvidence)
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.singleTask)
                .MakeSureIncomingValueMatches(HelperStrings.iDontHaveTimeEvidence)
                .AddCustomCondition((request) =>
                {
                    return request.HasEmbedField(HelperStrings.timeTaken, out var timeTaken) && FormatHelper.ExtractTimeInHours(timeTaken) <= 10;
                }));

            responses.Add(askDetailedDescription_SingleTask);
            ModalLauncher.AddModal(HelperStrings.addDetailedDescription, HelperStrings.detailedDescription);

            var postMessage_NoEvidence_SingleTask = new PostMessageAsResponse()
                .WithContent("Great work, now your statement needs to be reviewed by someone else to be submitted. " +
                                "It has been posted to <#1050053145188913292>\nAsk someone you know to take a look at your submission and to verify it.")
                .WithFieldToAdd(HelperStrings.description)
                .OnChannel(DiscordQueryHelper.AccountPostChannel)
                .WithPostMessageContent((request) =>
                {
                    return $"{request.User.Mention} is requesting credit for work. Click the button to verify the integrity of the data submitted.";
                })
                .WithPostMessageButtons(HelperStrings.verify)
                .WithPostMessageNewEmbedTitle("Account (Verification)")
                .WithPostMessageAddFields(new KeyValuePair<string, string>(HelperStrings.status, HelperStrings.verificationRequired), 
                new KeyValuePair<string, string>(HelperStrings.verifiers, HelperStrings.none))
                .WithEncryptedFields(HelperStrings.timeTaken)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.timeEvidence)
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.singleTask)
                .AddCustomCondition((request) =>
                {
                    return request.HasEmbedField(HelperStrings.timeEvidence, out var timeEvidence) && timeEvidence == HelperStrings.iDontHaveTimeEvidence;
                }));

            responses.Add(postMessage_NoEvidence_SingleTask);

            var postMessage_HasEvidence_SingleTask = new PostMessageAsResponse()
                .WithContent("Great work, now your statement needs to be reviewed by someone else to be submitted. " +
                                "It has been posted to <#1050053145188913292>\nAsk someone you know to take a look at your submission and to verify it.")
                .WithFieldToAdd(HelperStrings.timeEvidence)
                .OnChannel(DiscordQueryHelper.AccountPostChannel)
                .WithPostMessageContent((request) =>
                {
                    return $"{request.User.Mention} is requesting credit for work. Click the button to verify the integrity of the data submitted.";
                })
                .WithPostMessageButtons(HelperStrings.verify)
                .WithPostMessageNewEmbedTitle("Account (Verification)")
                .WithPostMessageAddFields(new KeyValuePair<string, string>(HelperStrings.status, HelperStrings.verificationRequired),
                new KeyValuePair<string, string>(HelperStrings.verifiers, HelperStrings.none))
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.timeTaken)
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.singleTask)
                .MakeSureFieldDoesNotExist(HelperStrings.timeEvidence)
                .MakeSureModalTitleMatches(HelperStrings.timeEvidence));

            responses.Add(postMessage_HasEvidence_SingleTask);

            var askDate_WholeDay = new StandardResponse()
                .WithContent("For what day are you presenting evidence")
                .WithButtons(HelperStrings.addDate)
                .WithFieldToAdd(HelperStrings.timeEvidence)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.timeTaken)
                .MakeSureFieldDoesNotExist(HelperStrings.date)
                .MakeSureModalTitleMatches(HelperStrings.timeEvidence)
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.wholeDay));

            responses.Add(askDate_WholeDay);

            ModalLauncher.AddModal(HelperStrings.addDate, HelperStrings.date);

            var postMessage_WholeDay = new PostMessageAsResponse()
                .WithContent("Great work, now your statement needs to be reviewed by someone else to be submitted. " +
                                "It has been posted to <#1050053145188913292>\nAsk someone you know to take a look at your submission and to verify it.")
                .WithFieldToAdd(HelperStrings.date)
                .OnChannel(DiscordQueryHelper.AccountPostChannel)
                .WithPostMessageContent((request) =>
                {
                    return $"{request.User.Mention} is requesting credit for work. Click the button to verify the integrity of the data submitted.";
                })
                .WithPostMessageButtons(HelperStrings.verify)
                .WithPostMessageNewEmbedTitle("Account (Verification)")
                .WithPostMessageAddFields(new KeyValuePair<string, string>(HelperStrings.status, HelperStrings.verificationRequired),
                new KeyValuePair<string, string>(HelperStrings.verifiers, HelperStrings.none))
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account")
                .MakeSureFieldExist(HelperStrings.timeEvidence)
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.wholeDay)
                .MakeSureFieldDoesNotExist(HelperStrings.date)
                .MakeSureModalTitleMatches(HelperStrings.date));

            responses.Add(postMessage_WholeDay);

            var onClickVerify_SingleTask = new StartVerificationAsResponse()
                .WithContent($"{HelperStrings.verificationRequired}\nYou verify, that to the best of your knowledge, the evidence shows that the work was completed, " +
                "and the time frame stated is reasonable?")
                .WithButtons(HelperStrings.yes, HelperStrings.no)
                .WithEmbedTitle("Account (Verification Process)")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account (Verification)")
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.singleTask)
                .MakeSureIncomingValueMatches(HelperStrings.verify));

            responses.Add(onClickVerify_SingleTask);

            var onClickVerify_WholeDay = new StartVerificationAsResponse()
                .WithContent($"{HelperStrings.verificationRequired}\nA response is required to this message at the end.\r\n" +
                "Submitting a whole day is intended for those people who are doing so much that it’s hard for them to track individual tasks." +
                " Their contribution should be obviously visible to their team and you, as an individual, are verifying that their " +
                "contribution is to such a degree that they should be entitled to an easier version of Submitting accounts because " +
                "their contribution is outstanding and obvious.\r\nThe person who submitted only needs to be reasonably accurate as " +
                "your personal testimony, as the other half of the account. They are not required to provide a detailed account of " +
                "the whole day, but a general overview of things they have done. \r\nWhen registering a whole day, the number one " +
                "piece of evidence should be the DeskTime screenshot, or the timestamp along with 2-5 screenshots of what they did " +
                "that day.\r\nTypically people who submit this way, are very active, therefore, you can consider factors like do you " +
                "see them around often and is what they say reasonably believable? However, if you don’t ever see them around, and " +
                "they’re submitting this sort of thing, vote now so that they have to submit the individual tasks.\r\nDo you verify, " +
                "that to the best of your knowledge, the evidence shows that the work was completed, and the time frame stated is " +
                "reasonable?\r\n")
                .WithButtons(HelperStrings.yes, HelperStrings.no)
                .WithEmbedTitle("Account (Verification Process)")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account (Verification)")
                .MakeSureFieldHasValue(HelperStrings.taskType, HelperStrings.wholeDay)
                .MakeSureIncomingValueMatches(HelperStrings.verify));

            responses.Add(onClickVerify_WholeDay);

            var verificationComplete_HasTimeEvidence = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    await AccountVerificationHelper.HandleResponse_HasTimeEvidence(request);
                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account (Verification Process)")
                .AddCustomCondition((request) =>
                {
                    var timeEvidence = request.ReferencedMessage.Embeds.First().Fields.FirstOrDefault(f => f.Name == HelperStrings.timeEvidence).Value;
                    return timeEvidence != HelperStrings.iDontHaveTimeEvidence;
                }));

            responses.Add(verificationComplete_HasTimeEvidence);

            var verification_SingleTask_noTimeEvidence_negativeVerification = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    await AccountVerificationHelper.HandleResponse_NoTimeEvidence_NegativeVerification(request);
                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account (Verification Process)")
                .AddCustomCondition((request) =>
                {
                    var timeEvidence = request.ReferencedMessage.Embeds.First().Fields.FirstOrDefault(f => f.Name == HelperStrings.timeEvidence).Value;
                    return timeEvidence == HelperStrings.iDontHaveTimeEvidence && request.IncomingValue == HelperStrings.no;
                }));

            responses.Add(verification_SingleTask_noTimeEvidence_negativeVerification);

            var verification_SingleTask_noTimeEvidence_expectedTimeTaken = new StandardResponse()
                .WithContent("Looking at the completed task. How long do you think the task took to complete?")
                .WithButtons(HelperStrings.addTimeTaken)
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account (Verification Process)")
                .AddCustomCondition((request) =>
                {
                    var timeEvidence = request.ReferencedMessage.Embeds.First().Fields.FirstOrDefault(f => f.Name == HelperStrings.timeEvidence).Value;
                    return timeEvidence == HelperStrings.iDontHaveTimeEvidence && request.IncomingValue == HelperStrings.yes;
                }));

            responses.Add(verification_SingleTask_noTimeEvidence_expectedTimeTaken);


            var verificationComplete_NoTimeEvidence = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    await AccountVerificationHelper.HandleResponse_NoTimeEvidence_PositiveVerification(request);
                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Account (Verification Process)")
                .AddCustomCondition((request) =>
                {
                    var timeEvidence = request.ReferencedMessage.Embeds.First().Fields.FirstOrDefault(f => f.Name == HelperStrings.timeEvidence).Value;
                    return timeEvidence == HelperStrings.iDontHaveTimeEvidence;
                }));

            responses.Add(verificationComplete_NoTimeEvidence);

            return responses;
        }

    }
}
