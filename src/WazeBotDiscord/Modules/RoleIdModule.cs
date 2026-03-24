using Discord;
using Discord.Interactions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("roleid", "Role ID lookup commands")]
    [RequireOwner]
    public class RoleIdModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("get", "Get the ID of a specific role")]
        public async Task GetSpecific([Summary("role", "The role to look up")] IRole role)
        {
            await RespondAsync($"Role {role.Name}: {role.Id}", ephemeral: true);
        }

        [SlashCommand("wazeall", "Get IDs of all Waze roles on this server")]
        public async Task GetAll()
        {
            var reply = new StringBuilder("__Waze roles on this server__");
            foreach (var name in WazeRoleNames.RoleNames)
            {
                var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == name);
                var idString = role == null ? "(role not present)" : role.Id.ToString();
                reply.Append($"\n{name}: {idString}");
            }
            await RespondAsync(reply.ToString(), ephemeral: true);
        }
    }
}