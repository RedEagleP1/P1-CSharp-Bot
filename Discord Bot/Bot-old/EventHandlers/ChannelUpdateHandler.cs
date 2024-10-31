using Bot.OneTimeRegister;
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
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bot.EventHandlers
{
	public class ChannelUpdateHandler : IEventHandler
    {
        readonly DiscordSocketClient client;
        public ChannelUpdateHandler(DiscordSocketClient client)
        {
            this.client = client;
        }

        public void Subscribe()
        {
            client.ChannelCreated += OnChannelCreated;
            client.ChannelDestroyed += OnChannelDestroyed;
            client.ChannelUpdated += OnChannelUpdated;
        }

        private Task OnChannelUpdated(SocketChannel before, SocketChannel after)
        {
            _ = Task.Run(async () =>
            {
                if(before is not SocketGuildChannel beforeChannel || after is not SocketGuildChannel afterChannel)
                {
                    return;
                }

                if(beforeChannel.Name == afterChannel.Name)
                {
                    return;
                }

                var channelType = after.GetChannelType();
                if(channelType == null)
                {
                    return;
                }

                var context = DBContextFactory.GetNewContext();
                switch (channelType.Value)
                {
                    case ChannelType.Voice:
                        var voiceChannel = await context.VoiceChannelCurrencyGains.FirstOrDefaultAsync(v => v.ChannelId == afterChannel.Id);
                        if (voiceChannel == null)
                        {
                            return;
                        }
                        voiceChannel.ChannelName = afterChannel.Name;
                        await context.SaveChangesAsync();
                        break;
                    default:
                        break;
                }               
                
            });

            return Task.CompletedTask;
        }

        private Task OnChannelDestroyed(SocketChannel channel)
        {
            _ = Task.Run(async () =>
            {
                var channelType = channel.GetChannelType();
                if (channelType == null)
                {
                    return;
                }

                var context = DBContextFactory.GetNewContext();
                switch (channelType.Value)
                {
                    case ChannelType.Voice:
                        var voiceChannel = await context.VoiceChannelCurrencyGains.FirstOrDefaultAsync(v => v.ChannelId == channel.Id);
                        if (voiceChannel == null)
                        {
                            return;
                        }
                        context.VoiceChannelCurrencyGains.Remove(voiceChannel);
                        await context.SaveChangesAsync();
                        break;
                    default:
                        break;
                }
            });

            return Task.CompletedTask;
        }

        private Task OnChannelCreated(SocketChannel channel)
        {
            _ = Task.Run(async () =>
            {                
                if (channel is not SocketGuildChannel guildChannel)
                {
                    return;
                }

                var channelType = channel.GetChannelType();
                if (channelType == null)
                {
                    return;
                }

                var context = DBContextFactory.GetNewContext();
                switch (channelType.Value)
                {
                    case ChannelType.Voice:
                        var voiceChannel = new VoiceChannelCurrencyGain()
                        {
                            ChannelId = channel.Id,
                            GuildId = guildChannel.Guild.Id,
                            ChannelName = guildChannel.Name
                        };
                        context.VoiceChannelCurrencyGains.Add(voiceChannel);
                        await context.SaveChangesAsync();
                        break;
                    default:
                        break;
                }
            });

            return Task.CompletedTask;
        }
    }
}
