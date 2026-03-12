using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using WazeBotDiscord.Abbreviation;

namespace WazeBotDiscord.Modules
{
    [Group("abbr")]
    [Alias("abbreviation")]
    public class AbbreviationModule : ModuleBase
    {
        readonly AbbreviationService _abbreviationSvc;

        public AbbreviationModule(AbbreviationService lookupSvc)
        {
            _abbreviationSvc = lookupSvc;
        }

        [Command]
        public async Task GetUrl()
        {
            await ReplyAsync("<https://www.waze.com/discuss/t/abbreviations-and-acronyms/377720>");
        }

        [Command(RunMode = RunMode.Async), Priority(5)]
        public async Task Search([Remainder]string searchString)
        {
            if (searchString.Length < 3)
            {
                await ReplyAsync("Your search term must be at least three characters long.");
                return;
            }
            var response = await _abbreviationSvc.SearchSheetAsync(Context.Channel.Id, searchString);
            await ReplyAsync(response.message, embed: response.results);
        }
    }
}
