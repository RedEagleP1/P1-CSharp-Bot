using Discord;
using Discord.WebSocket;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.SlashCommands.AccountCommandSpecific
{
    public static class ResponseProvider
    {
        static List<StepResponse> stepResponses = new()
        {
            new StepResponse(async (request) =>
                {
                    var embed = new EmbedBuilder()
                    .WithTitle(HelperStrings.accountYourHours)
                    .Build();

                    var messageComponent = ResponseHelper.CreateButtons("Sky Jellies Hour (SJH)");
                    string content = "Which version of hours are you accounting for?";
                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);
                }, (request) =>
                {
                    return request.IncomingComponentType == ComponentType.SlashCommand;
                }),
            new StepResponse(async (request) =>
                {
                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.currency, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.singleTask, HelperStrings.wholeDay);
                    string content = "For any account entry to be entered there must be at least one witness. " +
                         "Make sure the content you submit is accurate. " +
                         "First of all, we need to determine if you’re submitting a single task or a whole day of work";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && !request.HasEmbedField(HelperStrings.currency, out _);
                }),
            new StepResponse(async (request) =>
                {
                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.taskType, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addDescription);
                    string content = "Describe the task you accomplished in one sentence.";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && !request.HasEmbedField(HelperStrings.taskType, out _);
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.description);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingValue == HelperStrings.addDescription;
                }),
            new StepResponse(async (request) =>
                {
                    if(request.HasEmbedField(HelperStrings.description, out _))
                    {
                        return;
                    }

                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.description, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addTaskEvidence);
                    string content = "Provide evidence that you completed the task." +
                                "This should be in the form of a hyperlink starting with HTTP and can link to an image, a Trello card, or a webpage";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingModalName == HelperStrings.description;
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.taskEvidence);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingValue == HelperStrings.addTaskEvidence;
                }),
            new StepResponse(async (request) =>
                {
                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.taskEvidence, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addTimeTaken);
                    string content = "How long did the task take you? Remember we only expect 75% accuracy. Tasks under five minutes or not accountable this way. " +
                                "\nMake sure to follow the correct time format or your submission will be ignored until you input the correct format. Correct formats include:" +
                                "\n2h, 15h, 9m, 30m, 2h9m, 20h4m, 12h30m, 1h30m";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingModalName == HelperStrings.taskEvidence;
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.timeTaken, shortInput: true, placeHolder: "Example formats -> 12h30m, 12h, 30m");
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingValue == HelperStrings.addTimeTaken;
                }),
            new StepResponse(async (request) =>
                {
                    if(request.HasEmbedField(HelperStrings.timeTaken, out _))
                    {
                        return;
                    }

                    if (!ResponseHelper.IsInTimeFormat(request.IncomingValue))
                    {
                        await request.RespondSeparatelyAsync("Time given was in the wrong format. Try again.", null, null, true);
                        return;
                    }

                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.timeTaken, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addTimeEvidence, HelperStrings.iDontHaveTimeEvidence);
                    string content = "Provide evidence using a time stamp <#849364268494618634>, DeskTime Screenshot or activity watch. " +
                                "Respond to this post with a link to the timestamp, or a link to an image if you use DeskTime." +
                                $"\nIf you didn't clock your hours, select {HelperStrings.iDontHaveTimeEvidence}";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingModalName == HelperStrings.timeTaken;
                }),
            new StepResponse(async (request) =>
                {                    
                    await request.SendModalAsync(HelperStrings.timeEvidence);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingValue == HelperStrings.addTimeEvidence;
                }),
            new StepResponse(async (request) =>
                {
                    if(request.HasEmbedField(HelperStrings.timeEvidence, out _))
                    {
                        return;
                    }

                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.timeEvidence, request.IncomingValue);
                    string content = "Great work, now your statement needs to be reviewed by someone else to be submitted. " +
                                "It has been posted to <#1050053145188913292>\nAsk someone you know to take a look at your submission and to verify it.";

                    await request.UpdateOriginalMessageAsync(content, null, embed);
                    await ResponseHelper.PostMessage(request.User.Mention, embed);

                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingModalName == HelperStrings.timeEvidence;
                }),
            new StepResponse(async (request) =>
                {
                    string content;
                    if(request.HasEmbedField(HelperStrings.timeTaken, out var timeTaken) && ResponseHelper.ExtractTimeInHours(timeTaken) > 10)
                    {
                        content = "Looks like you are claiming too many hours without any evidence. Try dividing the task into smaller tasks and try again.";
                        await request.UpdateOriginalMessageAsync(content, null, null);
                        return;
                    }

                    content = "Please describe your task one more time in more detail. This will replace your original description that you added before.";
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addDetailedDescription);

                    await request.UpdateOriginalMessageAsync(content, messageComponent, request.Embed.ToEmbedBuilder().Build());
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingValue == HelperStrings.iDontHaveTimeEvidence;
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.detailedDescription);                 

                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingValue == HelperStrings.addDetailedDescription;
                }),
            new StepResponse(async (request) =>
                {
                    var embedBuilder = request.Embed.ToEmbedBuilder();
                    EmbedHelper.ChangeField(embedBuilder, HelperStrings.description, request.IncomingValue);
                    EmbedHelper.AddField(embedBuilder, HelperStrings.timeEvidence, HelperStrings.noTimeEvidenceProvided);

                    string content = "Great work, now your statement needs to be reviewed by someone else to be submitted. " +
                                "It has been posted to <#1050053145188913292>\nAsk someone you know to take a look at your submission and to verify it.";

                    await request.UpdateOriginalMessageAsync(content, null, embedBuilder.Build());
                    await ResponseHelper.PostMessage(request.User.Mention, embedBuilder.Build());

                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.singleTask
                    && request.IncomingModalName == HelperStrings.detailedDescription;
                }),
            new StepResponse(async (request) =>
                {
                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.taskType, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addDescription);
                    string content = "Describe the tasks you accomplished that day. This does not need to be a comprehensive list.";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);;
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && !request.HasEmbedField(HelperStrings.taskType, out _);
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.description);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingValue == HelperStrings.addDescription;
                }),
            new StepResponse(async (request) =>
                {
                    if(request.HasEmbedField(HelperStrings.description, out _))
                    {
                        return;
                    }

                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.description, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addTaskEvidence);
                    string content = "Provide evidence that you completed at least a few of the tasks mentioned. " +
                                "You should reply with hyperlinks starting with HTTP and can link to images, a Trello card, or a webpage.";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);;
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingModalName == HelperStrings.description;
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.taskEvidence);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingValue == HelperStrings.addTaskEvidence;
                }),
            new StepResponse(async (request) =>
                {
                    if(request.HasEmbedField(HelperStrings.taskEvidence, out _))
                    {
                        return;
                    }

                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.taskEvidence, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addTimeTaken);
                    string content = "How long did the task take you? Remember we only expect 75% accuracy."+
                                "\nMake sure to follow the correct time format or your submission will be ignored until you input the correct format. Correct formats include:" +
                                "\n2h, 15h, 9m, 30m, 2h9m, 20h4m, 12h30m, 1h30m";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);;
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingModalName == HelperStrings.taskEvidence;
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.timeTaken, shortInput: true, placeHolder: "Example formats -> 12h30m, 12h, 30m");
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingValue == HelperStrings.addTimeTaken;
                }),
            new StepResponse(async (request) =>
                {
                    if (!ResponseHelper.IsInTimeFormat(request.IncomingValue))
                    {
                        await request.RespondSeparatelyAsync("Time given was in the wrong format. Try again.", null, null, true);
                        return;
                    }

                    if(request.HasEmbedField(HelperStrings.timeTaken, out _))
                    {
                        return;
                    }

                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.timeTaken, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addDate);
                    string content = "For what day are you presenting evidence?";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);;
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingModalName == HelperStrings.timeTaken;
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.date, shortInput: true, placeHolder: "Type the date as follows: 2023/Jan/5");
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingValue == HelperStrings.addDate;
                }),
            new StepResponse(async (request) =>
                {
                    if(request.HasEmbedField(HelperStrings.date, out _))
                    {
                        return;
                    }

                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.date, request.IncomingValue);
                    var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addTimeEvidence);
                    string content = "Provide evidence using a time stamp <#849364268494618634>, DeskTime Screenshot or activity watch. " +
                                "Respond to this post with a link to the timestamp, or a link to an image if you use DeskTime.";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);;
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingModalName == HelperStrings.date;
                }),
            new StepResponse(async (request) =>
                {
                    await request.SendModalAsync(HelperStrings.timeEvidence);
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingValue == HelperStrings.addTimeEvidence;
                }),
            new StepResponse(async (request) =>
                {
                    if(request.HasEmbedField(HelperStrings.timeEvidence, out _))
                    {
                        return;
                    }

                    var embed = EmbedHelper.AddField(request.Embed, HelperStrings.timeEvidence, request.IncomingValue);
                    var messageComponent = new ComponentBuilder().Build();
                    string content = "Great work, now your statement needs to be reviewed by someone else to be submitted. " +
                                "It has been posted to <#1050053145188913292>\nAsk someone you know to take a look at your submission and to verify it.";

                    await request.UpdateOriginalMessageAsync(content, messageComponent, embed);;
                    await ResponseHelper.PostMessage(request.User.Mention, embed);
                    
                }, (request) =>
                {
                    return request.MessageType == MessageType.TaskInfoCreationMessage
                    && request.TaskType == HelperStrings.wholeDay
                    && request.IncomingModalName == HelperStrings.timeEvidence;
                }),
            new StepResponse(async (request) =>
            {
                if(request.User.Id == request.Message.MentionedUserIds.First())
                {
                    await request.RespondSeparatelyAsync("You can't verify your own work. Ask someone else to verify it.");
                    return;
                }

                if(ResponseHelper.HasVerified(request.User.Id, request))
                {
                    await request.RespondSeparatelyAsync("Looks like you have already verified once. You can't verify again. Let someone else verify it.");
                    return;
                }

                var content = request.TaskType == HelperStrings.singleTask ?

                $"{HelperStrings.verificationRequired}\nYou verify, that to the best of your knowledge, the evidence shows that the work was completed, " +
                "and the time frame stated is reasonable?" :

                $"{HelperStrings.verificationRequired}\nA response is required to this message at the end.\r\n" +
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
                "reasonable?\r\n";

                var messageComponent = ResponseHelper.CreateButtons(HelperStrings.yes, HelperStrings.no);
                var embed = new EmbedBuilder()
                .WithTitle(HelperStrings.verification)
                .Build();

                await request.RespondSeparatelyAsync(content, components: messageComponent, embed: embed, ephemeral: true);
            }, (request) =>
            {
                return request.MessageType == MessageType.VerificationRequestMessage
                && request.IncomingValue == HelperStrings.verify;
            }),
            new StepResponse(async (request) =>
            {
                var verificationRequestMessage = await ResponseHelper.GetVerificationRequestMessage(request);
                var timeEvidenceField = EmbedHelper.GetField(verificationRequestMessage.Embeds.First().ToEmbedBuilder(), HelperStrings.timeEvidence);
                bool isVerifiedPositive = request.IncomingValue == HelperStrings.yes;

                if(timeEvidenceField.Value.ToString() != HelperStrings.noTimeEvidenceProvided)
                {                    
                    await ResponseHelper.VerifySimple(request, verificationRequestMessage, isVerifiedPositive);
                    return;
                }

                if (!isVerifiedPositive)
                {
                    await ResponseHelper.VerifyWithoutTimeEvidence(request, false, verificationRequestMessage);
                    return;
                }

                var content = "Looking at the completed task. How long do you think the task took to complete?";
                var messageComponent = ResponseHelper.CreateButtons(HelperStrings.addTimeTaken);
                var embed = new EmbedBuilder()
                .WithTitle(HelperStrings.verification)
                .Build();

                await request.UpdateOriginalMessageAsync(content, messageComponent, embed: embed);

            }, (request) =>
            {
                return request.MessageType == MessageType.VerificationProcessMessage
                && (request.IncomingValue == HelperStrings.yes || request.IncomingValue == HelperStrings.no);
            }),
            new StepResponse(async (request) =>
            {
                await request.SendModalAsync("Time Taken", shortInput: true, placeHolder: "Example formats -> 12h30m, 12h, 30m");

            }, (request) =>
            {
                return request.MessageType == MessageType.VerificationProcessMessage
                && request.IncomingValue == HelperStrings.addTimeTaken;
            }),
            new StepResponse(async (request) =>
            {
                if(request.HasEmbedField(HelperStrings.timeTaken, out _))
                {
                    return;
                }

                if (!ResponseHelper.IsInTimeFormat(request.IncomingValue))
                {
                    await request.RespondSeparatelyAsync("Time given was in the wrong format. Try again.", null, null, true);
                    return;
                }

                if(ResponseHelper.ExtractTimeInHours(request.IncomingValue) > 10)
                {
                    await request.UpdateOriginalMessageAsync("We can't account for something that took so many hours without evidence.", null, null);
                    return;
                }

                await ResponseHelper.VerifyWithoutTimeEvidence(request, true);
            }, (request) =>
            {
                return request.MessageType == MessageType.VerificationProcessMessage
                && request.IncomingModalName == HelperStrings.timeTaken;
            })            
        };       

        public static Func<Request, Task> GetResponse(Request request)
        {
            foreach(var stepResponse in stepResponses)
            {
                if (stepResponse.ShouldIRespond(request))
                {
                    return stepResponse.Response;
                }
            }

            return StepResponse.Empty.Response;
        }
        
    }
}
