using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using WazeBotDiscord.Classes.Roles;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    public class RegionRoleModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("region", "Toggle the region-specific role for a user")]
        [RequireSmOrAbove]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleRegion([Summary("user", "The user to toggle the region role for")] IUser userIn)
        {
            var exists = Region.Ids.TryGetValue(Context.Guild.Id, out var roleId);
            if (!exists)
            {
                await RespondAsync("This server does not have a region-specific role.", ephemeral: true);
                return;
            }

            var user = (SocketGuildUser)userIn;
            var role = Context.Guild.GetRole(roleId);

            if (user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                await RespondAsync($"{user.Mention}: Region-specific role removed.");
            }
            else
            {
                await user.AddRoleAsync(role);
                await RespondAsync($"{user.Mention}: Region-specific role added.");
            }
        }
    }
}