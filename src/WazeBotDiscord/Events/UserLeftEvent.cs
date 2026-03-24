using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WazeBotDiscord.ServerLeave;

namespace WazeBotDiscord.Events
{
    public static class UserLeftEvent
    {
        public static async Task Alert(SocketGuild guild, SocketUser user,  ServerLeaveService serverLeaveService)
        {

            LeaveMessageChannel result = await serverLeaveService.GetExistingLeaveChannel(guild.Id);

            if (result != null)
            {
                var syncChannel = guild.GetTextChannel(result.ChannelId);
                string usernameString = user.Username;
                var guildUser = guild.GetUser(user.Id); // returns SocketGuildUser, which has Nickname

                //User might have been purged from the cache, but we can try to get the server specific nickname
                if (guildUser?.Nickname != null)
                    usernameString += $" ({guildUser?.Nickname})";
                await syncChannel.SendMessageAsync($"**{usernameString}** has left the server.");
            }
        }
    }
}
