using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using WazeBotDiscord.Classes.Roles;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    public class RoleNoSyncModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("worldcup", "Toggle the World Cup role for yourself")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleWorldCup()
        {
            await DeferAsync(ephemeral: true);
            var result = await RoleAssignmentHelper.ToggleRoleAsync(Context.User, WorldCup.Ids, Context);

            if (result == SyncedRoleStatus.Added)
                await FollowupAsync($"{Context.User.Mention}: Added worldcup role. Join the discussion in <#448908730243743754>.");
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{Context.User.Mention}: Removed worldcup role.");
        }

        [SlashCommand("iosbeta", "Toggle the iOS beta role for a user")]
        [RequireAdminModeratorInGlobal]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleiOSBeta([Summary("user", "The user to toggle the iOS beta role for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleAssignmentHelper.ToggleRoleAsync(user, iosbeta.Ids, Context);

            if (result == SyncedRoleStatus.Added)
                await FollowupAsync($"{user.Mention}: Added iOS beta role.");
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed iOS beta role.");
        }

        [SlashCommand("wmebeta", "Toggle the WME beta role for a user")]
        [RequireAdminModeratorInGlobal]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleWMEBeta([Summary("user", "The user to toggle the WME beta role for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleAssignmentHelper.ToggleRoleAsync(user, WMEBeta.Ids, Context);

            if (result == SyncedRoleStatus.Added)
                await FollowupAsync($"{user.Mention}: Added WME beta role.");
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed WME beta role.");
        }

        [SlashCommand("androidbeta", "Toggle the Android beta role for a user")]
        [RequireAdminModeratorInGlobal]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleAndroidBeta([Summary("user", "The user to toggle the Android beta role for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleAssignmentHelper.ToggleRoleAsync(user, AndroidBeta.Ids, Context);

            if (result == SyncedRoleStatus.Added)
                await FollowupAsync($"{user.Mention}: Added Android beta role.");
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed Android beta role.");
        }

        bool IsSelf(IUser target) => Context.User.Id == target.Id;
    }
}