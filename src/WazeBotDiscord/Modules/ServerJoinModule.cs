using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.ServerJoin;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("serverjoin", "Server join message commands")]
    [RequireChampInUSAdminInGlobalScriptsAttribute]
    public class ServerJoinModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly ServerJoinService _serverJoinSvc;

        public ServerJoinModule(ServerJoinService serverJoinSvc)
        {
            _serverJoinSvc = serverJoinSvc;
        }

        [SlashCommand("get", "Get the join message for this server")]
        public async Task GetMessage()
        {
            var message = await _serverJoinSvc.GetExistingJoinMessage(Context.Guild.Id);
            if (message == null)
                await RespondAsync("No join message has been set for this server.", ephemeral: true);
            else
                await RespondAsync(message.JoinMessage, ephemeral: true);
        }

        [SlashCommand("add", "Add or update the join message for this server")]
        public async Task Add([Summary("message", "The join message to display")] string message)
        {
            var result = await _serverJoinSvc.AddServerMessage(Context.Guild.Id, message);
            await RespondAsync(result ? "Server join message added." : "Server join message modified.", ephemeral: true);
        }

        [SlashCommand("remove", "Remove the join message for this server")]
        public async Task Remove()
        {
            var removed = await _serverJoinSvc.RemoveServerMessage(Context.Guild.Id);
            await RespondAsync(removed
                ? $"Removed server join message from {Context.Guild.Name}."
                : "No server join message was set for this server.", ephemeral: true);
        }
    }
}