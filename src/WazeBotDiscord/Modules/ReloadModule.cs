using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.Autoreplies;
using WazeBotDiscord.ChannelSync;
using WazeBotDiscord.Glossary;
using WazeBotDiscord.Keywords;
using WazeBotDiscord.Lookup;
using WazeBotDiscord.Outreach;
using WazeBotDiscord.Scripts;
using WazeBotDiscord.ServerJoin;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [Group("reload", "Reload bot data from the database")]
    [RequireChampInUSAdminInGlobalScripts]
    public class ReloadModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly LookupService _lookupSvc;
        readonly ScriptsService _scriptsSvc;
        readonly ServerJoinService _serverJoinSvc;
        readonly AutoreplyService _autoreplySvc;
        readonly OutreachService _outreachSvc;
        readonly GlossaryService _glossarySvc;
        readonly KeywordService _keywordSvc;
        readonly ChannelSyncService _channelSyncSvc;

        public ReloadModule(LookupService lookupSvc, ScriptsService scriptsSvc, ServerJoinService serverJoinSvc,
            AutoreplyService autoreplySvc, OutreachService outreachSvc, GlossaryService glossarySvc,
            KeywordService keywordSvc, ChannelSyncService channelSyncSvc)
        {
            _lookupSvc = lookupSvc;
            _scriptsSvc = scriptsSvc;
            _serverJoinSvc = serverJoinSvc;
            _autoreplySvc = autoreplySvc;
            _outreachSvc = outreachSvc;
            _glossarySvc = glossarySvc;
            _keywordSvc = keywordSvc;
            _channelSyncSvc = channelSyncSvc;
        }

        [SlashCommand("list", "List available modules to reload")]
        public async Task AvailableModules()
        {
            await RespondAsync("Modules available to reload: lookup, outreach, serverjoin, autoreplies, glossary, keywords, channelsync", ephemeral: true);
        }

        [SlashCommand("lookup", "Reload lookup sheets")]
        public async Task ReloadLookup()
        {
            await DeferAsync(ephemeral: true);
            await _lookupSvc.ReloadSheetsAsync();
            await FollowupAsync("Lookup reloaded.");
        }

        [SlashCommand("outreach", "Reload outreach sheets")]
        public async Task ReloadOutreach()
        {
            await DeferAsync(ephemeral: true);
            await _outreachSvc.ReloadOutreachAsync();
            await FollowupAsync("Outreach reloaded.");
        }

        [SlashCommand("serverjoin", "Reload server join messages")]
        public async Task ReloadServerJoin()
        {
            await DeferAsync(ephemeral: true);
            await _serverJoinSvc.ReloadServerjoinAsync();
            await FollowupAsync("Serverjoin reloaded.");
        }

        [SlashCommand("autoreplies", "Reload autoreplies")]
        public async Task ReloadAutoreplies()
        {
            await DeferAsync(ephemeral: true);
            await _autoreplySvc.ReloadAutorepliesAsync();
            await FollowupAsync("Autoreplies reloaded.");
        }

        [SlashCommand("glossary", "Reload glossary")]
        public async Task ReloadGlossary()
        {
            await DeferAsync(ephemeral: true);
            await _glossarySvc.ReloadGlossaryAsync();
            await FollowupAsync("Glossary reloaded.");
        }

        [SlashCommand("keywords", "Reload keywords")]
        public async Task ReloadKeywords()
        {
            await DeferAsync(ephemeral: true);
            await _keywordSvc.ReloadKeywordsAsync();
            await FollowupAsync("Keywords reloaded.");
        }

        [SlashCommand("channelsync", "Reload channel sync records")]
        public async Task ReloadChannelSync()
        {
            await DeferAsync(ephemeral: true);
            await _channelSyncSvc.ReloadChannelSyncAsync();
            await FollowupAsync("Channel sync records reloaded.");
        }
    }
}