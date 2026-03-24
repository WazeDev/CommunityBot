using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using WazeBotDiscord.Classes.Roles;
using WazeBotDiscord.Classes.Servers;

namespace WazeBotDiscord.Utilities
{
    public class RequireChampInNationalGuildAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckRequirementsAsync(
             IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();
            if (appInfo.Owner.Id == context.User.Id)
                return PreconditionResult.FromSuccess();

            if (context.Guild.Id != Servers.National)
                return PreconditionResult.FromError("That command can only be used on the national server.");

            if (((SocketGuildUser)context.User).Roles.Any(r => r.Id == GlobalChamp.Ids[Servers.National] || r.Id == LocalChamp.Ids[Servers.National]))
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError($"{context.User.Mention}: " + "You must be a champ to use that command.");
        }
    }
}
