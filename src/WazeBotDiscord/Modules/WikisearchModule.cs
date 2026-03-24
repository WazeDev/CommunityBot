using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WazeBotDiscord.Wikisearch;
using static WazeBotDiscord.Wikisearch.WikisearchService;

namespace WazeBotDiscord.Modules
{
    [Group("search", "Waze wiki search commands")]
    public class WikisearchModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly WikisearchService _wikiSearchService;

        public WikisearchModule(WikisearchService searchSvc)
        {
            _wikiSearchService = searchSvc;
        }

        [SlashCommand("wiki", "Search the Waze Discuss forum")]
        public async Task Search(
            [Summary("term", "The term to search for")] string searchPhrase)
        {
            await DeferAsync();
            var results = await _wikiSearchService.SearchWikiAsync(searchPhrase);//, searchType);
            var resultsString = new StringBuilder();

            if (results != null && results.Count > 0)
            {
                resultsString.AppendLine($"Top results for `{searchPhrase}`:");
                for (var i = 0; i < Math.Min(results.Count, 5); i++)
                    resultsString.AppendLine($"{results[i].Title} <{results[i].URL}>");
            }
            else
                resultsString.Append($"No matches found for `{searchPhrase}`.");

            await FollowupAsync(resultsString.ToString());
        }
    }
}