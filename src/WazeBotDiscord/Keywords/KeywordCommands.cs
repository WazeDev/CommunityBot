using Discord.Commands;
using Discord.WebSocket;
using System.Text;
using System.Threading.Tasks;

namespace WazeBotDiscord.Keywords
{
    [Group("keyword")]
    [Alias("keywords", "kwd", "kwds", "subscription", "subscriptions", "sub", "subs")]
    public class KeywordModule : ModuleBase
    {
        readonly KeywordService _kwdSvc;
        readonly string _helpLink = "<https://wazeopedia.waze.com/wiki/USA/CommunityBot#Keyword_Subscriptions>";

        public KeywordModule(KeywordService kwdSvc)
        {
            _kwdSvc = kwdSvc;
        }

        [Command]
        [Alias("help")]
        public async Task Help()
        {
            await ReplyAsync($"{Context.Message.Author.Mention}: For help with this command, see the Wazeopedia page: {_helpLink}");
        }

        [Command("test")]
        public async Task Test([Remainder] string testString = null)
        {
            ulong guildID = 1; //fake a guildID so the CheckForKeyword doesn't fail when doing a muted/ignored guild lookup, if Context.Guild is not null, pull the real ID
            if (Context.Guild != null)
                guildID = Context.Guild.Id;

            var matches = _kwdSvc.CheckForKeyword(testString, Context.User.Id, guildID, Context.Message.Channel.Id).Find(m => m.UserId == Context.User.Id);

            StringBuilder resultSB = new StringBuilder();
            
            if (matches != null)
            {
                for (var i = 0; i < matches.MatchedKeywords.Count; i++)
                {
                    if (i > 0)
                        resultSB.Append(", ");
                    resultSB.Append($"`{matches.MatchedKeywords[i]}`");

                }
            }
            if (resultSB.Length > 0)
                await ReplyAsync($"Matched keyword(s) {resultSB}");
            else
                await ReplyAsync("No matches found.");
        }

        [Command("list")]
        public async Task List([Remainder]string unused = null)
        {
            var keywords = _kwdSvc.GetKeywordsForUser(Context.Message.Author.Id);
            var reply = new StringBuilder();

            if (keywords.Count == 0)
            {
                reply.Append(Context.Message.Author.Mention);
                reply.Append(": You have no keywords.");
            }
            else
            {
                reply.Append("__");
                reply.Append(Context.Message.Author.Mention);
                reply.Append("'s Keywords__\n```");

                foreach (var k in keywords)
                {
                    reply.Append("\n");
                    reply.Append(k.Keyword);
                }

                //reply.Remove(reply.Length - 1, 1);
                reply.Append("```");
            }

            await ReplyAsync(reply.ToString());
        }

        [Command("add")]
        [Alias("sub", "subscribe")]
        public async Task Add([Remainder]string keyword = null)
        {
            if (keyword == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                return;
            }

            if (keyword.Length < 2)
            {
                await ReplyAsync($"{Context.Message.Author.Mention}: Your keyword must be at least 2 characters long.");
                return;
            }

            if (keyword.Length > 100)
            {
                await ReplyAsync($"{Context.Message.Author.Mention}: Your keyword cannot be longer than 100 characters.");
                return;
            }

            var result = await _kwdSvc.AddKeywordAsync(Context.Message.Author.Id, keyword);
            if (result.AlreadyExisted)
            {
                await ReplyAsync($"{Context.Message.Author.Mention}: " +
                    $"You were already subscribed to the keyword `{keyword}`. No change has been made.");
                return;
            }

            var reply = $"{Context.Message.Author.Mention}: Added keyword `{keyword}`.";
            if (keyword.Contains(" "))
                reply += "\n\n**Note that your keyword contains spaces.** It will only match if all words are matched exactly " +
                    "as you typed them. If you meant to add these as individual keywords, please remove this entry and " +
                    "run the command separately for each individual keyword.";

            await ReplyAsync(reply);
        }

        [Command("remove")]
        [Alias("unsub", "unsubscribe")]
        public async Task Remove([Remainder]string keyword = null)
        {
            if (keyword == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                return;
            }

            var existed = await _kwdSvc.RemoveKeywordAsync(Context.Message.Author.Id, keyword);

            if (!existed)
                await ReplyAsync($"{Context.Message.Author.Mention}: You were not subscribed to that keyword. No change was made.");
            else
                await ReplyAsync($"{Context.Message.Author.Mention}: Subscription to `{keyword}` removed.");
        }

        [Group("ignore")]
        public class IgnoreKeywordModule : ModuleBase
        {
            readonly KeywordService _kwdSvc;
            readonly string _helpLink = "<https://wazeopedia.waze.com/wiki/USA/CommunityBot#Keyword_Subscriptions>";

            public IgnoreKeywordModule(KeywordService kwdSvc)
            {
                _kwdSvc = kwdSvc;
            }

            [Command("server")]
            [Alias("guild")]
            public async Task IgnoreGuild(ulong guildId, [Remainder]string keyword = null)
            {
                var guild = await Context.Client.GetGuildAsync(guildId);
                if (guild == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: That server ID is invalid. For more help, see {_helpLink}.");
                    return;
                }

                if (keyword == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                    return;
                }

                switch (await _kwdSvc.IgnoreGuildsAsync(Context.Message.Author.Id, keyword, guildId))
                {
                    case IgnoreResult.Success:
                        await ReplyAsync($"{Context.Message.Author.Mention}: Ignored keyword `{keyword}` in server {guild.Name}.");
                        break;

                    case IgnoreResult.AlreadyIgnored:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're already ignoring that keyword in that server. No change made.");
                        break;

                    case IgnoreResult.NotSubscribed:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're not subscribed to that keyword. No change made.");
                        break;
                }
            }

            [Command("channel")]
            public async Task IgnoreChannel(string channelId, [Remainder]string keyword = null)
            {
                //We want channelId as a string so people can directly reference a channel with #, then we strip out the reference data
                //A hell of a lot easier than having to pull the channel ID...
                ulong channelID = channelId.GetChannelIDFromString();

                var rawChannel = await Context.Client.GetChannelAsync(channelID);
                if (rawChannel == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: That channel ID is invalid. For more help, see {_helpLink}.");
                    return;
                }

                if (keyword == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                    return;
                }

                var channel = rawChannel as SocketTextChannel;

                switch (await _kwdSvc.IgnoreChannelsAsync(Context.Message.Author.Id, keyword, channelID))
                {
                    case IgnoreResult.Success:
                        await ReplyAsync($"{Context.Message.Author.Mention}: Ignored keyword `{keyword}` in channel {channel.Mention} (server {channel.Guild.Name}).");
                        break;

                    case IgnoreResult.AlreadyIgnored:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're already ignoring that keyword in that channel. No change made.");
                        break;

                    case IgnoreResult.NotSubscribed:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're not subscribed to that keyword. No change made.");
                        break;
                }
            }

            [Command("list")]
            public async Task ListIgnored([Remainder] string notUsed = null)
            {
                var keywords = _kwdSvc.GetKeywordsForUser(Context.Message.Author.Id);
                var reply = new StringBuilder();

                if (keywords.Count == 0)
                {
                    reply.Append(Context.Message.Author.Mention);
                    reply.Append(": You have no keywords.");
                }
                else
                {
                    foreach (var k in keywords)
                    {
                        if (k.IgnoredChannels.Count > 0 || k.IgnoredGuilds.Count > 0) {
                            if(reply.Length > 0)
                                reply.Append("\n"); //need an extra new line to space out the list
                            reply.Append($"`{k.Keyword}`");
                            if (k.IgnoredChannels.Count > 0)
                            {
                                reply.Append($"\nIgnored Channels: ");
                                for(var i=0; i<k.IgnoredChannels.Count; i++)
                                {
                                    reply.Append($"{(i>0 ? ", " : "")}<#{k.IgnoredChannels[i]}>");
                                }
                            }
                            if(k.IgnoredGuilds.Count > 0)
                            {
                                reply.Append($"\nIgnored Servers: ");
                                for(var i=0; i<k.IgnoredGuilds.Count; i++)
                                {
                                    var guild = await Context.Client.GetGuildAsync(k.IgnoredGuilds[i]);
                                    reply.Append($"{(i > 0 ? ", " : "")}{guild.Name}");
                                }
                            }
                        }
                    }
                }
                if (reply.Length == 0)
                    reply.Append("No keywords are ignored in any channels or servers.");
                await ReplyAsync(reply.ToString());
            }
        }

        [Group("unignore")]
        [Alias("unsub", "unsubscribe")]
        public class UnignoreKeywordModule : ModuleBase
        {
            readonly KeywordService _kwdSvc;
            readonly string _helpLink = "<https://wazeopedia.waze.com/wiki/USA/CommunityBot#Keyword_Subscriptions>";

            public UnignoreKeywordModule(KeywordService kwdSvc)
            {
                _kwdSvc = kwdSvc;
            }

            [Command("server")]
            [Alias("guild")]
            public async Task UnignoreGuild(ulong guildId, [Remainder]string keyword = null)
            {
                if (keyword == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                    return;
                }

                switch (await _kwdSvc.UnignoreGuildsAsync(Context.Message.Author.Id, keyword, guildId))
                {
                    case UnignoreResult.Success:
                        var guild = await Context.Client.GetGuildAsync(guildId);
                        await ReplyAsync($"{Context.Message.Author.Mention}: Unignored keyword `{keyword}` in server {guild.Name}.");
                        break;

                    case UnignoreResult.NotIgnored:
                        await ReplyAsync($"{Context.Message.Author.Mention}: That keyword was not ignored in that server. No change made.");
                        break;

                    case UnignoreResult.NotSubscribed:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're not subscribed to that keyword. No change made.");
                        break;
                }
            }

            [Command("channel")]
            public async Task UnignoreChannel(string channelId, [Remainder]string keyword = null)
            {
                ulong channelID = channelId.GetChannelIDFromString();

                if (keyword == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                    return;
                }

                switch (await _kwdSvc.UnignoreChannelsAsync(Context.Message.Author.Id, keyword, channelID))
                {
                    case UnignoreResult.Success:
                        var channel = (await Context.Client.GetChannelAsync(channelID)) as SocketTextChannel;
                        await ReplyAsync($"{Context.Message.Author.Mention}: Unignored keyword `{keyword}` in channel {channel.Mention} (server {channel.Guild.Name}).");
                        break;

                    case UnignoreResult.NotIgnored:
                        await ReplyAsync($"{Context.Message.Author.Mention}: That keyword was not ignored in that channel. No change made.");
                        break;

                    case UnignoreResult.NotSubscribed:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're not subscribed to that keyword. No change made.");
                        break;
                }
            }
        }

        [Group("mute")]
        [Alias("silence", "quiet")]
        public class MuteKeywordsModule : ModuleBase
        {
            readonly KeywordService _kwdSvc;
            readonly string _helpLink = "<https://wazeopedia.waze.com/wiki/USA/CommunityBot#Keyword_Subscriptions>";

            public MuteKeywordsModule(KeywordService kwdSvc)
            {
                _kwdSvc = kwdSvc;
            }

            [Command("server")]
            [Alias("guild")]
            public async Task MuteGuild(ulong guildId)
            {
                var guild = await Context.Client.GetGuildAsync(guildId);
                if (guild == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: That server ID is invalid. For more help, see {_helpLink}.");
                    return;
                }

                await _kwdSvc.MuteGuildAsync(Context.Message.Author.Id, guildId);
                await ReplyAsync($"{Context.Message.Author.Mention}: Muted {guild.Name}.");
            }

            [Command("channel")]
            public async Task MuteChannel(string channelId)
            {
                ulong channelID = channelId.GetChannelIDFromString();
                var channel = await Context.Client.GetChannelAsync(channelID);
                if (channel == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: That channel ID is invalid. For more help, see {_helpLink}.");
                    return;
                }

                await _kwdSvc.MuteChannelAsync(Context.Message.Author.Id, channelID);
                await ReplyAsync($"{Context.Message.Author.Mention}: Muted {channel.Name}.");
            }

            [Command("list")]
            public async Task ListMuted([Remainder] string notUsed = null)
            {
                var mutedChannels = _kwdSvc.GetMutedChannelsForUser(Context.Message.Author.Id);
                var mutedGuilds = _kwdSvc.GetMutedGuildsForUser(Context.Message.Author.Id);
                var reply = new StringBuilder();

                if (mutedChannels == null && mutedGuilds == null)
                {
                    reply.Append("No channels or servers are muted.");
                }
                else
                {
                    if(mutedChannels != null)
                    {
                        reply.Append("Muted Channels\n");
                        for(var i=0; i<mutedChannels.ChannelIds.Count; i++)
                        {
                            reply.Append($"{(i > 0 ? ", " : "")}<#{mutedChannels.ChannelIds[i]}>");
                        }
                    }

                    if (mutedGuilds != null)
                    {
                        reply.Append($"{(mutedChannels != null ? "\n" : "")}Muted Servers\n");
                        for (var i = 0; i < mutedGuilds.GuildIds.Count; i++)
                        {
                            var guild = await Context.Client.GetGuildAsync(mutedGuilds.GuildIds[i]);
                            reply.Append($"{(i > 0 ? ", " : "")}{(guild != null ? guild.Name : mutedGuilds.GuildIds[i].ToString())}");
                        }
                    }
                }

                await ReplyAsync(reply.ToString());
            }
        }

        [Group("unmute")]
        [Alias("unsilence", "unquiet")]
        public class UnmuteKeywordsModule : ModuleBase
        {
            readonly KeywordService _kwdSvc;
            readonly string _helpLink = "<https://wazeopedia.waze.com/wiki/USA/CommunityBot#Keyword_Subscriptions>";

            public UnmuteKeywordsModule(KeywordService kwdSvc)
            {
                _kwdSvc = kwdSvc;
            }

            [Command("server")]
            [Alias("guild")]
            public async Task UnmuteGuild(ulong guildId)
            {
                var guild = await Context.Client.GetGuildAsync(guildId);
                if (guild == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: That server ID is invalid. For more help, see {_helpLink}.");
                    return;
                }

                await _kwdSvc.UnmuteGuildAsync(Context.Message.Author.Id, guildId);
                await ReplyAsync($"{Context.Message.Author.Mention}: Unmuted {guild.Name}.");
            }

            [Command("channel")]
            public async Task UnmuteChannel(string channelId)
            {
                ulong channelID = channelId.GetChannelIDFromString();
                var channel = await Context.Client.GetChannelAsync(channelID);
                if (channel == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: That channel ID is invalid. For more help, see {_helpLink}.");
                    return;
                }

                await _kwdSvc.UnmuteChannelAsync(Context.Message.Author.Id, channelID);
                await ReplyAsync($"{Context.Message.Author.Mention}: Unmuted {channel.Name}.");
            }
        }

        [Group("specify")]
        public class SpecifyKeywordModule : ModuleBase
        {
            readonly KeywordService _kwdSvc;
            readonly string _helpLink = "<https://wazeopedia.waze.com/wiki/USA/CommunityBot#Keyword_Subscriptions>";

            public SpecifyKeywordModule(KeywordService kwdSvc)
            {
                _kwdSvc = kwdSvc;
            }

            [Command("server")]
            [Alias("guild")]
            public async Task SpecifyGuild(ulong guildId, [Remainder] string keyword = null)
            {
                var guild = await Context.Client.GetGuildAsync(guildId);
                if (guild == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: That server ID is invalid. For more help, see {_helpLink}.");
                    return;
                }

                if (keyword == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                    return;
                }

                switch (await _kwdSvc.SpecifyGuildsAsync(Context.Message.Author.Id, keyword, guildId))
                {
                    case SpecifyResult.Success:
                        await ReplyAsync($"{Context.Message.Author.Mention}: Specified keyword `{keyword}` in server {guild.Name}.");
                        break;

                    case SpecifyResult.AlreadySpecified:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're already specifying that keyword in that server. No change made.");
                        break;

                    case SpecifyResult.NotSubscribed:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're not subscribed to that keyword. No change made.");
                        break;
                }
            }

            [Command("channel")]
            public async Task SpecifyChannel(string channelId, [Remainder] string keyword = null)
            {
                //We want channelId as a string so people can directly reference a channel with #, then we strip out the reference data
                //A hell of a lot easier than having to pull the channel ID...
                ulong channelID = channelId.GetChannelIDFromString();

                var rawChannel = await Context.Client.GetChannelAsync(channelID);
                if (rawChannel == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: That channel ID is invalid. For more help, see {_helpLink}.");
                    return;
                }

                if (keyword == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                    return;
                }

                var channel = rawChannel as SocketTextChannel;

                switch (await _kwdSvc.SpecifyChannelsAsync(Context.Message.Author.Id, keyword, channelID))
                {
                    case SpecifyResult.Success:
                        await ReplyAsync($"{Context.Message.Author.Mention}: Specified keyword `{keyword}` in channel {channel.Mention} (server {channel.Guild.Name}).");
                        break;

                    case SpecifyResult.AlreadySpecified:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're already specifying that keyword in that channel. No change made.");
                        break;

                    case SpecifyResult.NotSubscribed:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're not subscribed to that keyword. No change made.");
                        break;
                }
            }

            [Command("list")]
            public async Task ListSpecified([Remainder] string notUsed = null)
            {
                var keywords = _kwdSvc.GetKeywordsForUser(Context.Message.Author.Id);
                var reply = new StringBuilder();

                if (keywords.Count == 0)
                {
                    reply.Append(Context.Message.Author.Mention);
                    reply.Append(": You have no keywords.");
                }
                else
                {
                    foreach (var k in keywords)
                    {
                        if (k.SpecifiedChannels.Count > 0 || k.SpecifiedGuilds.Count > 0)
                        {
                            if (reply.Length > 0)
                                reply.Append("\n"); //need an extra new line to space out the list
                            reply.Append($"`{k.Keyword}`");
                            if (k.SpecifiedChannels.Count > 0)
                            {
                                reply.Append($"\nSpecified Channels: ");
                                for (var i = 0; i < k.SpecifiedChannels.Count; i++)
                                {
                                    reply.Append($"{(i > 0 ? ", " : "")}<#{k.SpecifiedChannels[i]}>");
                                }
                            }
                            if (k.SpecifiedGuilds.Count > 0)
                            {
                                reply.Append($"\nSpecified Servers: ");
                                for (var i = 0; i < k.SpecifiedGuilds.Count; i++)
                                {
                                    var guild = await Context.Client.GetGuildAsync(k.SpecifiedGuilds[i]);
                                    reply.Append($"{(i > 0 ? ", " : "")}{guild.Name}");
                                }
                            }
                        }
                    }
                }
                if (reply.Length == 0)
                    reply.Append("No keywords are specified in any channels or servers.");
                await ReplyAsync(reply.ToString());
            }
        }

        [Group("unspecify")]
        public class UnspecifyKeywordModule : ModuleBase
        {
            readonly KeywordService _kwdSvc;
            readonly string _helpLink = "<https://wazeopedia.waze.com/wiki/USA/CommunityBot#Keyword_Subscriptions>";

            public UnspecifyKeywordModule(KeywordService kwdSvc)
            {
                _kwdSvc = kwdSvc;
            }

            [Command("server")]
            [Alias("guild")]
            public async Task UnspecifyGuild(ulong guildId, [Remainder] string keyword = null)
            {
                if (keyword == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                    return;
                }

                switch (await _kwdSvc.UnspecifyGuildsAsync(Context.Message.Author.Id, keyword, guildId))
                {
                    case UnspecifyResult.Success:
                        var guild = await Context.Client.GetGuildAsync(guildId);
                        await ReplyAsync($"{Context.Message.Author.Mention}: Unspecified keyword `{keyword}` in server {guild.Name}.");
                        break;

                    case UnspecifyResult.NotSpecified:
                        await ReplyAsync($"{Context.Message.Author.Mention}: That keyword was not specified in that server. No change made.");
                        break;

                    case UnspecifyResult.NotSubscribed:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're not subscribed to that keyword. No change made.");
                        break;
                }
            }

            [Command("channel")]
            public async Task UnspecifyChannel(string channelId, [Remainder] string keyword = null)
            {
                ulong channelID = channelId.GetChannelIDFromString();

                if (keyword == null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a keyword. For more help, see {_helpLink}.");
                    return;
                }

                switch (await _kwdSvc.UnspecifyChannelsAsync(Context.Message.Author.Id, keyword, channelID))
                {
                    case UnspecifyResult.Success:
                        var channel = (await Context.Client.GetChannelAsync(channelID)) as SocketTextChannel;
                        await ReplyAsync($"{Context.Message.Author.Mention}: Unspecified keyword `{keyword}` in channel {channel.Mention} (server {channel.Guild.Name}).");
                        break;

                    case UnspecifyResult.NotSpecified:
                        await ReplyAsync($"{Context.Message.Author.Mention}: That keyword was not specified in that channel. No change made.");
                        break;

                    case UnspecifyResult.NotSubscribed:
                        await ReplyAsync($"{Context.Message.Author.Mention}: You're not subscribed to that keyword. No change made.");
                        break;
                }
            }
        }
    }
}
