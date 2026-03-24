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
    class RequireChampInNationalL6InGlobalAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckRequirementsAsync(
             IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();
            if (appInfo.Owner.Id == context.User.Id)
                return PreconditionResult.FromSuccess();

            if (!(context.Guild.Id == Servers.National || context.Guild.Id == Servers.GlobalMapraid))
                return PreconditionResult.FromError("That command can only be used on the national or global servers.");

            //Check if champ on national server
            if (context.Guild.Id == Servers.National && ((SocketGuildUser)context.User).Roles.Any(r => r.Id == GlobalChamp.Ids[Servers.National] || r.Id == LocalChamp.Ids[Servers.National])) //Champ roles on national
                return PreconditionResult.FromSuccess();

            //Check if L6 on global server
            if (context.Guild.Id == Servers.GlobalMapraid && (((SocketGuildUser)context.User).Roles.Any(r => r.Id == Level6.Ids[Servers.GlobalMapraid])))
                return PreconditionResult.FromSuccess();

            if (context.Guild.Id == Servers.National)
                return PreconditionResult.FromError($"{context.User.Mention}: " + "You must be a champ to use that command.");
            else
                return PreconditionResult.FromError($"{context.User.Mention}: " + "You must be L6 to use that command.");
        }
    }
}
