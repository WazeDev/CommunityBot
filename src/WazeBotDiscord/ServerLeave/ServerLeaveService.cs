using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WazeBotDiscord.ServerLeave
{
    public class ServerLeaveService
    {
        List<LeaveMessageChannel> _leaveChannels; readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        bool _initialized = false;

        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return; // double check after acquiring lock
                using (var db = new WbContext())
                {
                    _leaveChannels = await db.LeaveMessageChannels.ToListAsync();
                }
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Adds a channel to the database for the user leave event on the server to notify of the user leaving
        /// </summary>
        /// <param name="guildID"></param>
        /// <param name="channelID"></param>
        /// <returns>Returns true if it is a new add, false if there was an entry and we are modifying</returns>
        public async Task<bool> AddChannelIDAsync(ulong guildID, ulong channelID)
        {
            await EnsureInitializedAsync();
            var existing = await GetExistingLeaveChannel(guildID);
            if (existing == null)
            {
                var dbSheet = new LeaveMessageChannel
                {
                    GuildId = guildID,
                    ChannelId = channelID
                };

                using (var db = new WbContext())
                {
                    db.LeaveMessageChannels.Add(dbSheet);
                    await db.SaveChangesAsync();
                }

                _leaveChannels.Add(dbSheet);
                return true;
            }

            existing.GuildId = guildID;
            existing.ChannelId = channelID;

            using (var db = new WbContext())
            {
                db.LeaveMessageChannels.Update(existing);
                await db.SaveChangesAsync();
            }

            return false;
        }

        /// <summary>
        /// Removes the set channel for the server
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns>True if the channel was removed, false if there was no channel set</returns>
        public async Task<bool> RemoveServerChannelAsync(ulong guildID)
        {
            await EnsureInitializedAsync();
            var existing = await GetExistingLeaveChannel(guildID);
            if (existing == null)
                return false;

            _leaveChannels.Remove(existing);

            using (var db = new WbContext())
            {
                db.LeaveMessageChannels.Remove(existing);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<LeaveMessageChannel> GetExistingLeaveChannel(ulong guildId)
        {
            await EnsureInitializedAsync();
            return _leaveChannels.Find(r => r.GuildId == guildId);
        }
    }
}
