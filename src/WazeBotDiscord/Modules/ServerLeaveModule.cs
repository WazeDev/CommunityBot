using Discord.Commands;
using System.Threading.Tasks;
using WazeBotDiscord.Utilities;
using WazeBotDiscord.ServerLeave;

namespace WazeBotDiscord.Modules
{
    [Group("serverleave")]
    public class ServerLeaveModule : ModuleBase
    {
        readonly ServerLeaveService _serverLeaveSvc;

        public ServerLeaveModule(ServerLeaveService lookupSvc)
        {
            _serverLeaveSvc = lookupSvc;
        }

        [Command]
        public async Task ListAll()
        {
            var result = _serverLeaveSvc.GetExistingLeaveChannel(Context.Guild.Id);

            if (result == null)
                await ReplyAsync("No channel has been set for this server.");
            else
                await ReplyAsync($"Channel <#{result.ChannelId}> set for this server.");
        }

        [Command("add"), Priority(10)]
        [RequireAdmin]
        public async Task Add([Remainder] string channelId = null)
        {
            if (channelId == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a channel ID.");
                return;
            }
            ulong channelID = channelId.GetChannelIDFromString();

            var result = await _serverLeaveSvc.AddChannelIDAsync(Context.Guild.Id, channelID);

            var reply = $"{Context.Message.Author.Mention}: channel added.";
            if (result == false)
                reply = $"{Context.Message.Author.Mention}: channel modified.";

            await ReplyAsync(reply);
        }

        [Command("remove"), Priority(9)]
        [RequireAdmin]
        public async Task Remove([Remainder] string channelId = null)
        {
            var removed = await _serverLeaveSvc.RemoveServerChannelAsync(Context.Guild.Id);

            if (removed)
                await ReplyAsync("Channel removed.");
            else
                await ReplyAsync("No channel was set for this server.");

        }
    }
}
