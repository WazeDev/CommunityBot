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
        const string wikiProfileBase = "https://wazeopedia.waze.com/wiki/USA/User:";
        const string discussProfileBase = "https://www.waze.com/discuss/g?username=";

        [SlashCommand("profile", "Look up Waze profiles for an editor")]
        public async Task GetProfile([Summary("username", "The Waze editor username")] string editorName)
        {
            await DeferAsync();

            var editorProfile = editorProfileBase + editorName;
            var forumProfile = forumProfileBase + editorName + "/summary"; //await CheckProfile(forumProfileBase + editorName + "/summary", "forum");
            var wikiProfile = await CheckProfile(wikiProfileBase + editorName, "wiki");
            var discussProfile = await CheckProfile(discussProfileBase + editorName, "Discuss");

            var pr = new ProfileResult
            {
                EditorName = editorName,
                EditorProfile = $"<{editorProfile}>",
                ForumProfile = forumProfile.StartsWith("http") ? $"<{forumProfile}>" : forumProfile,
                WikiProfile = wikiProfile.StartsWith("http") ? $"<{wikiProfile}>" : wikiProfile,
                DiscussProfile = discussProfile.StartsWith("http") ? $"<{discussProfile}>" : discussProfile
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
            string avatarURL = "";

            if (Context.Guild != null)
            {
                var users = await Context.Guild.GetUsersAsync().FlattenAsync();
                foreach (var u in users)
                {
                    if (u.Username.ToLower().StartsWith(item.EditorName.ToLower()))
                    {
                        avatarURL = u.GetAvatarUrl();
                        break;
                    }
                }
            }

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
            sr.AppendLine(item.WikiProfile.Contains("waze.com")
                ? $"[Wiki Profile]({item.WikiProfile})"
                : item.WikiProfile);

            return new EmbedBuilder()
            {
                Color = new Color(147, 196, 211),
                Title = "Profiles for " + item.EditorName,
                Description = sr.ToString(),
                ThumbnailUrl = avatarURL
            }.Build();
        }
    }
}