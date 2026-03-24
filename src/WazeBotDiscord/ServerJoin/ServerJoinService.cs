using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WazeBotDiscord.ServerJoin
{
    public class ServerJoinService
    {
        List<ServerJoinRecord> _serverJoinMessages;
        readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
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
                    _serverJoinMessages = await db.ServerJoinRecords.ToListAsync();
                }
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Adds a message to the database for the server the command is run on, to be sent when a user joins a server
        /// </summary>
        /// <param name="guildID"></param>
        /// <param name="message"></param>
        /// <returns>Returns true if it is a new add, false if there was an entry and we are modifying</returns>
        public async Task<bool> AddServerMessage(ulong guildID, string message)
        {
            await EnsureInitializedAsync();
            var existing = await GetExistingJoinMessage(guildID);
            if (existing == null)
            {
                var dbSheet = new ServerJoinRecord
                {
                    GuildId = guildID,
                    JoinMessage = message
                };

                using (var db = new WbContext())
                {
                    db.ServerJoinRecords.Add(dbSheet);
                    await db.SaveChangesAsync();
                }

                _serverJoinMessages.Add(dbSheet);
                return true;
            }

            existing.GuildId = guildID;
            existing.JoinMessage = message;

            using (var db = new WbContext())
            {
                db.ServerJoinRecords.Update(existing);
                await db.SaveChangesAsync();
            }

            return false;
        }

        public async Task<bool> RemoveServerMessage(ulong guildID)
        {
            await EnsureInitializedAsync();
            var existing = await GetExistingJoinMessage(guildID);
            if (existing == null)
                return false;

            _serverJoinMessages.Remove(existing);

            using (var db = new WbContext())
            {
                db.ServerJoinRecords.Remove(existing);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<ServerJoinRecord> GetExistingJoinMessage(ulong guildId)
        {
            await EnsureInitializedAsync();
            return _serverJoinMessages.Find(r => r.GuildId == guildId);
        }

        public async Task ReloadServerjoinAsync()
        {
            await _initLock.WaitAsync();
            try
            {
                _initialized = false;
                _serverJoinMessages.Clear();
            }
            finally
            {
                _initLock.Release();
            }
            await EnsureInitializedAsync();
        }
    }
}
