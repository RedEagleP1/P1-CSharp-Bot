using Bot.SlashCommands;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.EventHandlers
{
	public class VoiceStateUpdateHandler : IEventHandler
	{
        readonly DiscordSocketClient client;
        public VoiceStateUpdateHandler(DiscordSocketClient client)
        {
            this.client = client;
        }
        public void Subscribe()
		{
            client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
		}

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            var timeAtWhichEventWasTriggered = DateTime.Now;
            _ = Task.Run(async () =>
            {
                await DBReadWrite.LockReadWrite();
                try
                {
                    var context = DBContextFactory.GetNewContext();
                    var existingTrack = await context.VoiceChannelTracks.FirstOrDefaultAsync(t => t.UserId == user.Id);
                    if(existingTrack == null)
                    {
                        existingTrack = new VoiceChannelTrack()
                        {
                            UserId = user.Id,
                            ChannelId = after.VoiceChannel.Id,
                            IsMuteOrDeafen = after.IsSelfDeafened || after.IsSelfMuted || after.IsDeafened || after.IsMuted,
                            LastRecorded = timeAtWhichEventWasTriggered
                        };

                        context.VoiceChannelTracks.Add(existingTrack);
                        await context.SaveChangesAsync();
                        return;
                    }

                    if (existingTrack.LastRecorded > timeAtWhichEventWasTriggered)
                    {
                        return;
                    }

                    if (before.VoiceChannel == null)
                    {
                        existingTrack.LastRecorded = timeAtWhichEventWasTriggered;
                        existingTrack.IsMuteOrDeafen = after.IsSelfDeafened || after.IsSelfMuted || after.IsDeafened || after.IsMuted;
                        existingTrack.ChannelId = after.VoiceChannel?.Id;
                        await context.SaveChangesAsync();
                        return;
                    }

                    if (existingTrack.ChannelId != null)
                    {
                        var time = timeAtWhichEventWasTriggered - existingTrack.LastRecorded;
                        var currencyGain = await context.VoiceChannelCurrencyGains.FirstOrDefaultAsync(cg => cg.ChannelId == existingTrack.ChannelId);
                        if (currencyGain.IsEnabled && currencyGain.CurrencyId != null)
                        {
                            var amountPerHour = existingTrack.IsMuteOrDeafen ? currencyGain.AmountGainedPerHourWhenMuteOrDeaf : currencyGain.AmountGainedPerHourWhenSpeaking;
                            var totalAmount = (float)(amountPerHour * time.TotalHours);

                            var currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.OwnerId == user.Id && co.CurrencyId == currencyGain.CurrencyId);
                            if (currencyOwned == null)
                            {
                                currencyOwned = new CurrencyOwned()
                                {
                                    OwnerId = user.Id,
                                    CurrencyId = currencyGain.CurrencyId.Value,
                                    Amount = 0
                                };

                                context.CurrenciesOwned.Add(currencyOwned);
                            }

                            currencyOwned.Amount += totalAmount;
                        }
                    }

                    existingTrack.LastRecorded = timeAtWhichEventWasTriggered;
                    existingTrack.IsMuteOrDeafen = after.IsSelfDeafened || after.IsSelfMuted || after.IsDeafened || after.IsMuted;
                    existingTrack.ChannelId = after.VoiceChannel?.Id;
                    await context.SaveChangesAsync();
                }
                finally
                {
                    DBReadWrite.ReleaseLock();
                }                
            });

            return Task.CompletedTask;
        }
    }
}
