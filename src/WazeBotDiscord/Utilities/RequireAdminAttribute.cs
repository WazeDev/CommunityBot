using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using WazeBotDiscord.Classes.Roles;

namespace WazeBotDiscord.Utilities
{
    public class RequireAdminAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckRequirementsAsync(
            IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();
            if (appInfo.Owner.Id == context.User.Id)
                return PreconditionResult.FromSuccess();

            var guild = context.Guild as SocketGuild;
            var exists = Admin.Ids.TryGetValue(guild.Id, out var roleId);
            if (!exists)
                return PreconditionResult.FromError("This server is not configured for that command.");

            var adminRole = guild.GetRole(roleId);

            if (((SocketGuildUser)context.User).Hierarchy >= adminRole.Position)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError($"{context.User.Mention}: " + "You must be an administrator to use that command.");
        }
    }
}
