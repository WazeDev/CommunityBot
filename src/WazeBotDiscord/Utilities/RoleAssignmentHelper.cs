using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WazeBotDiscord.Utilities
{
    public class RoleAssignmentHelper
    {
        public static async Task<SyncedRoleStatus> ToggleRoleAsync(IUser userIn, IReadOnlyDictionary<ulong, ulong> guildRoles, IInteractionContext context)
        {
            var user = userIn as SocketGuildUser;
            var exists = guildRoles.TryGetValue(context.Guild.Id, out var roleId);
            if (!exists)
            {
                await context.Channel.SendMessageAsync("This server is not configured for that command.");
                return SyncedRoleStatus.NotConfigured;
            }

            var role = context.Guild.GetRole(roleId);

            if (user.Roles.Contains(role))
            {
                await RemoveSyncedRolesAsync(user, guildRoles, context.Guild, context.Client);
                return SyncedRoleStatus.Removed;
            }
            else
            {
                await AddSyncedRolesAsync(user, guildRoles, context.Guild, context.Client);
                return SyncedRoleStatus.Added;
            }
        }

        public static async Task AddSyncedRolesAsync(SocketGuildUser guildUser, IReadOnlyDictionary<ulong, ulong> guildRole, IGuild guild, IDiscordClient client)
        {
            var exists = guildRole.TryGetValue(guild.Id, out var roleId);
            if (!exists)
                return;

            var role = guild.GetRole(roleId);
            var thisGuildUser = await guild.GetUserAsync(guildUser.Id);
            await thisGuildUser.AddRoleAsync(role);
        }

        public static async Task RemoveSyncedRolesAsync(SocketGuildUser guildUser, IReadOnlyDictionary<ulong, ulong> guildRole, IGuild guild, IDiscordClient client)
        {
            var exists = guildRole.TryGetValue(guild.Id, out var roleId);
            if (!exists)
                return;

            var role = guild.GetRole(roleId);
            var thisGuildUser = await guild.GetUserAsync(guildUser.Id);
            await thisGuildUser.RemoveRoleAsync(role);
        }
    }
}