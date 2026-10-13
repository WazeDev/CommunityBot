using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WazeBotDiscord.Utilities
{
        public static class RoleSyncHelpers
        {
            public static async Task<SyncedRoleStatus> ToggleSyncedRolesAsync(
                IUser userIn,
                IReadOnlyDictionary<ulong, ulong> guildRoles,
                IInteractionContext context)
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
                    await RemoveSyncedRolesAsync(user, guildRoles, context);
                    return SyncedRoleStatus.Removed;
                }
                else
                {
                    await AddSyncedRolesAsync(user, guildRoles, context.Client);
                    return SyncedRoleStatus.Added;
                }
            }

            public static async Task RemoveSyncedRolesAsync(
                SocketGuildUser guildUser,
                IReadOnlyDictionary<ulong, ulong> guildRole,
                IInteractionContext context)
            {
                foreach (var (guild, member) in await GetUserMembershipsAsync(guildUser.Id, context.Client, guildRole.Keys))
                {
                    var roleId = guildRole[guild.Id];
                    try
                    {
                        if (member.RoleIds.Contains(roleId))
                            await member.RemoveRoleAsync(roleId);
                    }
                    catch
                    {
                        await context.Channel.SendMessageAsync($"Error removing {roleId} from {guild.Id} ({guild.Name})");
                    }
                }
            }

            public static async Task AddSyncedRolesAsync(
                SocketGuildUser guildUser,
                IReadOnlyDictionary<ulong, ulong> guildRole,
                IDiscordClient client)
            {
                foreach (var (guild, member) in await GetUserMembershipsAsync(guildUser.Id, client, guildRole.Keys))
                {
                    var roleId = guildRole[guild.Id];
                    if (!member.RoleIds.Contains(roleId))
                        await member.AddRoleAsync(roleId);
                }
            }

            /// <summary>
            /// Finds every server the bot shares with the user, along with the user's member record there.
            /// Fetches each member individually (cache first, then REST), so it works without the
            /// Guild Members intent. Pass onlyGuildIds to skip servers that don't matter.
            /// </summary>
            public static async Task<List<(SocketGuild Guild, IGuildUser Member)>> GetUserMembershipsAsync(
                ulong userId,
                IDiscordClient client,
                IEnumerable<ulong> onlyGuildIds = null)
            {
                var wanted = onlyGuildIds == null ? null : new HashSet<ulong>(onlyGuildIds);
                var result = new List<(SocketGuild, IGuildUser)>();

                foreach (var g in (await client.GetGuildsAsync()).OfType<SocketGuild>())
                {
                    if (wanted != null && !wanted.Contains(g.Id))
                        continue;

                    IGuildUser member;
                    try
                    {
                        member = await ((IGuild)g).GetUserAsync(userId, CacheMode.AllowDownload);
                    }
                    catch
                    {
                        continue;
                    }

                    if (member != null)
                        result.Add((g, member));
                }

                return result;
            }
        }
    

    public enum SyncedRoleStatus
    {
        Added,
        Removed,
        NotConfigured
    }
}
