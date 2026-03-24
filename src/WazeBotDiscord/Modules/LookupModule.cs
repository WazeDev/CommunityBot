using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.Lookup;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("lookup", "Spreadsheet lookup commands")]
    public class LookupModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly LookupService _lookupSvc;

        public LookupModule(LookupService lookupSvc)
        {
            _lookupSvc = lookupSvc;
        }

        [SlashCommand("url", "Get the spreadsheet URL for this channel")]
        public async Task GetUrl()
        {
            await DeferAsync(ephemeral: true);
            await FollowupAsync(await _lookupSvc.GetChannelSheetUrl(Context.Channel.Id));
        }

        [SlashCommand("search", "Search the spreadsheet for this channel")]
        public async Task Search([Summary("term", "Search term (minimum 4 characters)")] string searchString)
        {
            if (searchString.Length < 4)
            {
                await RespondAsync("Your search term must be at least four characters long.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await FollowupAsync(await _lookupSvc.SearchSheetAsync(Context.Channel.Id, searchString));
        }

        [SlashCommand("add", "Add or update the spreadsheet for this channel")]
        [RequireSmOrAbove]
        public async Task Add(
            [Summary("sheetid", "The Google Sheet ID")] string sheetId,
            [Summary("gid", "The sheet GID (optional)")] string gid = null)
        {
            bool result;
            if (gid != null)
                result = await _lookupSvc.AddSheetIDAsync(Context.Guild.Id, Context.Channel.Id, sheetId, gid);
            else
                result = await _lookupSvc.AddSheetIDAsync(Context.Guild.Id, Context.Channel.Id, sheetId);

            await RespondAsync(result ? "Sheet added." : "Sheet modified.", ephemeral: true);
        }

        [SlashCommand("remove", "Remove the spreadsheet for this channel")]
        [RequireSmOrAbove]
        public async Task Remove()
        {
            var removed = await _lookupSvc.RemoveSheetIDAsync(Context.Guild.Id, Context.Channel.Id);
            await RespondAsync(removed ? "Sheet removed." : "No sheet was set for this channel.", ephemeral: true);
        }
    }
}