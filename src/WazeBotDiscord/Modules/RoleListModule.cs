using Discord;
using Discord.Interactions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("roles", "Role management commands")]
    [RequireAdmin]
    public class RoleListModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("list", "List all roles on this server")]
        public async Task ListRoles()
        {
            await DeferAsync(ephemeral: true);

            var orderedRoles = Context.Guild.Roles.OrderByDescending(r => r.Position);
            var replySb = new StringBuilder("__Roles__\n");

            foreach (var role in orderedRoles)
            {
                var roleName = role.Name == "@everyone" ? "(@)everyone" : role.Name;
                var roleLine = $"{roleName}: {role.Id}\n";

                if (replySb.Length + roleLine.Length >= 2000)
                {
                    await FollowupAsync(replySb.ToString().TrimEnd(), ephemeral: true);
                    replySb.Clear();
                }

                replySb.AppendLine(roleLine);
            }

            if (replySb.Length > 0)
                await FollowupAsync(replySb.ToString().TrimEnd(), ephemeral: true);
        }
    }
}