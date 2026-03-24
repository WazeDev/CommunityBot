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
    class RequireAdminModeratorInGlobal : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckRequirementsAsync(
             IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();
            if (appInfo.Owner.Id == context.User.Id)
                return PreconditionResult.FromSuccess();

            if (context.Guild.Id != Servers.GlobalMapraid) //Global server
                return PreconditionResult.FromError("That command can only be used on the global server.");

            //Global server and Admin
            if ((context.Guild.Id == Servers.GlobalMapraid && ((SocketGuildUser)context.User).Roles.Any(r => (r.Id == Admin.Ids[Servers.GlobalMapraid] || r.Id == Moderator.Ids[Servers.GlobalMapraid]))))
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError($"{context.User.Mention}: " + "You must be an admin to use that command.");
        }
    }
}
