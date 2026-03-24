using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    public class AssignModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("assign", "Assign a role to a user")]
        [RequireAdminInGlobal]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AssignRoles(
            [Summary("role", "The role to assign")] IRole role,
            [Summary("user", "The user to assign the role to")] IUser user)
        {
            var guildUser = (SocketGuildUser)user;
            if (!guildUser.Roles.Contains(role))
            {
                await guildUser.AddRoleAsync(role);
                await RespondAsync($"Assigned {role.Name} to {user.Mention}.", ephemeral: true);
            }
            else
                await RespondAsync($"{user.Mention} already has the {role.Name} role.", ephemeral: true);
        }
    }
}