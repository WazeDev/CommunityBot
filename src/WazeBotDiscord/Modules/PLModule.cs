using Discord.Interactions;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WazeBotDiscord.Modules
{
    public class PLModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("pl", "Clean up a Waze editor permalink or convert a share link")]
        public async Task Permalink([Summary("url", "The Waze URL to clean up or convert")] string message)
        {
            var sb = new StringBuilder();

            // Handle ul.waze.com share links
            var shareRegex = new Regex(@"https?://ul\.waze\.com/ul\?[^\s]+");
            foreach (Match shareMatch in shareRegex.Matches(message))
            {
                var uri = new Uri(shareMatch.Value);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var ll = query["ll"];
                if (ll != null)
                {
                    var coords = ll.Split(',');
                    if (coords.Length == 2)
                    {
                        var lat = coords[0].Trim();
                        var lon = coords[1].Trim();
                        if (sb.Length > 0) sb.Append(" | ");
                        sb.Append($"https://www.waze.com/en-US/editor/?env=usa&lat={lat}&lon={lon}&zoom=5");
                    }
                }
            }

            // Handle regular editor permalinks
            var regURL = new Regex(@"(?:http(?:s):\/\/)?(?:www\.|beta\.)?waze\.com\/(?:.*?\/)?editor[-a-zA-Z0-9@:%_\+,.~#?&//=]*");
            foreach (Match itemMatch in regURL.Matches(message))
            {
                if (sb.Length > 0) sb.Append(" | ");
                var rgx = new Regex(@"&[^&]*Filter=[^&]*|&s=(\d+)");
                sb.Append(rgx.Replace(itemMatch.ToString(), ""));
            }

            // Handle live-map links
            var liveMapRegex = new Regex(@"https?://(?:www\.)?waze\.com/[a-z-]+/live-map/[^\s]+");
            foreach (Match liveMatch in liveMapRegex.Matches(message))
            {
                var uri = new Uri(liveMatch.Value);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var latlng = query["latlng"];
                if (latlng != null)
                {
                    var coords = latlng.Split(',');
                    if (coords.Length == 2)
                    {
                        var lat = coords[0].Trim();
                        var lon = coords[1].Trim();
                        if (sb.Length > 0) sb.Append(" | ");
                        sb.Append($"https://www.waze.com/en-US/editor/?env=usa&lat={lat}&lon={lon}&zoom=5");
                    }
                }
            }

            await RespondAsync(sb.Length > 0 ? sb.ToString() : "No Waze URLs found.");
        }
    }
}