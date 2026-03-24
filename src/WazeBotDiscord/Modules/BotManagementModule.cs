using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.BotManagement;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    public class BotManagementModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly BotManagementService _botmanagementSvc;

        public BotManagementModule(BotManagementService botmanagementSvc)
        {
            _botmanagementSvc = botmanagementSvc;
        }

        [SlashCommand("restartbot", "Trigger a bot restart")]
        [RequireChampInNationalGuild]
        public async Task RestartBot()
        {
            await DeferAsync(ephemeral: true);
            var success = await _botmanagementSvc.ExecuteBotService("restart");
            await FollowupAsync(success ? "Bot restart triggered." : "Bot restart failed.", ephemeral: true);
        }

        [SlashCommand("updatebot", "Trigger a bot update")]
        [RequireChampInNationalGuild]
        public async Task UpdateBot()
        {
            await DeferAsync(ephemeral: true);
            var success = await _botmanagementSvc.ExecuteBotService("update");
            await FollowupAsync(success ? "Bot update triggered." : "Bot update failed.", ephemeral: true);
        }

        [SlashCommand("stopbot", "Trigger a bot stop")]
        [RequireAppOwner]
        public async Task StopBot()
        {
            await DeferAsync(ephemeral: true);
            var success = await _botmanagementSvc.ExecuteBotService("stop");
            await FollowupAsync(success ? "Bot stop triggered." : "Bot stop failed.", ephemeral: true);
        }
    }
}