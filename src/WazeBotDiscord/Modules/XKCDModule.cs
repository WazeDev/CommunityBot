using Discord;
using Discord.Interactions;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WazeBotDiscord.XKCD;

namespace WazeBotDiscord.Modules
{
    public class XKCDModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly HttpClient _httpClient;

        public XKCDModule(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("WazeBot");
        }

        [SlashCommand("xkcd", "Get a random or specific xkcd comic")]
        public async Task GetXkcd([Summary("number", "The comic number (leave empty for random)")] int? comicNum = null)
        {
            await DeferAsync();
            var result = await GetComicAsync(comicNum);
            var embed = CreateEmbed(result);
            await FollowupAsync(embed: embed);
        }

        async Task<XKCDResult> GetComicAsync(int? comicNum)
        {
            string url;
            if (comicNum.HasValue)
                url = $"https://xkcd.com/{comicNum}/info.0.json";
            else
            {
                // Get the latest comic number first, then pick a random one
                var latestJson = await _httpClient.GetStringAsync("https://xkcd.com/info.0.json");
                var latest = JObject.Parse(latestJson);
                var latestNum = latest["num"].Value<int>();
                var randomNum = new Random().Next(1, latestNum + 1);
                url = $"https://xkcd.com/{randomNum}/info.0.json";
            }

            var json = await _httpClient.GetStringAsync(url);
            var comic = JObject.Parse(json);

            return new XKCDResult
            {
                Title = comic["title"].ToString(),
                ImageURL = comic["img"].ToString(),
                AltText = comic["alt"].ToString()
            };
        }

        Embed CreateEmbed(XKCDResult result)
        {
            return new EmbedBuilder()
            {
                Color = new Color(147, 196, 211),
                Title = "xkcd: " + result.Title,
                Url = "http://www.xkcd.com",
                ImageUrl = result.ImageURL,
                Footer = new EmbedFooterBuilder
                {
                    Text = result.AltText
                }
            }.Build();
        }
    }
}