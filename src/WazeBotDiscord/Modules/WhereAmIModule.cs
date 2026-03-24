using Discord.Interactions;
using System.Threading.Tasks;

namespace WazeBotDiscord.Modules
{
    public class WhereAmIModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("whereami", "Get the current channel and server IDs")]
        public async Task WhereAmI()
        {
            await RespondAsync($"Channel ID: `{Context.Channel.Id}`\nServer ID: `{Context.Guild.Id}`", ephemeral: true);
        }
    }
}