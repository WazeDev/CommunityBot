using Discord;
using Discord.Interactions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WazeBotDiscord.Profile;

namespace WazeBotDiscord.Modules
{
    public class ProfileModule : InteractionModuleBase<SocketInteractionContext>
    {
        const string editorProfileBase = "https://www.waze.com/user/editor/";
        const string forumProfileBase = "https://www.waze.com/discuss/u/";
        const string discussProfileBase = "https://www.waze.com/discuss/g?username=";
        const string discordProfileBase = "https://discord.com/users/";

        [SlashCommand("profile", "Look up Waze profiles for an editor")]
        public async Task GetProfile([Summary("username", "The Waze editor username")] string editorName)
        {
            await DeferAsync();

            var editorProfile = editorProfileBase + editorName;
            var forumProfile = forumProfileBase + editorName + "/summary"; //await CheckProfile(forumProfileBase + editorName + "/summary", "forum");
            var discussProfile = await CheckProfile(discussProfileBase + editorName, "Discuss");
            var discordProfile = "";
            string avatarURL = "";

            if (Context.Guild != null)
            {
                var users = await Context.Guild.GetUsersAsync().FlattenAsync();
                foreach (var u in users)
                {
                    if (u.Username.ToLower().StartsWith(editorName.ToLower()))
                    {
                        avatarURL = u.GetAvatarUrl();
                        discordProfile = discordProfileBase + u.Id;
                        break;
                    }
                }
            }
            var pr = new ProfileResult
            {
                EditorName = editorName,
                EditorProfile = $"<{editorProfile}>",
                ForumProfile = forumProfile.StartsWith("http") ? $"<{forumProfile}>" : forumProfile,
                DiscussProfile = discussProfile.StartsWith("http") ? $"<{discussProfile}>" : discussProfile,
                DiscordProfile = discordProfile.StartsWith("http") ? $"<{discordProfile}>" : discordProfile,
                AvatarURL = avatarURL
            };

            var embed = await CreateEmbedAsync(pr);
            await FollowupAsync(embed: embed);
        }

        async Task<string> CheckProfile(string url, string profileType)
        {
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                var response = await request.GetResponseAsync() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                    return "No " + profileType + " profile";
                response.Close();
                return url;
            }
            catch
            {
                return "No " + profileType + " profile";
            }
        }

        async Task<Embed> CreateEmbedAsync(ProfileResult item)
        {
            var sr = new StringBuilder();
            sr.AppendLine(item.EditorProfile.Contains("waze.com")
                ? $"[Editor Profile]({item.EditorProfile})"
                : item.EditorProfile);
            sr.AppendLine(item.ForumProfile.Contains("waze.com")
                ? $"[Discuss User Profile]({item.ForumProfile})"
                : item.ForumProfile);
            sr.AppendLine(item.DiscussProfile.Contains("waze.com")
                ? $"[Discuss Group Profile]({item.DiscussProfile})"
                : item.DiscussProfile);
            sr.AppendLine(item.DiscordProfile.Contains("discord.com")
                ? $"[Discord Profile]({item.DiscordProfile})"
                : item.DiscordProfile);

            return new EmbedBuilder()
            {
                Color = new Color(147, 196, 211),
                Title = "Profiles for " + item.EditorName,
                Description = sr.ToString(),
                ThumbnailUrl = item.AvatarURL
            }.Build();
        }
    }
}