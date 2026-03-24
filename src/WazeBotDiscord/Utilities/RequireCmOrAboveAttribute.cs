using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using WazeBotDiscord.Classes.Roles;

namespace WazeBotDiscord.Utilities
{
    public class RequireCmOrAboveAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckRequirementsAsync(
             IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();
            if (appInfo.Owner.Id == context.User.Id)
                return PreconditionResult.FromSuccess();

            var guild = context.Guild as SocketGuild;
            var exists = CountryManager.Ids.TryGetValue(guild.Id, out var roleId);
            if (!exists)
                return PreconditionResult.FromError("This server is not configured for that command.");

            var cmRole = guild.GetRole(roleId);

            if (((SocketGuildUser)context.User).Hierarchy >= cmRole.Position)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError($"{context.User.Mention}: " + "You must be CM or above (which includes RC/ARC) to use that command.");
        }
    }
}
