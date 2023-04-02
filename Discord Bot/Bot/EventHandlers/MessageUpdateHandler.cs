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

namespace Bot.EventHandlers
{
	public class MessageUpdateHandler : IEventHandler
	{
        readonly DiscordSocketClient client;
        public MessageUpdateHandler(DiscordSocketClient client)
        {
            this.client = client;
        }
        public void Subscribe()
        {
            client.MessageReceived += OnMessageRecievedForImages;
            client.MessageReceived += OnMessageRecievedForMessages;
        }

        private Task OnMessageRecievedForMessages(SocketMessage message)
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
                    var currencyGain = await context.TextChannelsCurrencyGainMessage.FirstOrDefaultAsync(cg => cg.ChannelId == message.Channel.Id);
                    if (currencyGain == null || !currencyGain.IsEnabled || currencyGain.CurrencyId == null)
                    {
                        return;
                    }

                    var lastTrack = await context.LastPostedMessageTracks.FirstOrDefaultAsync(p => p.UserId == message.Author.Id && p.ChannelId == message.Channel.Id);
                    if (lastTrack == null)
                    {
                        lastTrack = new LastPostedMessageTrack()
                        {
                            ChannelId = message.Channel.Id,
                            UserId = message.Author.Id,
                            LastRecordedPost = timeAtWhichEventWasTriggered
                        };

                        context.LastPostedMessageTracks.Add(lastTrack);
                        await AwardCurrency(context, currencyGain, message);
                        return;
                    }

                    if (lastTrack.LastRecordedPost > timeAtWhichEventWasTriggered)
                    {
                        return;
                    }

                    if ((timeAtWhichEventWasTriggered - lastTrack.LastRecordedPost).TotalMinutes < currencyGain.DelayBetweenAllowedMessageInMinutes)
                    {
                        return;
                    }

                    lastTrack.LastRecordedPost = timeAtWhichEventWasTriggered;
                    await AwardCurrency(context, currencyGain, message);
                }
                finally
                {
                    DBReadWrite.ReleaseLock();
                }

            });

            return Task.CompletedTask;
        }

        private Task OnMessageRecievedForImages(SocketMessage message)
        {
            if(message.Source != MessageSource.User)
            {
                return Task.CompletedTask;
            }

            if(message.Attachments.Count < 1)
            {
                return Task.CompletedTask;
            }

            if(!message.Attachments.Any(a => a.Filename.EndsWith(".jpg", true, null) || a.Filename.EndsWith(".jpeg", true, null) || a.Filename.EndsWith(".png", true, null)
            || a.Filename.EndsWith(".tiff", true, null) || a.Filename.EndsWith(".gif", true, null) || a.Filename.EndsWith(".tif", true, null) || a.Filename.EndsWith(".bmp", true, null)))
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
                    var currencyGain = await context.TextChannelsCurrencyGainImage.FirstOrDefaultAsync(cg => cg.ChannelId == message.Channel.Id);
                    if(currencyGain == null || !currencyGain.IsEnabled || currencyGain.CurrencyId == null)
                    {
                        return;
                    }

                    var lastTrack = await context.LastPostedImageTracks.FirstOrDefaultAsync(p => p.UserId == message.Author.Id && p.ChannelId == message.Channel.Id);
                    if(lastTrack == null)
                    {
                        lastTrack = new LastPostedImageTrack()
                        {
                            ChannelId = message.Channel.Id,
                            UserId = message.Author.Id,
                            LastRecordedPost = timeAtWhichEventWasTriggered
                        };

                        context.LastPostedImageTracks.Add(lastTrack);
                        await AwardCurrency(context, currencyGain, message);
                        return;
                    }

                    if(lastTrack.LastRecordedPost > timeAtWhichEventWasTriggered)
                    {
                        return;
                    }

                    if((timeAtWhichEventWasTriggered - lastTrack.LastRecordedPost).TotalMinutes < currencyGain.DelayBetweenAllowedImagePostInMinutes)
                    {
                        return;
                    }

                    lastTrack.LastRecordedPost = timeAtWhichEventWasTriggered;
                    await AwardCurrency(context, currencyGain, message);
                }
                finally
                {
                    DBReadWrite.ReleaseLock();
                }
                
            });

            return Task.CompletedTask;
        }

        async Task AwardCurrency(ApplicationDbContext context, TextChannelCurrencyGainImage currencyGain, SocketMessage message)
        {
            var currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.OwnerId == message.Author.Id && co.CurrencyId == currencyGain.CurrencyId);
            if (currencyOwned == null)
            {
                currencyOwned = new CurrencyOwned()
                {
                    OwnerId = message.Author.Id,
                    CurrencyId = currencyGain.CurrencyId.Value,
                    Amount = 0
                };

                context.CurrenciesOwned.Add(currencyOwned);
            }

            currencyOwned.Amount += currencyGain.AmountGainedPerImagePost;
            await context.SaveChangesAsync();
        }
        async Task AwardCurrency(ApplicationDbContext context, TextChannelCurrencyGainMessage currencyGain, SocketMessage message)
        {
            var currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.OwnerId == message.Author.Id && co.CurrencyId == currencyGain.CurrencyId);
            if (currencyOwned == null)
            {
                currencyOwned = new CurrencyOwned()
                {
                    OwnerId = message.Author.Id,
                    CurrencyId = currencyGain.CurrencyId.Value,
                    Amount = 0
                };

                context.CurrenciesOwned.Add(currencyOwned);
            }

            currencyOwned.Amount += currencyGain.AmountGainedPerMessage;
            await context.SaveChangesAsync();
        }
    }
}
