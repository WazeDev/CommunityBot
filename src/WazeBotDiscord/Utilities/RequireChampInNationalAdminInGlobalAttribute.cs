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
    public class RequireChampInNationalAdminInGlobalAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckRequirementsAsync(
             IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();
            if (appInfo.Owner.Id == context.User.Id)
                return PreconditionResult.FromSuccess();

            if (context.Guild.Id != Servers.National && context.Guild.Id != Servers.GlobalMapraid) //National and Global servers
                return PreconditionResult.FromError("That command can only be used on the national & global server.");

            //National server and global or local champ roles OR Global server and admin role
            if (((SocketGuildUser)context.User).Roles.Any(r => (context.Guild.Id == 300471946494214146 && (r.Id == 300494132839841792 || r.Id == 300494182403801088)) || (context.Guild.Id == Servers.GlobalMapraid && (r.Id == Admin.Ids[Servers.GlobalMapraid]))))
                return PreconditionResult.FromSuccess();

            if(context.Guild.Id == 300471946494214146)
                return PreconditionResult.FromError($"{context.User.Mention}: " + "You must be a champ to use that command.");
            else
                return PreconditionResult.FromError($"{context.User.Mention}: " + "You must be an admin to use that command.");
        }
    }
}
