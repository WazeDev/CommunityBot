using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.Scripts;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("scripts", "Waze scripts commands")]
    public class ScriptsModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly ScriptsService _scriptsService;

        public ScriptsModule(ScriptsService scriptsService)
        {
            _scriptsService = scriptsService;
        }

        [SlashCommand("url", "Get the scripts spreadsheet URL")]
        public async Task GetUrl()
        {
            await RespondAsync(_scriptsService.GetChannelSheetUrl(Context.Channel.Id), ephemeral: true);
        }

        [SlashCommand("search", "Search the scripts spreadsheet")]
        public async Task Search([Summary("term", "Search term (minimum 3 characters)")] string searchString)
        {
            if (searchString.Length < 3)
            {
                await RespondAsync("Your search term must be at least three characters long.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await FollowupAsync(await _scriptsService.SearchSheetAsync(searchString, Context.Guild?.Id ?? 0));
        }
    }
}