using Discord;
using Discord.Interactions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using WazeBotDiscord.Tiles;

namespace WazeBotDiscord.Modules
{
    public class TilesModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly HttpClient _httpClient;
        static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";

        public TilesModule(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("WazeBot");
        }

        [SlashCommand("tiles", "Get the latest Waze map tile update status")]
        public async Task Tiles()
        {
            await DeferAsync();
            var tilesResult = await GetTilesAsync();
            var embed = CreateEmbed(tilesResult);
            await FollowupAsync(embed: embed);
        }

        async Task<TilesResult> GetTilesAsync()
        {
            var (naTitle, naPublished) = await ParseFeedAsync(
                "https://storage.googleapis.com/waze-tile-build-public/release-history/na-feed.xml");
            var (intlTitle, intlPublished) = await ParseFeedAsync(
                "https://storage.googleapis.com/waze-tile-build-public/release-history/intl-feed.xml");
            var (ilTitle, ilPublished) = await ParseFeedAsync(
                "https://storage.googleapis.com/waze-tile-build-public/release-history/il-feed.xml");

            return new TilesResult
            {
                NATileDate = "NA: " + naTitle,
                NAUpdatePerformed = $"*(performed: {naPublished:MMM d, yyyy HH:mm} UTC | <t:{((DateTimeOffset)naPublished):R}>)*",
                INTLTileDate = "INTL: " + intlTitle,
                INTLUpdatePerformed = $"*(performed: {intlPublished:MMM d, yyyy HH:mm} UTC | <t:{((DateTimeOffset)intlPublished).ToUnixTimeSeconds()}:R>)*",
                ILTileDate = "IL: " + ilTitle,
                ILUpdatePerformed = $"*(performed: {ilPublished:MMM d, yyyy HH:mm} UTC | <t:{((DateTimeOffset)ilPublished).ToUnixTimeSeconds()}:R>)*",
            };
        }

        async Task<(string title, DateTime published)> ParseFeedAsync(string url)
        {
            var xml = await _httpClient.GetStringAsync(url);
            var doc = XDocument.Parse(xml);
            var entry = doc.Root.Element(Atom + "entry");
            var title = entry.Element(Atom + "content").Value
                .Replace("North America map tiles were successfully updated to: ", "")
                .Replace("International map tiles were successfully updated to: ", "")
                .Replace("Israel map tiles were successfully updated to: ", "")
                .Trim();
            var published = DateTime.Parse(entry.Element(Atom + "published").Value);
            return (title, published);
        }

        Embed CreateEmbed(TilesResult item)
        {
            return new EmbedBuilder()
            {
                Color = new Color(147, 196, 211),
                Title = "Waze Tile Status",
                Url = "https://www.waze.com/",
                Description = $"{item.NATileDate}{Environment.NewLine}{item.NAUpdatePerformed}" +
                              $"{Environment.NewLine}{Environment.NewLine}" +
                              $"{item.INTLTileDate}{Environment.NewLine}{item.INTLUpdatePerformed}" +
                              $"{Environment.NewLine}{Environment.NewLine}" +
                              $"{item.ILTileDate}{Environment.NewLine}{item.ILUpdatePerformed}"
            }.Build();
        }
    }
}