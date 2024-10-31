using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Chat;

namespace Bot.EventHandlers
{
	public class MessageUpdateHandler : IEventHandler
	{
        readonly DiscordSocketClient client;
        const string regexURLCheck = @"\b(?:https?://|www\.)\S+\b";
        const string regexMediaURLCheck = @"\b(?:https?://|www\.)\S+\.(?:jpg|jpeg|png|gif|bmp)\b";
        public MessageUpdateHandler(DiscordSocketClient client)
        {
            this.client = client;
        }
        public void Subscribe()
        {
            client.MessageReceived += ValidateMessage;
        }
        private Task ValidateMessage(SocketMessage message)
        {
            if (message.Source != MessageSource.User)
            {
                return Task.CompletedTask;
            }            

            var timeAtWhichEventWasTriggered = DateTime.Now;
            _ = Task.Run(async () =>
            {
                await DBReadWrite.LockReadWrite();
                try
                {
                    var context = DBContextFactory.GetNewContext();
                    var textChannelMessageValidation = await context.TextChannelMessageValidation.FirstOrDefaultAsync(mv => mv.ChannelId == message.Channel.Id);
                    if(textChannelMessageValidation == null || !textChannelMessageValidation.IsEnabled)
                    {
                        return;
                    }

                    var messageValidationSuccessTrack = await context.MessageValidationSuccessTracks.FirstOrDefaultAsync(track => track.UserId == message.Author.Id && track.ChannelId == message.Channel.Id);
                    if(!CheckTrack(messageValidationSuccessTrack, timeAtWhichEventWasTriggered, textChannelMessageValidation.DelayBetweenAllowedMessageInMinutes))
                    {
                        return;
                    }

                    if (MessageMeetsCriteria(message, textChannelMessageValidation))
                    {
                        if(!await ValidateWithGPT(textChannelMessageValidation, message))
                        {
                            await HandleFailureCase(textChannelMessageValidation, message);
                            return;
                        }
                        await UpdateTrack(context, message, timeAtWhichEventWasTriggered, messageValidationSuccessTrack);
                        await HandleSuccessCase(context, textChannelMessageValidation, message, timeAtWhichEventWasTriggered);
                        return;
                    }

                    await HandleFailureCase(textChannelMessageValidation, message);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    DBReadWrite.ReleaseLock();
                }

            });

            return Task.CompletedTask;
        }
        async Task HandleSuccessCase(ApplicationDbContext context, TextChannelMessageValidation textChannelMessageValidation, SocketMessage message, DateTime timeAtWhichEventWasTriggered)
        {            
            if (textChannelMessageValidation.CurrencyId != null)
            {
                await AwardCurrency(context, textChannelMessageValidation.CurrencyId.Value, textChannelMessageValidation.AmountGainedPerMessage, message);
            }
            if(textChannelMessageValidation.ShouldSendDMSuccess && textChannelMessageValidation.MessageToSendSuccess != null)
            {
                await message.Author.SendMessageAsync(textChannelMessageValidation.MessageToSendSuccess);
            }

            if(textChannelMessageValidation.RoleToGiveSuccess != null)
            {
                var guildUser = message.Author as SocketGuildUser;
                if (guildUser.Roles.FirstOrDefault(r => r.Id == textChannelMessageValidation.RoleToGiveSuccess.Value) == null)
                {
                    await guildUser.AddRoleAsync(textChannelMessageValidation.RoleToGiveSuccess.Value);
                }
            }

            if(textChannelMessageValidation.ShouldDeleteMessageOnSuccess)
            {
                await message.DeleteAsync();
            }
        }
        async Task HandleFailureCase(TextChannelMessageValidation textChannelMessageValidation, SocketMessage message)
        {
            if (textChannelMessageValidation.ShouldSendDMFailure && textChannelMessageValidation.MessageToSendFailure != null)
            {
                await message.Author.SendMessageAsync(textChannelMessageValidation.MessageToSendFailure);
            }

            if (textChannelMessageValidation.RoleToGiveFailure != null)
            {
                var guildUser = message.Author as SocketGuildUser;
                if(guildUser.Roles.FirstOrDefault(r => r.Id == textChannelMessageValidation.RoleToGiveFailure.Value) == null)
                {
                    await guildUser.AddRoleAsync(textChannelMessageValidation.RoleToGiveFailure.Value);
                }
            }

            if (textChannelMessageValidation.ShouldDeleteMessageOnFailure)
            {
                await message.DeleteAsync();
            }
        }
        async Task UpdateTrack(ApplicationDbContext context, SocketMessage message, DateTime timeAtWhichEventWasTriggered, MessageValidationSuccessTrack messageValidationSuccessTrack)
        {            
            if (messageValidationSuccessTrack == null)
            {
                messageValidationSuccessTrack = new MessageValidationSuccessTrack()
                {
                    ChannelId = message.Channel.Id,
                    UserId = message.Author.Id,
                    LastRecordedPost = timeAtWhichEventWasTriggered
                };

                context.MessageValidationSuccessTracks.Add(messageValidationSuccessTrack);
                await context.SaveChangesAsync();
                return;
            }

            messageValidationSuccessTrack.LastRecordedPost = timeAtWhichEventWasTriggered;
            await context.SaveChangesAsync();
        }
        bool CheckTrack(MessageValidationSuccessTrack messageValidationSuccessTrack, DateTime timeAtWhichEventWasTriggered, int delayInMinutes)
        {
            if(messageValidationSuccessTrack == null)
            {
                return true;
            }

            if (messageValidationSuccessTrack.LastRecordedPost > timeAtWhichEventWasTriggered)
            {
                return false;
            }

            if ((timeAtWhichEventWasTriggered - messageValidationSuccessTrack.LastRecordedPost).TotalMinutes < delayInMinutes)
            {
                return false;
            }

            return true;
        }
        
        bool MessageMeetsCriteria(SocketMessage message, TextChannelMessageValidation textChannelMessageValidation)
        {
            if (textChannelMessageValidation.IsEnabledCharacterCountCheck && message.Content.Length < textChannelMessageValidation.MinimumCharacterCount)
            {
                return false;
            }
            if (textChannelMessageValidation.IsEnabledPhraseCheck && textChannelMessageValidation.PhrasesThatShouldExist != null)
            {
                var phrases = textChannelMessageValidation.PhrasesThatShouldExist.Split('\n', '\r');
                foreach (var phrase in phrases)
                {
                    if (!message.Content.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            if(textChannelMessageValidation.ShouldContainURL && !Regex.IsMatch(message.Content, regexURLCheck))
            {
                return false;
            }
            if (textChannelMessageValidation.ShouldContainMediaURL && !Regex.IsMatch(message.Content, regexMediaURLCheck))
            {
                return false;
            }
            if (textChannelMessageValidation.ShouldContainMedia && 
                (message.Attachments.Count < 1 ||
                !message.Attachments.Any(a => a.Filename.EndsWith(".jpg", true, null) || 
                a.Filename.EndsWith(".jpeg", true, null) || a.Filename.EndsWith(".png", true, null) || 
                a.Filename.EndsWith(".tiff", true, null) || a.Filename.EndsWith(".gif", true, null) || 
                a.Filename.EndsWith(".tif", true, null) || a.Filename.EndsWith(".bmp", true, null))))
            {
                return false;
            }           

            return true;
        }

        async Task<bool> ValidateWithGPT(TextChannelMessageValidation textChannelMessageValidation, SocketMessage message)
        {
            if (textChannelMessageValidation.UseGPT && textChannelMessageValidation.GPTCriteria != null)
            {
                var outputResult = await GetGPTResponse(textChannelMessageValidation, message.Content);
                if(string.IsNullOrEmpty(outputResult.response))
                {
                    return outputResult.success;
                }

                var messageToSend = $"Thank you for your message. We were expecting a response about following: \n{textChannelMessageValidation.GPTCriteria}\n and you";
                if (outputResult.success)
                {
                    messageToSend += " met the criteria. Here is why:\n";
                }
                else
                {
                    messageToSend += " didn't meet the criteria. Here is why:\n";
                }

                messageToSend += outputResult.response;
                await message.Author.SendMessageAsync(messageToSend);
                return outputResult.success;
            }

            return true;
        }

        async Task<GPTResponse> GetGPTResponse(TextChannelMessageValidation textChannelMessageValidation, string message)
        {
            var systemChatMessage = "You are a text classification algorithm. All of your responses should be either \"Yes\" or \"No\"." +
                " It should be \"Yes\" only if the user's message meets the following criteria : " +
                textChannelMessageValidation.GPTCriteria;
            
            var openAI = new OpenAIAPI(Settings.GPT_APIKey);
            var request = new ChatRequest()
            {
                Model = OpenAI_API.Models.Model.ChatGPTTurbo,
                MaxTokens = 1024,
                Messages = new ChatMessage[]
                {
                    new ChatMessage(ChatMessageRole.System, systemChatMessage),
                    new ChatMessage(ChatMessageRole.User, message)
                }
            };
            var response = await openAI.Chat.CreateChatCompletionAsync(request);
            if(response == null)
            {
                return new GPTResponse(false, null);
            }
            var success = response.Choices[0].Message.Content == "Yes";
            systemChatMessage = "";
            if (success && !string.IsNullOrEmpty(textChannelMessageValidation.DMStyleSuccess))
            {
                systemChatMessage = textChannelMessageValidation.DMStyleSuccess + "\n";
            }
            else if(!success && ! string.IsNullOrEmpty(textChannelMessageValidation.DMStyleFailure))
            {
                systemChatMessage = textChannelMessageValidation.DMStyleFailure + "\n";
            }

            systemChatMessage += "Give reasons as to why the user's message ";
            if(!success)
            {
                systemChatMessage += "doesn't meet";
            }
            else
            {
                systemChatMessage += "successfully meets";
            }
            systemChatMessage += " the following criteria : " + textChannelMessageValidation.GPTCriteria;
            
            request = new ChatRequest()
            {
                Model = OpenAI_API.Models.Model.ChatGPTTurbo,
                MaxTokens = 1024,
                Messages = new ChatMessage[]
                {
                    new ChatMessage(ChatMessageRole.System, systemChatMessage),
                    new ChatMessage(ChatMessageRole.User, message)
                }
            };

            response = await openAI.Chat.CreateChatCompletionAsync(request);
            if (response == null)
            {
                return new GPTResponse(success, null);
            }

            return new GPTResponse(success, response.Choices[0].Message.Content);
        }
        async Task AwardCurrency(ApplicationDbContext context, int currencyId, int amount, SocketMessage message)
        {
            var currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.OwnerId == message.Author.Id && co.CurrencyId == currencyId);
            if (currencyOwned == null)
            {
                currencyOwned = new CurrencyOwned()
                {
                    OwnerId = message.Author.Id,
                    CurrencyId = currencyId,
                    Amount = 0
                };

                context.CurrenciesOwned.Add(currencyOwned);
            }

            currencyOwned.Amount += amount;
            await context.SaveChangesAsync();
        }

        struct GPTResponse
        {
            public bool success;
            public string response;

            public GPTResponse(bool success, string response)
            {
                this.success = success;
                this.response = response;
            }
        }
    }
}
