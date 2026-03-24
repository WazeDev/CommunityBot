using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Discord.Interactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WazeBotDiscord.Wikisearch
{
    public enum DiscussSearchType
    {
        [ChoiceDisplay("Topics")]
        Topics,
        [ChoiceDisplay("Users")]
        Users,
        [ChoiceDisplay("Categories")]
        Categories
    }

    public class WikisearchService
    {
        readonly HttpClient _client;
        public WikisearchService(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("WazeBot");
        }

        public async Task<List<SearchItem>> SearchWikiAsync(string searchPhrase)//, DiscussSearchType searchType = DiscussSearchType.Topics)
        {
            string url;
            //switch (searchType)
            //{
            //    case DiscussSearchType.Users:
            //        url = $"https://www.waze.com/discuss/search.json?q={Uri.EscapeDataString(searchPhrase)}&search_type=users";
            //        break;
            //    case DiscussSearchType.Categories:
            //        url = $"https://www.waze.com/discuss/search.json?q={Uri.EscapeDataString(searchPhrase)}&search_type=categories_tags";
            //        break;
            //    default:
                    url = $"https://www.waze.com/discuss/search.json?q={Uri.EscapeDataString(searchPhrase)}";
            //        break;
            //}

            var json = await _client.GetStringAsync(url);
            var result = JObject.Parse(json);

            var items = new List<SearchItem>();

            //if (searchType == DiscussSearchType.Users)
            //{
            //    var users = result["users"] as JArray;
            //    if (users != null)
            //        foreach (var u in users)
            //            items.Add(new SearchItem
            //            {
            //                Title = u["username"]?.ToString(),
            //                URL = $"https://www.waze.com/discuss/u/{u["username"]}"
            //            });
            //}
            //else if (searchType == DiscussSearchType.Categories)
            //{
            //    var categories = result["categories"] as JArray;
            //    if (categories != null)
            //        foreach (var c in categories)
            //            items.Add(new SearchItem
            //            {
            //                Title = c["name"]?.ToString(),
            //                URL = $"https://www.waze.com/discuss/c/{c["slug"]}"
            //            });
            //}
            //else
            //{
                var topics = result["topics"] as JArray;
                if (topics != null)
                    foreach (var topic in topics)
                    {
                        var title = topic["title"]?.ToString();
                        var slug = topic["slug"]?.ToString();
                        var id = topic["id"]?.ToString();
                        if (title != null && slug != null && id != null)
                            items.Add(new SearchItem
                            {
                                Title = title,
                                URL = $"https://www.waze.com/discuss/t/{slug}/{id}"
                            });
                    }
            //}

            return items;
        }
    }
}
