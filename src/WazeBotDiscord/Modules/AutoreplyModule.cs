using Discord;
using Discord.Interactions;
using System;
using System.Linq;
using System.Threading.Tasks;
using WazeBotDiscord.Autoreplies;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("autoreply", "Autoreply commands")]
    public class AutoreplyModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly AutoreplyService _arService;

        public AutoreplyModule(AutoreplyService arService)
        {
            _arService = arService;
        }

        [SlashCommand("list", "List all autoreplies for this channel")]
        public async Task ListAll()
        {
            var channelReplies = (await _arService.GetChannelAutoreplies(Context.Channel.Id)).Select(r => r.Trigger);
            var guildReplies = (await _arService.GetGuildAutoreplies(Context.Guild.Id)).Select(r => r.Trigger);
            var globalReplies = (await _arService.GetGlobalAutoreplies()).Select(r => r.Trigger);

            guildReplies = guildReplies.Select(r => channelReplies.Contains(r) ? "~~" + r + "~~" : r);
            globalReplies = globalReplies.Select(r => channelReplies.Contains(r) ? "~~" + r + "~~" : r);
            globalReplies = globalReplies.Select(r => guildReplies.Contains(r) ? "~~" + r + "~~" : r);

            var channelRepliesString = string.Join(", ", channelReplies);
            var guildRepliesString = string.Join(", ", guildReplies);
            var globalRepliesString = string.Join(", ", globalReplies);

            if (string.IsNullOrEmpty(channelRepliesString)) channelRepliesString = "_(none)_";
            if (string.IsNullOrEmpty(guildRepliesString)) guildRepliesString = "_(none)_";
            if (string.IsNullOrEmpty(globalRepliesString)) globalRepliesString = "_(none)_";

            var msg = $"__Channel__\n{channelRepliesString}\n\n__Server__\n{guildRepliesString}\n\n__Global__\n{globalRepliesString}";

            if (msg.Length > 1500)
                msg = msg.Substring(0, 1500) + "\n\n_(autoreply list too long, sorry!)_";

            await RespondAsync(msg, ephemeral: true);
        }

        [SlashCommand("add-channel", "Add an autoreply to this channel")]
        [RequireSmOrAbove]
        public async Task AddToChannel(
            [Summary("trigger", "The trigger word")] string trigger,
            [Summary("reply", "The reply message")] string reply)
        {
            if (!await ValidateAutoreply(trigger, reply)) return;

            if (trigger.StartsWith("!"))
                trigger = trigger.Substring(1);

            var autoreply = new Autoreply
            {
                ChannelId = Context.Channel.Id,
                GuildId = Context.Guild.Id,
                Trigger = trigger.ToLowerInvariant(),
                Reply = reply,
                AddedById = Context.User.Id,
                AddedAt = DateTime.UtcNow
            };

            var newlyAdded = await _arService.AddOrModifyAutoreply(autoreply);
            await RespondAsync($"Channel autoreply {(newlyAdded ? "added" : "edited")}. {autoreply.Trigger} | {autoreply.Reply}", ephemeral: true);
        }

        [SlashCommand("add-server", "Add an autoreply to this server")]
        [RequireSmOrAboveAdminInGlobal]
        public async Task AddToServer(
            [Summary("trigger", "The trigger word")] string trigger,
            [Summary("reply", "The reply message")] string reply)
        {
            if (!await ValidateAutoreply(trigger, reply)) return;

            if (trigger.StartsWith("!"))
                trigger = trigger.Substring(1);

            var autoreply = new Autoreply
            {
                ChannelId = 1,
                GuildId = Context.Guild.Id,
                Trigger = trigger.ToLowerInvariant(),
                Reply = reply,
                AddedById = Context.User.Id,
                AddedAt = DateTime.UtcNow
            };

            var newlyAdded = await _arService.AddOrModifyAutoreply(autoreply);
            await RespondAsync($"Server autoreply {(newlyAdded ? "added" : "edited")}. {autoreply.Trigger} | {autoreply.Reply}", ephemeral: true);
        }

        [SlashCommand("add-global", "Add a global autoreply")]
        [RequireChampInNationalAdminInGlobalAttribute]
        public async Task AddToGlobal(
            [Summary("trigger", "The trigger word")] string trigger,
            [Summary("reply", "The reply message")] string reply)
        {
            if (!await ValidateAutoreply(trigger, reply)) return;

            if (trigger.StartsWith("!"))
                trigger = trigger.Substring(1);

            var autoreply = new Autoreply
            {
                ChannelId = 1,
                GuildId = 1,
                Trigger = trigger.ToLowerInvariant(),
                Reply = reply,
                AddedById = Context.User.Id,
                AddedAt = DateTime.UtcNow
            };

            var newlyAdded = await _arService.AddOrModifyAutoreply(autoreply);
            await RespondAsync($"Global autoreply {(newlyAdded ? "added" : "edited")}. {autoreply.Trigger} | {autoreply.Reply}", ephemeral: true);
        }

        [SlashCommand("remove-channel", "Remove an autoreply from this channel")]
        [RequireSmOrAbove]
        public async Task RemoveFromChannel([Summary("trigger", "The trigger to remove")] string trigger)
        {
            var removed = await _arService.RemoveAutoreply(Context.Channel.Id, Context.Guild.Id, trigger.ToLowerInvariant());
            await RespondAsync(removed ? "Channel autoreply removed." : "Channel autoreply does not exist.", ephemeral: true);
        }

        [SlashCommand("remove-server", "Remove an autoreply from this server")]
        [RequireSmOrAboveAdminInGlobal]
        public async Task RemoveFromServer([Summary("trigger", "The trigger to remove")] string trigger)
        {
            var removed = await _arService.RemoveAutoreply(1, Context.Guild.Id, trigger.ToLowerInvariant());
            await RespondAsync(removed ? "Server autoreply removed." : "Server autoreply does not exist.", ephemeral: true);
        }

        [SlashCommand("remove-global", "Remove a global autoreply")]
        [RequireChampInNationalAdminInGlobalAttribute]
        public async Task RemoveFromGlobal([Summary("trigger", "The trigger to remove")] string trigger)
        {
            var removed = await _arService.RemoveAutoreply(1, 1, trigger.ToLowerInvariant());
            await RespondAsync(removed ? "Global autoreply removed." : "Global autoreply does not exist.", ephemeral: true);
        }

        async Task<bool> ValidateAutoreply(string trigger, string reply)
        {
            if (trigger.Length > 30)
            {
                await RespondAsync("Trigger is too long. Trigger must be <= 30 characters.", ephemeral: true);
                return false;
            }
            if (reply.Length > 1000)
            {
                await RespondAsync("Reply is too long.", ephemeral: true);
                return false;
            }
            return true;
        }
    }
}