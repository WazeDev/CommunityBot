using Discord.Commands;
using System.Threading.Tasks;
using WazeBotDiscord.Outreach;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("outreach")]
    public class OutreachModule : ModuleBase
    {
        readonly OutreachService _outreachSvc;

        public OutreachModule(OutreachService lookupSvc)
        {
            _outreachSvc = lookupSvc;
        }

        [Command]
        public async Task GetUrl()
        {
            await ReplyAsync(_outreachSvc.GetChannelSheetUrl(Context.Channel.Id));
        }

        [Command(RunMode = RunMode.Async), Priority(5)]
        public async Task Search([Remainder]string searchString)
        {
            if (searchString.Length < 4)
            {
                await ReplyAsync("Your search term must be at least four characters long.");
                return;
            }

            await ReplyAsync(await _outreachSvc.SearchSheetAsync(Context.Channel.Id, searchString));
        }

        [Command("add"), Priority(10)]
        [RequireSmOrAbove]
        public async Task Add([Remainder]string sheetID = null)
        {
            if (sheetID == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention}: You must provide a sheet ID.");
                return;
            }
            var sheetInfo = sheetID.Split(" ");
            bool result;
            if (sheetInfo.Length > 2)
            {
                await ReplyAsync("Incorrect paramaters specified.");
                return;
            }
            else if (sheetInfo.Length == 2)
            {
                result = await _outreachSvc.AddSheetIDAsync(Context.Guild.Id, Context.Channel.Id, sheetInfo[0], sheetInfo[1]);
            }
            else
                result = await _outreachSvc.AddSheetIDAsync(Context.Guild.Id, Context.Channel.Id, sheetID);


            var reply = $"{Context.Message.Author.Mention}: outreach sheet added.";
            if (result == false)
                reply = $"{Context.Message.Author.Mention}: outreach sheet modified.";

            await ReplyAsync(reply);
        }

        [Command("remove"), Priority(9)]
        [RequireSmOrAbove]
        public async Task Remove([Remainder]string sheetID = null)
        {
            var removed = await _outreachSvc.RemoveSheetIDAsync(Context.Guild.Id, Context.Channel.Id);

            if (removed)
                await ReplyAsync("Outreach sheet removed.");
            else
                await ReplyAsync("No outreach sheet was set for this channel.");
        }
    }
}
