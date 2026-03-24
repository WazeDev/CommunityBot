using Discord.Interactions;
using System;
using System.Threading.Tasks;
using WazeBotDiscord.DND;

namespace WazeBotDiscord.Modules
{
    [Group("dnd", "Do Not Disturb commands")]
    public class DNDModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly DNDService _dndService;

        public DNDModule(DNDService dndSvc)
        {
            _dndService = dndSvc;
        }

        [SlashCommand("status", "Check your current DND status")]
        public async Task GetDNDTime()
        {
            string result = await _dndService.GetDNDTime(Context.User.Id);
            await RespondAsync(result, ephemeral: true);
        }

        [SlashCommand("set", "Enable DND for a number of hours")]
        public async Task SetDNDTime([Summary("hours", "Number of hours to enable DND for")] double hours)
        {
            if (hours <= 0)
            {
                await RespondAsync("DND hours must be greater than zero.", ephemeral: true);
                return;
            }

            if (await _dndService.AddDND(Context.User.Id, DateTime.Now.AddHours(hours)))
                await RespondAsync($"DND enabled for {hours} hours.", ephemeral: true);
            else
                await RespondAsync($"DND time changed to {hours} hours.", ephemeral: true);
        }

        [SlashCommand("disable", "Disable DND")]
        public async Task Remove()
        {
            var removed = await _dndService.RemoveDND(Context.User.Id);

            if (removed)
                await RespondAsync("DND disabled.", ephemeral: true);
            else
                await RespondAsync("DND was not enabled.", ephemeral: true);
        }
    }
}
