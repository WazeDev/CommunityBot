using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WazeBotDiscord.Wikisearch;

namespace WazeBotDiscord.Modules
{
    [Group("search")]
    [Alias("w")]
    public class WikisearchModule : ModuleBase
    {
        readonly WikisearchService _wikiSearchService;

        public WikisearchModule(WikisearchService searchSvc)
        {
            _wikiSearchService = searchSvc;
        }

        [Command()]
        public async Task NoSearchTerm()
        {
            await ReplyAsync("Please provide a search term. Ex: `!search Places`");
        }

        [Command(RunMode = RunMode.Async), Priority(5)]
        public async Task Search([Remainder]string searchphrase)
        {
            List<SearchItem> results;
            results = await _wikiSearchService.SearchWikiAsync(searchphrase.Replace(" ", "+"));

            StringBuilder resultsString = new StringBuilder();
            if (results != null && results.Count > 0)
            {
                resultsString.AppendLine("Top results for `" + searchphrase + "`: ");
                for (var i = 0; i < Math.Min(results.Count, 3); i++)
                {
                    resultsString.AppendLine(results[i].Title + " <" + results[i].URL + ">");
                }
            }
            else
                resultsString.Append("No matches found for `" + searchphrase + "`");

            await ReplyAsync(resultsString.ToString());
        }
    }
}
