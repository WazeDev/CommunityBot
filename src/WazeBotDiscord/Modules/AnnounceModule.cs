using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using WazeBotDiscord.Announce;
using WazeBotDiscord.Utilities;

namespace WazeBotDiscord.Modules
{
    [RequireChampInNationalAdminInGlobal]
    public class AnnounceModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly AnnounceService _announceSvc;

        public AnnounceModule(AnnounceService announceSvc)
        {
            _announceSvc = announceSvc;
        }

        [ModalInteraction("announce_modal")]
        [RequireChampInNationalAdminInGlobal]
        public async Task HandleAnnounceModal(AnnounceModal modal)
        {
            await DeferAsync(ephemeral: true);
            Console.WriteLine("HandleAnnounceModal fired");

            var channels = await _announceSvc.GetAnnounceChannels();
            Console.WriteLine($"Got {channels.Count} channels");

            var guilds = _announceSvc.GetBotGuilds();
            Console.WriteLine($"Got {guilds.Count} guilds");

            foreach (var c in channels)
            {
                foreach (SocketGuild g in guilds)
                {
                    var announceChannel = g.GetTextChannel(c.Channel);
                    if (announceChannel != null)
                    {

                        Console.WriteLine($"Sending to {g.Name} #{announceChannel.Name}");
                        await announceChannel.SendMessageAsync(modal.Message);
                    }
                }
            }

            await FollowupAsync("Announcement sent.", ephemeral: true);
            Console.WriteLine("HandleAnnounceModal complete");
        }

        [SlashCommand("announce", "Send an announcement to all configured channels")]
        public async Task SendMessage()
        {
            await RespondWithModalAsync<AnnounceModal>("announce_modal");
        }

        //[SlashCommand("announce", "Send an announcement to all configured channels")]
        //public async Task SendMessage([Summary("message", "The message to announce")] string message)
        //{
        //    await DeferAsync(ephemeral: true);

        //    var channels = await _announceSvc.GetAnnounceChannels();
        //    var guilds = _announceSvc.GetBotGuilds();

        //    foreach (var c in channels)
        //    {
        //        foreach (SocketGuild g in guilds)
        //        {
        //            var announceChannel = g.GetTextChannel(c.Channel);
        //            if (announceChannel != null)
        //                await announceChannel.SendMessageAsync(message);
        //        }
        //    }

        //    await FollowupAsync("Announcement sent.", ephemeral: true);
        //}
    }
}