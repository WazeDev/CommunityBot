using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.Abbreviation;

namespace WazeBotDiscord.Modules
{
    [Group("abbr", "Waze abbreviation lookup commands")]
    public class AbbreviationModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly AbbreviationService _abbreviationSvc;

        public AbbreviationModule(AbbreviationService lookupSvc)
        {
            _abbreviationSvc = lookupSvc;
        }

        [SlashCommand("url", "Get the link to the Waze abbreviations page")]
        public async Task GetUrl()
        {
            await RespondAsync("<https://wazeopedia.waze.com/wiki/USA/Abbreviations_and_acronyms>");
        }

        [SlashCommand("search", "Search for a Waze abbreviation")]
        public async Task Search([Summary("term", "Search term (minimum 3 characters)")] string searchString)
        {
            if (searchString.Length < 3)
            {
                await RespondAsync("Your search term must be at least three characters long.");
                return;
            }
            await DeferAsync();
            var response = await _abbreviationSvc.SearchSheetAsync(Context.Channel.Id, searchString);
            await FollowupAsync(response.message, embed: response.results);
        }
    }
}
