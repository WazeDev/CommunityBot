using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using WazeBotDiscord.Classes.Roles;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    public class RoleSyncModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("cm", "Toggle Country Manager role for a user")]
        [RequireCmOrAbove]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleCm([Summary("user", "The user to toggle CM for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            SyncedRoleStatus result = SyncedRoleStatus.NotConfigured;
            try
            {
                result = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, CountryManager.Ids, Context);
            }
            catch
            {
                await FollowupAsync("Error setting CM role on this server.");
                return;
            }

            if (result == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, LargeAreaManager.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, AreaManager.Ids, Context);
                await FollowupAsync($"{user.Mention}: Added CM, removed LAM and AM (if applicable).");
            }
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed CM.");
        }

        [SlashCommand("sm", "Toggle State Manager role for a user")]
        [RequireSmOrAbove]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleSm([Summary("user", "The user to toggle SM for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, StateManager.Ids, Context);

            if (result == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, LargeAreaManager.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, AreaManager.Ids, Context);
                await FollowupAsync($"{user.Mention}: Added SM, removed LAM and AM (if applicable).");
            }
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed SM.");
        }

        [SlashCommand("lam", "Toggle Large Area Manager role for a user")]
        [RequireSmOrAbove]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleLam([Summary("user", "The user to toggle LAM for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, LargeAreaManager.Ids, Context);

            if (result == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, StateManager.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, AreaManager.Ids, Context);
                await RoleSyncHelpers.AddSyncedRolesAsync((SocketGuildUser)user, LargeAreaManager.Ids, Context.Client);
                await FollowupAsync($"{user.Mention}: Added LAM, removed SM and AM (if applicable).");
            }
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed LAM.");
        }

        [SlashCommand("am", "Toggle Area Manager role for a user")]
        [RequireSmOrAbove]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleAm([Summary("user", "The user to toggle AM for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, AreaManager.Ids, Context);

            if (result == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, StateManager.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, LargeAreaManager.Ids, Context);
                await RoleSyncHelpers.AddSyncedRolesAsync((SocketGuildUser)user, AreaManager.Ids, Context.Client);
                await FollowupAsync($"{user.Mention}: Added AM, removed SM and LAM (if applicable).");
            }
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed AM.");
        }

        [SlashCommand("mentor", "Toggle Mentor role for a user")]
        [RequireSmOrAbove]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleMentor([Summary("user", "The user to toggle Mentor for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, Mentor.Ids, Context);

            if (result == SyncedRoleStatus.Added)
                await FollowupAsync($"{user.Mention}: Added mentor.");
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed mentor.");
        }

        [SlashCommand("l6", "Toggle Level 6 role for a user")]
        [RequireChampInNationalL6InGlobal]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleL6([Summary("user", "The user to toggle L6 for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, Level6.Ids, Context);

            if (result == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level5.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level4.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level3.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level2.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level1.Ids, Context);
                await FollowupAsync($"{user.Mention}: Added L6, removed other level roles.");
            }
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed L6.");
        }

        [SlashCommand("l5", "Toggle Level 5 role for a user")]
        [RequireSmOrAbove]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleL5([Summary("user", "The user to toggle L5 for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, Level5.Ids, Context);

            if (result == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level6.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level4.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level3.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level2.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level1.Ids, Context);
                await FollowupAsync($"{user.Mention}: Added L5, removed other level roles.");
            }
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed L5.");
        }

        [SlashCommand("l4", "Toggle Level 4 role for a user")]
        [RequireSmOrAbove]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleL4([Summary("user", "The user to toggle L4 for")] IUser user)
        {
            if (IsSelf(user))
            {
                await RespondAsync("You can't change this role for yourself.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            var result = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, Level4.Ids, Context);

            if (result == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level6.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level5.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level3.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level2.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level1.Ids, Context);
                await FollowupAsync($"{user.Mention}: Added L4, removed other level roles.");
            }
            else if (result == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed L4.");
        }

        [SlashCommand("l3", "Toggle Level 3 role for yourself or another user")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleL3([Summary("user", "The user to toggle L3 for (leave empty for yourself)")] IUser user = null)
        {
            user ??= Context.User;

            if (user != Context.User)
            {
                var precondition = new RequireSmOrAboveAttribute();
                var result = await precondition.CheckRequirementsAsync(Context, null, null);
                if (!result.IsSuccess)
                {
                    await RespondAsync(result.ErrorReason, ephemeral: true);
                    return;
                }
            }

            await DeferAsync(ephemeral: true);
            var toggleResult = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, Level3.Ids, Context);

            if (toggleResult == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level6.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level5.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level4.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level2.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level1.Ids, Context);
                await FollowupAsync($"{user.Mention}: Added L3, removed other level roles.");
            }
            else if (toggleResult == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed L3.");
        }

        [SlashCommand("l2", "Toggle Level 2 role for yourself or another user")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleL2([Summary("user", "The user to toggle L2 for (leave empty for yourself)")] IUser user = null)
        {
            user ??= Context.User;

            if (user != Context.User)
            {
                var precondition = new RequireSmOrAboveAttribute();
                var result = await precondition.CheckRequirementsAsync(Context, null, null);
                if (!result.IsSuccess)
                {
                    await RespondAsync(result.ErrorReason, ephemeral: true);
                    return;
                }
            }

            await DeferAsync(ephemeral: true);
            var toggleResult = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, Level2.Ids, Context);

            if (toggleResult == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level6.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level5.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level4.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level3.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level1.Ids, Context);
                await FollowupAsync($"{user.Mention}: Added L2, removed other level roles.");
            }
            else if (toggleResult == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed L2.");
        }

        [SlashCommand("l1", "Toggle Level 1 role for yourself or another user")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ToggleL1([Summary("user", "The user to toggle L1 for (leave empty for yourself)")] IUser user = null)
        {
            user ??= Context.User;

            if (user != Context.User)
            {
                var precondition = new RequireSmOrAboveAttribute();
                var result = await precondition.CheckRequirementsAsync(Context, null, null);
                if (!result.IsSuccess)
                {
                    await RespondAsync(result.ErrorReason, ephemeral: true);
                    return;
                }
            }

            await DeferAsync(ephemeral: true);
            var toggleResult = await RoleSyncHelpers.ToggleSyncedRolesAsync(user, Level1.Ids, Context);

            if (toggleResult == SyncedRoleStatus.Added)
            {
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level6.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level5.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level4.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level3.Ids, Context);
                await RoleSyncHelpers.RemoveSyncedRolesAsync((SocketGuildUser)user, Level2.Ids, Context);
                await FollowupAsync($"{user.Mention}: Added L1, removed other level roles.");
            }
            else if (toggleResult == SyncedRoleStatus.Removed)
                await FollowupAsync($"{user.Mention}: Removed L1.");
        }

        bool IsSelf(IUser target) => Context.User.Id == target.Id;
    }
}