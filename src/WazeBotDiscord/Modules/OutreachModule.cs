using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.Outreach;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("outreach", "Outreach spreadsheet commands")]
    public class OutreachModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly OutreachService _outreachSvc;

        public OutreachModule(OutreachService outreachSvc)
        {
            _outreachSvc = outreachSvc;
        }

        [SlashCommand("url", "Get the outreach spreadsheet URL for this channel")]
        public async Task GetUrl()
        {
            await DeferAsync(ephemeral: true);
            await FollowupAsync(await _outreachSvc.GetChannelSheetUrl(Context.Channel.Id));
        }

        [SlashCommand("search", "Search the outreach spreadsheet for this channel")]
        public async Task Search([Summary("term", "Search term (minimum 4 characters)")] string searchString)
        {
            if (searchString.Length < 4)
            {
                await RespondAsync("Your search term must be at least four characters long.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await FollowupAsync(await _outreachSvc.SearchSheetAsync(Context.Channel.Id, searchString));
        }

        [SlashCommand("add", "Add or update the outreach spreadsheet for this channel")]
        [RequireSmOrAbove]
        public async Task Add(
            [Summary("sheetid", "The Google Sheet ID")] string sheetId,
            [Summary("gid", "The sheet GID (optional)")] string gid = null)
        {
            bool result;
            if (gid != null)
                result = await _outreachSvc.AddSheetIDAsync(Context.Guild.Id, Context.Channel.Id, sheetId, gid);
            else
                result = await _outreachSvc.AddSheetIDAsync(Context.Guild.Id, Context.Channel.Id, sheetId);

            await RespondAsync(result ? "Outreach sheet added." : "Outreach sheet modified.", ephemeral: true);
        }

        [SlashCommand("remove", "Remove the outreach spreadsheet for this channel")]
        [RequireSmOrAbove]
        public async Task Remove()
        {
            var removed = await _outreachSvc.RemoveSheetIDAsync(Context.Guild.Id, Context.Channel.Id);
            await RespondAsync(removed ? "Outreach sheet removed." : "No outreach sheet was set for this channel.", ephemeral: true);
        }
    }
}