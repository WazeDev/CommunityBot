using Discord.WebSocket;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using WazeBotDiscord.ChannelSync;
using Discord;
using System.Net;

namespace WazeBotDiscord.ChannelSync
{
    class ChannelSyncHandler
    {
        public static async Task HandleChannelSyncAsync(SocketMessage msg, ChannelSyncService service, DiscordSocketClient client)
        {
            if (msg is not SocketUserMessage)
                return;
            if (msg.Author.Id == client.CurrentUser.Id) //don't sync bot messages
                return;

            var channels = await service.getSyncChannels(msg.Channel.Id);
            if (channels != null) {
                ulong channelToSyncTo = channels.Channel1;

                if (msg.Channel.Id == channels.Channel1)
                    channelToSyncTo = channels.Channel2;

                string nickname = msg.Author.Username;
                if (msg.Author is Discord.WebSocket.SocketGuildUser)
                    if (((Discord.WebSocket.SocketGuildUser)msg.Author).Nickname != null)
                        nickname = ((Discord.WebSocket.SocketGuildUser)msg.Author).Nickname;

                var author = new EmbedAuthorBuilder().WithName(nickname).WithIconUrl(msg.Author.GetAvatarUrl());

                var embed = new EmbedBuilder().WithAuthor(author).WithDescription(msg.Content);

                var myChannel = client.GetChannel(channelToSyncTo) as IMessageChannel;
                await myChannel.SendMessageAsync("", false, embed.Build());
                foreach(Attachment attachment in msg.Attachments)
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(attachment.Url, attachment.Filename);
                    }
                    await myChannel.SendFileAsync(attachment.Filename);
                }
                
            }

        }
    }
}
