using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using WazeBotDiscord.ChannelSync;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("channelsync", "Channel sync commands")]
    [RequireAppOwner]
    public class ChannelSyncModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly ChannelSyncService _channelSyncSvc;

        public ChannelSyncModule(ChannelSyncService channelSyncSvc)
        {
            _channelSyncSvc = channelSyncSvc;
        }

        [SlashCommand("add", "Sync two channels together")]
        public async Task Add(
            [Summary("channel1", "First channel to sync")] IChannel channel1,
            [Summary("channel2", "Second channel to sync")] IChannel channel2)
        {
            if (channel1.Id == channel2.Id)
            {
                await RespondAsync("You cannot sync a channel to itself.", ephemeral: true);
                return;
            }

            var existing1 = _channelSyncSvc.getSyncChannels(channel1.Id);
            if (existing1 != null)
            {
                await RespondAsync($"<#{channel1.Id}> ({channel1.Id}) is already syncing with a channel. A channel can only sync to one other channel.", ephemeral: true);
                return;
            }

            var existing2 = _channelSyncSvc.getSyncChannels(channel2.Id);
            if (existing2 != null)
            {
                await RespondAsync($"<#{channel2.Id}> ({channel2.Id}) is already syncing with a channel. A channel can only sync to one other channel.", ephemeral: true);
                return;
            }

            var result = await _channelSyncSvc.AddChannelSync(channel1.Id, channel2.Id, Context.User.Id, DateTime.UtcNow, Context.User.Username);
            if (result)
                await RespondAsync($"{Context.User.Mention} Channels synced.", ephemeral: true);
        }

        [SlashCommand("remove", "Remove a channel sync")]
        public async Task Remove([Summary("channel", "Channel to remove sync for")] IChannel channel)
        {
            var channels = _channelSyncSvc.getSyncChannels(channel.Id);
            if (channels == null)
            {
                await RespondAsync($"Channel <#{channel.Id}> is not synced to any other channels.", ephemeral: true);
                return;
            }

            var result = await _channelSyncSvc.RemoveChannelSync(channel.Id);
            if (result)
                await RespondAsync($"{Context.User.Mention} sync removed for channel <#{channel.Id}> ({channel.Id})", ephemeral: true);
            else
                await RespondAsync($"{Context.User.Mention} failed to remove channel sync.", ephemeral: true);
        }
    }
}