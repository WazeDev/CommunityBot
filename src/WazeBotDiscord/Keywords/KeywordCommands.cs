using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;
using System.Threading.Tasks;

namespace WazeBotDiscord.Keywords
{
    [Group("keyword", "Keyword subscription commands")]
    public class KeywordModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly KeywordService _kwdSvc;
        readonly string _helpLink = "<https://wazeopedia.waze.com/wiki/USA/CommunityBot#Keyword_Subscriptions>";

        public KeywordModule(KeywordService kwdSvc)
        {
            _kwdSvc = kwdSvc;
        }

        [SlashCommand("help", "Get help with keyword subscriptions")]
        public async Task Help()
        {
            await RespondAsync($"For help with this command, see the Wazeopedia page: {_helpLink}", ephemeral: true);
        }

        [SlashCommand("test", "Test if a string matches any of your keywords")]
        public async Task Test([Summary("text", "The text to test")] string testString)
        {
            ulong guildId = Context.Guild?.Id ?? 1;
            var matches = (await _kwdSvc.CheckForKeywordAsync(testString, Context.User.Id, guildId, Context.Channel.Id))
                .Find(m => m.UserId == Context.User.Id);

            var resultSB = new StringBuilder();
            if (matches != null)
            {
                for (var i = 0; i < matches.MatchedKeywords.Count; i++)
                {
                    if (i > 0) resultSB.Append(", ");
                    resultSB.Append($"`{matches.MatchedKeywords[i]}`");
                }
            }

            await RespondAsync(resultSB.Length > 0
                ? $"Matched keyword(s): {resultSB}"
                : "No matches found.", ephemeral: true);
        }

        [SlashCommand("list", "List your keyword subscriptions")]
        public async Task List()
        {
            var keywords = await _kwdSvc.GetKeywordsForUserAsync(Context.User.Id);
            var reply = new StringBuilder();

            if (keywords.Count == 0)
                reply.Append("You have no keywords.");
            else
            {
                reply.Append("__Your Keywords__\n```");
                foreach (var k in keywords)
                    reply.Append($"\n{k.Keyword}");
                reply.Append("```");
            }

            await RespondAsync(reply.ToString(), ephemeral: true);
        }

        [SlashCommand("add", "Subscribe to a keyword")]
        public async Task Add([Summary("keyword", "The keyword to subscribe to")] string keyword)
        {
            if (keyword.Length < 2)
            {
                await RespondAsync("Your keyword must be at least 2 characters long.", ephemeral: true);
                return;
            }

            if (keyword.Length > 100)
            {
                await RespondAsync("Your keyword cannot be longer than 100 characters.", ephemeral: true);
                return;
            }

            var result = await _kwdSvc.AddKeywordAsync(Context.User.Id, keyword);
            if (result.AlreadyExisted)
            {
                await RespondAsync($"You were already subscribed to the keyword `{keyword}`. No change has been made.", ephemeral: true);
                return;
            }

            var reply = $"Added keyword `{keyword}`.";
            if (keyword.Contains(" "))
                reply += "\n\n**Note that your keyword contains spaces.** It will only match if all words are matched exactly " +
                    "as you typed them. If you meant to add these as individual keywords, please remove this entry and " +
                    "run the command separately for each individual keyword.";

            await RespondAsync(reply, ephemeral: true);
        }

        [SlashCommand("remove", "Unsubscribe from a keyword")]
        public async Task Remove([Summary("keyword", "The keyword to unsubscribe from")] string keyword)
        {
            var existed = await _kwdSvc.RemoveKeywordAsync(Context.User.Id, keyword);
            await RespondAsync(existed
                ? $"Subscription to `{keyword}` removed."
                : "You were not subscribed to that keyword. No change was made.", ephemeral: true);
        }

        [SlashCommand("ignore-server", "Ignore a keyword in a specific server")]
        public async Task IgnoreGuild(
            [Summary("keyword", "The keyword to ignore")] string keyword,
            [Summary("serverid", "The server ID to ignore the keyword in")] string guildIdStr)
        {
            if (!ulong.TryParse(guildIdStr, out var guildId))
            {
                await RespondAsync("That server ID is invalid.", ephemeral: true);
                return;
            }

            var guild = Context.Client.GetGuild(guildId);
            if (guild == null)
            {
                await RespondAsync($"That server ID is invalid. For more help, see {_helpLink}.", ephemeral: true);
                return;
            }

            switch (await _kwdSvc.IgnoreGuildsAsync(Context.User.Id, keyword, guildId))
            {
                case IgnoreResult.Success:
                    await RespondAsync($"Ignored keyword `{keyword}` in server {guild.Name}.", ephemeral: true);
                    break;
                case IgnoreResult.AlreadyIgnored:
                    await RespondAsync("You're already ignoring that keyword in that server. No change made.", ephemeral: true);
                    break;
                case IgnoreResult.NotSubscribed:
                    await RespondAsync("You're not subscribed to that keyword. No change made.", ephemeral: true);
                    break;
            }
        }

        [SlashCommand("ignore-channel", "Ignore a keyword in a specific channel")]
        public async Task IgnoreChannel(
            [Summary("keyword", "The keyword to ignore")] string keyword,
            [Summary("channel", "The channel to ignore the keyword in")] IChannel channel)
        {
            switch (await _kwdSvc.IgnoreChannelsAsync(Context.User.Id, keyword, channel.Id))
            {
                case IgnoreResult.Success:
                    var textChannel = channel as SocketTextChannel;
                    await RespondAsync($"Ignored keyword `{keyword}` in channel <#{channel.Id}>{(textChannel != null ? $" (server {textChannel.Guild.Name})" : "")}.", ephemeral: true);
                    break;
                case IgnoreResult.AlreadyIgnored:
                    await RespondAsync("You're already ignoring that keyword in that channel. No change made.", ephemeral: true);
                    break;
                case IgnoreResult.NotSubscribed:
                    await RespondAsync("You're not subscribed to that keyword. No change made.", ephemeral: true);
                    break;
            }
        }

        [SlashCommand("ignore-list", "List your ignored channels and servers per keyword")]
        public async Task ListIgnored()
        {
            var keywords = await _kwdSvc.GetKeywordsForUserAsync(Context.User.Id);
            var reply = new StringBuilder();

            if (keywords.Count == 0)
            {
                await RespondAsync("You have no keywords.", ephemeral: true);
                return;
            }

            foreach (var k in keywords)
            {
                if (k.IgnoredChannels.Count > 0 || k.IgnoredGuilds.Count > 0)
                {
                    if (reply.Length > 0) reply.Append("\n");
                    reply.Append($"`{k.Keyword}`");

                    if (k.IgnoredChannels.Count > 0)
                    {
                        reply.Append("\nIgnored Channels: ");
                        for (var i = 0; i < k.IgnoredChannels.Count; i++)
                            reply.Append($"{(i > 0 ? ", " : "")}<#{k.IgnoredChannels[i]}>");
                    }

                    if (k.IgnoredGuilds.Count > 0)
                    {
                        reply.Append("\nIgnored Servers: ");
                        for (var i = 0; i < k.IgnoredGuilds.Count; i++)
                        {
                            var guild = Context.Client.GetGuild(k.IgnoredGuilds[i]);
                            reply.Append($"{(i > 0 ? ", " : "")}{guild?.Name ?? k.IgnoredGuilds[i].ToString()}");
                        }
                    }
                }
            }

            await RespondAsync(reply.Length > 0 ? reply.ToString() : "No keywords are ignored in any channels or servers.", ephemeral: true);
        }

        [SlashCommand("unignore-server", "Stop ignoring a keyword in a specific server")]
        public async Task UnignoreGuild(
            [Summary("keyword", "The keyword to unignore")] string keyword,
            [Summary("serverid", "The server ID to unignore the keyword in")] string guildIdStr)
        {
            if (!ulong.TryParse(guildIdStr, out var guildId))
            {
                await RespondAsync("That server ID is invalid.", ephemeral: true);
                return;
            }

            switch (await _kwdSvc.UnignoreGuildsAsync(Context.User.Id, keyword, guildId))
            {
                case UnignoreResult.Success:
                    var guild = Context.Client.GetGuild(guildId);
                    await RespondAsync($"Unignored keyword `{keyword}` in server {guild?.Name ?? guildId.ToString()}.", ephemeral: true);
                    break;
                case UnignoreResult.NotIgnored:
                    await RespondAsync("That keyword was not ignored in that server. No change made.", ephemeral: true);
                    break;
                case UnignoreResult.NotSubscribed:
                    await RespondAsync("You're not subscribed to that keyword. No change made.", ephemeral: true);
                    break;
            }
        }

        [SlashCommand("unignore-channel", "Stop ignoring a keyword in a specific channel")]
        public async Task UnignoreChannel(
            [Summary("keyword", "The keyword to unignore")] string keyword,
            [Summary("channel", "The channel to unignore the keyword in")] IChannel channel)
        {
            switch (await _kwdSvc.UnignoreChannelsAsync(Context.User.Id, keyword, channel.Id))
            {
                case UnignoreResult.Success:
                    var textChannel = channel as SocketTextChannel;
                    await RespondAsync($"Unignored keyword `{keyword}` in channel <#{channel.Id}>{(textChannel != null ? $" (server {textChannel.Guild.Name})" : "")}.", ephemeral: true);
                    break;
                case UnignoreResult.NotIgnored:
                    await RespondAsync("That keyword was not ignored in that channel. No change made.", ephemeral: true);
                    break;
                case UnignoreResult.NotSubscribed:
                    await RespondAsync("You're not subscribed to that keyword. No change made.", ephemeral: true);
                    break;
            }
        }

        [SlashCommand("mute-server", "Mute all keyword notifications from a server")]
        public async Task MuteGuild([Summary("serverid", "The server ID to mute")] string guildIdStr)
        {
            if (!ulong.TryParse(guildIdStr, out var guildId))
            {
                await RespondAsync("That server ID is invalid.", ephemeral: true);
                return;
            }

            var guild = Context.Client.GetGuild(guildId);
            if (guild == null)
            {
                await RespondAsync($"That server ID is invalid. For more help, see {_helpLink}.", ephemeral: true);
                return;
            }

            await _kwdSvc.MuteGuildAsync(Context.User.Id, guildId);
            await RespondAsync($"Muted {guild.Name}.", ephemeral: true);
        }

        [SlashCommand("mute-channel", "Mute all keyword notifications from a channel")]
        public async Task MuteChannel([Summary("channel", "The channel to mute")] IChannel channel)
        {
            await _kwdSvc.MuteChannelAsync(Context.User.Id, channel.Id);
            await RespondAsync($"Muted <#{channel.Id}>.", ephemeral: true);
        }

        [SlashCommand("mute-list", "List your muted channels and servers")]
        public async Task ListMuted()
        {
            var mutedChannels = await _kwdSvc.GetMutedChannelsForUserAsync(Context.User.Id);
            var mutedGuilds = await _kwdSvc.GetMutedGuildsForUserAsync(Context.User.Id);
            var reply = new StringBuilder();

            if (mutedChannels == null && mutedGuilds == null)
            {
                await RespondAsync("No channels or servers are muted.", ephemeral: true);
                return;
            }

            if (mutedChannels != null)
            {
                reply.Append("Muted Channels\n");
                for (var i = 0; i < mutedChannels.ChannelIds.Count; i++)
                    reply.Append($"{(i > 0 ? ", " : "")}<#{mutedChannels.ChannelIds[i]}>");
            }

            if (mutedGuilds != null)
            {
                reply.Append($"{(mutedChannels != null ? "\n" : "")}Muted Servers\n");
                for (var i = 0; i < mutedGuilds.GuildIds.Count; i++)
                {
                    var guild = Context.Client.GetGuild(mutedGuilds.GuildIds[i]);
                    reply.Append($"{(i > 0 ? ", " : "")}{(guild != null ? guild.Name : mutedGuilds.GuildIds[i].ToString())}");
                }
            }

            await RespondAsync(reply.ToString(), ephemeral: true);
        }

        [SlashCommand("unmute-server", "Unmute keyword notifications from a server")]
        public async Task UnmuteGuild([Summary("serverid", "The server ID to unmute")] string guildIdStr)
        {
            if (!ulong.TryParse(guildIdStr, out var guildId))
            {
                await RespondAsync("That server ID is invalid.", ephemeral: true);
                return;
            }

            var guild = Context.Client.GetGuild(guildId);
            if (guild == null)
            {
                await RespondAsync($"That server ID is invalid. For more help, see {_helpLink}.", ephemeral: true);
                return;
            }

            await _kwdSvc.UnmuteGuildAsync(Context.User.Id, guildId);
            await RespondAsync($"Unmuted {guild.Name}.", ephemeral: true);
        }

        [SlashCommand("unmute-channel", "Unmute keyword notifications from a channel")]
        public async Task UnmuteChannel([Summary("channel", "The channel to unmute")] IChannel channel)
        {
            await _kwdSvc.UnmuteChannelAsync(Context.User.Id, channel.Id);
            await RespondAsync($"Unmuted <#{channel.Id}>.", ephemeral: true);
        }
    }
}