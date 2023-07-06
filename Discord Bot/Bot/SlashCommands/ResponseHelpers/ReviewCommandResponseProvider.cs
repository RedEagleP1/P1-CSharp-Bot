using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public static class ReviewCommandResponseProvider
    {
        static List<IResponse> responses = CreateResponses();

        public static async Task HandleResponse(Request request)
        {
            foreach (var response in responses)
            {
                if (response.ShouldRespond(request))
                {
                    await response.HandleResponse(request);
                    return;
                }
            }
        }

        public static async Task HandleFirstResponse(SocketSlashCommand command)
        {
            if (command.Data.Name != "review")
            {
                return;
            }

            await DBReadWrite.LockReadWrite();
            try
            {
                var context = DBContextFactory.GetNewContext();
                var trustCurrency = await context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Name == "Trust");
                var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == command.User.Id);
                if (trustOwned == null || trustOwned.Amount < Settings.ReviewCommandSettings.Cost)
                {
                    await command.ModifyOriginalResponseAsync((mp) =>
                    {
                        mp.Content = $"Access denied. Task Reviews cost {Settings.ReviewCommandSettings.Cost} Trust. \r\n\r\n**\U0001f6d1 URGENT \U0001f6d1**\r\nEarn Trust, then return instead of pursuing other tasks or means of review. \r\n\r\n**Open this guide now**: https://trello.com/c/QHLYcAKQ.";
                    });

                    return;
                }

                trustOwned.Amount -= Settings.ReviewCommandSettings.Cost;
                await context.SaveChangesAsync();
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }   

            await command.ModifyOriginalResponseAsync((mp) =>
            {
                mp.Content = "Which type of Review would you like to submit?";
                mp.Components = MessageComponentAndEmbedHelper.CreateButtons("Task Review", "Academy Task Review");
                mp.Embed = new EmbedBuilder().WithTitle("Review").Build();
            });
        }

        static List<IResponse> CreateResponses()
        {
            var responses = new List<IResponse>();

            var taskReview_trelloCardLink = new StandardResponse()
                .WithContent("Add a link to the Trello card where the task was created")
                .WithButtons("Add Task Card Link")
                .WithFieldToAdd("Review Type")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureIncomingValueMatches("Task Review"));

            responses.Add(taskReview_trelloCardLink);
            ModalLauncher.AddModal("Add Task Card Link", "Task Card Link");

            var taskReview_acceptanceCriteria = new StandardResponse()
                .WithContent("Copy/paste the Acceptance Criteria the card creator made. " +
                "If the card did not have any, ask the card creator to make some before proceeding. " +
                "If they are not available, write out a minimum of 6 sentences describing the product to be delivered, " +
                "where it should have been delivered, in what format it should have been delivered and why. ")
                .WithButtons("Add Acceptance Criteria")
                .WithFieldToAdd("Task Card Link")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureModalTitleMatches("Task Card Link")
                .MakeSureFieldHasValue("Review Type", "Task Review")
                .MakeSureFieldDoesNotExist("Task Card Link"));

            responses.Add(taskReview_acceptanceCriteria);
            ModalLauncher.AddModal("Add Acceptance Criteria", "Acceptance Criteria");

            var taskReview_UserStory = new StandardResponse()
                .WithContent("Copy/paste the User Story if there was one. If there was none, just write NA in the modal.")
                .WithButtons("Add User Story")
                .WithFieldToAdd("Acceptance Criteria")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureFieldHasValue("Review Type", "Task Review")
                .MakeSureModalTitleMatches("Acceptance Criteria")
                .MakeSureFieldDoesNotExist("Acceptance Criteria"));

            responses.Add(taskReview_UserStory);

            ModalLauncher.AddModal("Add User Story", "User Story");

            var taskReview_WorkResult = new StandardResponse()
                .WithContent("Describe how you met all the Acceptance Criteria. Link to the results of the work:")
                .WithButtons("Add Work Result")
                .WithFieldToAdd("User Story")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureFieldHasValue("Review Type", "Task Review")
                .MakeSureModalTitleMatches("User Story")
                .MakeSureFieldDoesNotExist("User Story"));

            responses.Add(taskReview_WorkResult);

            ModalLauncher.AddModal("Add Work Result", "Work Result");

            var updateRelevantDocuments = new StandardResponse()
                .WithContent("Update relevant documents: \n" +
                "If your work involves changes anyone else should know about, documenting the changes and modifying all existing documentation " +
                "(including high level architectural documents related to the task) is critical. Link to any/all changes you made to documents and " +
                "reply with all the documentation you wrote. \nIn the case of design, make sure you update the documents on the idea progression board. ")
                .WithButtons("Add Documentation Changes", "No changes to the team's documentation are needed")
                .WithFieldToAdd("Work Result")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureModalTitleMatches("Work Result")
                .MakeSureFieldDoesNotExist("Work Result"));

            responses.Add(updateRelevantDocuments);

            ModalLauncher.AddModal("Add Documentation Changes", "Documentation Changes");

            var academyTaskReview_academyCardLink = new StandardResponse()
                .WithContent("Copy the original exam card where you found it on the academy here:")
                .WithButtons("Add Original Card Link")
                .WithFieldToAdd("Review Type")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureIncomingValueMatches("Academy Task Review"));

            responses.Add(academyTaskReview_academyCardLink);

            ModalLauncher.AddModal("Add Original Card Link", "Original Card Link");

            var academyTaskReview_requirements = new StandardResponse()
                .WithContent("Copy the requirements the exam card requested of you here:")
                .WithButtons("Add Requirements")
                .WithFieldToAdd("Original Card Link")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureFieldHasValue("Review Type", "Academy Task Review")
                .MakeSureModalTitleMatches("Original Card Link")
                .MakeSureFieldDoesNotExist("Original Card Link"));

            responses.Add(academyTaskReview_requirements);

            ModalLauncher.AddModal("Add Requirements", "Requirements");

            var academyTaskReview_description = new StandardResponse()
                .WithContent("Describe how you fully completed the requirements outlined by the Academy Task. Link to the cardi n which you put the results: ")
                .WithButtons("Add Academy Task Description")
                .WithFieldToAdd("Requirements")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureFieldHasValue("Review Type", "Academy Task Review")
                .MakeSureModalTitleMatches("Requirements")
                .MakeSureFieldDoesNotExist("Requirements"));

            responses.Add(academyTaskReview_description);

            ModalLauncher.AddModal("Add Academy Task Description", "Academy Task Description");

            var academyTaskReview_Results = new StandardResponse()
                .WithContent("Link to the results themselves. Either your answer, the miro board, card etc. where you put your answer. " +
                "Basically, give the reviewer a shortcut to quickly see how you met the requirements:")
                .WithButtons("Add Work Result")
                .WithFieldToAdd("Academy Task Description")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureFieldHasValue("Review Type", "Academy Task Review")
                .MakeSureModalTitleMatches("Academy Task Description")
                .MakeSureFieldDoesNotExist("Academy Task Description"));

            responses.Add(academyTaskReview_Results);


            var postToChannel = new PostMessageAsResponse()
                .WithContent("Great work, now your statement needs to be reviewed in #review by someone else to be submitted." + 
                            "Do /account anywhere in Discord to submit your time for the Revenue Royalty Program. ")
                .WithFieldToAdd("Documentation Changes")
                .OnChannel(DiscordQueryHelper.ReviewPostChannel)
                .WithPostMessageContent((request) =>
                {
                    return $"{request.User.Mention} is requesting a review of the following data, click [Review] to get give a review:";
                })
                .WithPostMessageButtons("Review")
                .WithPostMessageNewEmbedTitle("Review (Verification)")
                .WithPostMessageAddFields(new KeyValuePair<string, string>(HelperStrings.status, HelperStrings.verificationRequired),
                new KeyValuePair<string, string>(HelperStrings.verifiers, HelperStrings.none))
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review")
                .MakeSureFieldDoesNotExist("Documentation Changes")
                .AddCustomCondition((request) =>
                {
                    if(request.IncomingModalName == "Documentation Changes")
                    {
                        return true;
                    }

                    if(request.IncomingValue == "No changes to the team's documentation are needed")
                    {
                        return true;
                    }

                    return false;
                }));

            responses.Add(postToChannel);

            var verification_IsCriteriaClearEnough = new StartVerificationAsResponse()
                .WithContent("Is the Acceptance Criteria clear enough to review this task?")
                .WithButtons("It's clear", "It's unclear")
                .WithEmbedTitle("Review (Verification Process)")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification)")
                .MakeSureIncomingValueMatches("Review"));

            responses.Add(verification_IsCriteriaClearEnough);

            var verification_AcceptanceCriteria_unclear = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    var content = "Thank you for your time. We will hold off on this review until the submitter has time to come back and fix things.";
                    if (!await ReviewVerificationHelper.HandleResponse_NegativeVerification(request, content))
                    {
                        return;
                    }

                    var text = "Your card was rejected for review due to unclear Acceptance Criteria, " +
                    "please review the following data you submitted and resubmit with clearer Acceptance Criteria." +
                    $" We have refunded your {Settings.ReviewCommandSettings.Cost - Settings.ReviewCommandSettings.Reward} trust. \nHere is the data: \n";
                    var embed = request.ReferencedMessage.Embeds.First().ToEmbedBuilder().Build();
                    var userThatIsRequestingReview = await DiscordQueryHelper.GetUserAsync(FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault());
                    try
                    {
                        await userThatIsRequestingReview.SendMessageAsync(text: text, embed: embed);                                      
                    }
                    catch (Discord.Net.HttpException exc)
                    {
                        if (exc.DiscordCode != DiscordErrorCode.CannotSendMessageToUser)
                        {
                            Console.WriteLine(exc.ToString());
                        }
                    }
                    await DBReadWrite.LockReadWrite();
                    try
                    {
                        var context = DBContextFactory.GetNewContext();
                        var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                        var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == userThatIsRequestingReview.Id);
                        trustOwned.Amount += Settings.ReviewCommandSettings.Cost - Settings.ReviewCommandSettings.Reward;
                        await context.SaveChangesAsync();
                    }
                    finally
                    {
                        DBReadWrite.ReleaseLock();
                    }
                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatches("It's unclear"));

            responses.Add(verification_AcceptanceCriteria_unclear);

            var verification_AcceptanceCriteria_Clear = new StandardResponse()
                .WithContent("When looking at the completed work, does it match the requirements for completed work as listed here: + " +
                "\nhttps://trello.com/c/Fh00Z08B")
                .WithButtons("Requirements are met", "Requirements are not met")                
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatches("It's clear"));

            responses.Add(verification_AcceptanceCriteria_Clear);

            var verification_Requirements_NotMet = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    var content = "Thank you for your time. We will hold off on this review until the submitter has time to come back and fix things.";
                    if (!await ReviewVerificationHelper.HandleResponse_NegativeVerification(request, content))
                    {
                        return;
                    }

                    var text = "Your card was rejected for review because it didn't meet the requirements, " +
                    "please review the following data you submitted and resubmit after you meet the requirements." +
                    $" We have refunded your {Settings.ReviewCommandSettings.Cost - Settings.ReviewCommandSettings.Reward} trust. \nHere is the data: \n";
                    var embed = request.ReferencedMessage.Embeds.First().ToEmbedBuilder().Build();
                    var userThatIsRequestingReview = await DiscordQueryHelper.GetUserAsync(FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault());
                    await userThatIsRequestingReview.SendMessageAsync(text: text, embed: embed);

                    await DBReadWrite.LockReadWrite();
                    try
                    {
                        var context = DBContextFactory.GetNewContext();
                        var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                        var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == userThatIsRequestingReview.Id);
                        trustOwned.Amount += Settings.ReviewCommandSettings.Cost - Settings.ReviewCommandSettings.Reward;
                        await context.SaveChangesAsync();
                    }
                    finally
                    {
                        DBReadWrite.ReleaseLock();
                    }                    

                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatches("Requirements are not met"));

            responses.Add(verification_Requirements_NotMet);

            var verification_Requirements_Met = new StandardResponse()
                .WithContent("What sort of task is this?")
                .WithButtons("Programming", "Academy Exam", "Design", "Art", "Sound", "Production", "User Integration")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatches("Requirements are met"));

            responses.Add(verification_Requirements_Met);

            /*var verification_TaskType_Programming = new StandardResponse()
                .WithContent("Programming tasks need to be handled by specialists through a different system. " +
                $"\nWe will delete this submission and return the {Settings.ReviewCommandSettings.Reward} Trust to the treasury. Is that ok?")
                .WithButtons("Yes", "No")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatches("Programming"));

            responses.Add(verification_TaskType_Programming);

            var verification_TaskType_Programming_Yes = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    await request.DeleteOriginalMessageAsync();
                    var userThatIsRequestingReview = await DiscordQueryHelper.GetUserAsync(FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault());
                    await request.DeleteReferencedMessageAsync();

                    var text = "Good news! You don’t need to do /Review for programming tasks. The person who takes the pull request does the “review”. ";
                    await userThatIsRequestingReview.SendMessageAsync(text: text);
                    await DBReadWrite.LockReadWrite();
                    try
                    {
                        var context = DBContextFactory.GetNewContext();
                        var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                        var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == userThatIsRequestingReview.Id);
                        trustOwned.Amount += Settings.ReviewCommandSettings.Cost;
                        await context.SaveChangesAsync();
                    }
                    finally
                    {
                        DBReadWrite.ReleaseLock();
                    }
                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatches("Yes"));

            responses.Add(verification_TaskType_Programming_Yes);*/

            /*var verification_TaskType_Programming_No = new StandardResponse()
                .WithContent("What sort of task is this?")
                .WithButtons("Programming", "Academy Exam", "Design", "Art", "Sound", "Production", "User Integration")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatches("No"));

            responses.Add(verification_TaskType_Programming_No);*/

            var verification_rateAcceptanceCriteria = new StandardResponse()
                .WithContent("How well did the submission meet the Acceptance Criteria?")
                .WithButtons("1) Not at all", "2) It was missing key elements or needs revision", "3) Somewhat", "4) Very well", "5) They went far beyond the requirements")
                .WithFieldToAdd("Task Type")
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                //.MakeSureIncomingValueDoesNotMatch("Programming")
                .MakeSureFieldDoesNotExist("Task Type"));

            responses.Add(verification_rateAcceptanceCriteria);

            var verification_lowRating = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    var content = $"Please send <@{FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault()}> how they could improve their submission.\nThank you for your time. You have been awarded +{Settings.ReviewCommandSettings.Reward} Trust. \n";
                    if (!await ReviewVerificationHelper.HandleResponse_NegativeVerification(request, content))
                    {
                        return;
                    }

                    var text = "Your card was rejected for review due to insufficient completion. " +
                    $"Please reach out to {request.User.Mention} and ask them what was lacking in your submission if you need clarity. " +
                    $"Your {Settings.ReviewCommandSettings.Cost - Settings.ReviewCommandSettings.Reward} trust has been returned";


                    var embed = request.ReferencedMessage.Embeds.First().ToEmbedBuilder().Build();
                    var userThatIsRequestingReview = await DiscordQueryHelper.GetUserAsync(FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault());
                    try
                    {
                        await userThatIsRequestingReview.SendMessageAsync(text: text, embed: embed);
                    }
                    catch (Discord.Net.HttpException exc)
                    {
                        if (exc.DiscordCode != DiscordErrorCode.CannotSendMessageToUser)
                        {
                            Console.WriteLine(exc.ToString());
                        }
                    }

                    await DBReadWrite.LockReadWrite();
                    try
                    {
                        var context = DBContextFactory.GetNewContext();
                        var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                        var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == userThatIsRequestingReview.Id);
                        trustOwned.Amount += Settings.ReviewCommandSettings.Cost - Settings.ReviewCommandSettings.Reward;
                        await context.SaveChangesAsync();
                    }
                    finally
                    {
                        DBReadWrite.ReleaseLock();
                    }
                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatchesFollowing("1) Not at all", "2) It was missing key elements or needs revision"));

            responses.Add(verification_lowRating);

            var verification_highRating = new CustomResponse()
                .WithResponse(async (request) =>
                {
                    var content = $"Thank you for your time. You have been awarded +{Settings.ReviewCommandSettings.Reward} Trust!\nPlease send <@{FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault()}> congratulations and any tips you might have.\n";
                    if (!await ReviewVerificationHelper.HandleResponse_PositiveVerification(request, content))
                    {
                        return;
                    }

                    var currencyToAward = ReviewVerificationHelper.GetCurrencyToAward(request);
                    var text = $"Your review was approved! You have also been awarded +{currencyToAward} Trust for doing a great job! ";

                    await DBReadWrite.LockReadWrite();
                    try
                    {
                        var context = DBContextFactory.GetNewContext();
                        var trustCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
                        var trustOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == trustCurrency.Id && co.OwnerId == FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault());
                        trustOwned.Amount += currencyToAward;
                        await context.SaveChangesAsync();
                    }
                    finally
                    {
                        DBReadWrite.ReleaseLock();
                    }

                    var embed = request.ReferencedMessage.Embeds.First().ToEmbedBuilder().Build();
                    var userThatIsRequestingReview = await DiscordQueryHelper.GetUserAsync(FormatHelper.ExtractUserMentionsIDs(request.ReferencedMessage.Content).FirstOrDefault());
                    await userThatIsRequestingReview.SendMessageAsync(text: text, embed: embed);
                })
                .WithConditions(new Conditions()
                .MakeSureEmbedTitleMatches("Review (Verification Process)")
                .MakeSureIncomingValueMatchesFollowing("3) Somewhat", "4) Very well", "5) They went far beyond the requirements"));

            responses.Add(verification_highRating);
            return responses;
        }
    }
}
