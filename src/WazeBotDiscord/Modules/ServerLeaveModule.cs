using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.ServerLeave;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("serverleave", "Server leave message commands")]
    public class ServerLeaveModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly ServerLeaveService _serverLeaveSvc;

        public ServerLeaveModule(ServerLeaveService serverLeaveSvc)
        {
            _serverLeaveSvc = serverLeaveSvc;
        }

        [SlashCommand("get", "Get the leave notification channel for this server")]
        public async Task ListAll()
        {
            var result = await _serverLeaveSvc.GetExistingLeaveChannel(Context.Guild.Id);
            if (result == null)
                await RespondAsync("No channel has been set for this server.", ephemeral: true);
            else
                await RespondAsync($"Channel <#{result.ChannelId}> set for this server.", ephemeral: true);
        }

        [SlashCommand("add", "Add or update the leave notification channel for this server")]
        [RequireAdmin]
        public async Task Add([Summary("channel", "The channel to send leave notifications to")] IChannel channel)
        {
            var result = await _serverLeaveSvc.AddChannelIDAsync(Context.Guild.Id, channel.Id);
            await RespondAsync(result ? "Channel added." : "Channel modified.", ephemeral: true);
        }

        [SlashCommand("remove", "Remove the leave notification channel for this server")]
        [RequireAdmin]
        public async Task Remove()
        {
            var removed = await _serverLeaveSvc.RemoveServerChannelAsync(Context.Guild.Id);
            await RespondAsync(removed ? "Channel removed." : "No channel was set for this server.", ephemeral: true);
        }
    }
}