using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WazeBotDiscord.Autoreplies
{
    public static class AutoreplyHandler
    {
        public static async Task HandleAutoreplyAsync(SocketMessage inMsg, AutoreplyService arService)//, IReadOnlyCollection<SocketGuild> guilds)
        {
            var msg = (SocketUserMessage)inMsg;
            var content = msg.Content.ToLowerInvariant();
            Autoreply ar;

            if (inMsg.Channel is SocketDMChannel)
                ar = await arService.GetGlobalAutoreply(content);
            else
            {
                var channel = (SocketTextChannel)msg.Channel;
                ar = await arService.SearchForAutoreply(content, channel);
            }

            if (ar == null)
                return;

            await inMsg.Channel.SendMessageAsync(ar.Reply);
            //you can get the channel from the guild with GetTextChannelAsync then SendMessageAsync on it like any other channel
            /*if (msg.Channel.Id == 359327158944137228) {
                SocketGuild syncGuild = null;
                foreach(SocketGuild g  in guilds)
                {
                    if (g.Id == 360595895965843456)
                        syncGuild = g;
                }

                
                var syncChannel = syncGuild.GetTextChannel(361352875680595969);
                await syncChannel.SendMessageAsync($"**{inMsg.Author.Username}**: {inMsg.Content}");
            }*/
        }
    }
}
