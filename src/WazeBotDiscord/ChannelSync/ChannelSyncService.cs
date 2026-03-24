using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WazeBotDiscord.ChannelSync
{
    public class ChannelSyncService
    {
        List<SyncChannelsRow> _syncdChannels;
        readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        bool _initialized = false;

        public ChannelSyncService()
        {
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return; // double check after acquiring lock
                using (var db = new WbContext())
                {
                    _syncdChannels = await db.SyncChannels.ToListAsync();
                }
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<SyncChannelsRow> getSyncChannels(ulong channel)
        {
            await EnsureInitializedAsync();
            return _syncdChannels.Find(c => c.Channel1 == channel || c.Channel2 == channel);
        }

        public async Task<bool> AddChannelSync(ulong channelID1, ulong channelID2,ulong AddedBy, DateTime AddedAt, string AddedByName)
        {
            await EnsureInitializedAsync();
            var dbSheet = new SyncChannelsRow
            {
                Channel1 = channelID1,
                Channel2 = channelID2,
                AddedAt = AddedAt,
                AddedById = AddedBy,
                AddedByName = AddedByName
            };

            using (var db = new WbContext())
            {
                db.SyncChannels.Add(dbSheet);
                await db.SaveChangesAsync();

            }

            _syncdChannels.Add(dbSheet);
            return true;
        }

        public async Task<bool> RemoveChannelSync(ulong channelID)
        {
            await EnsureInitializedAsync();
            var syncdChannels = await getSyncChannels(channelID);
            if (syncdChannels == null)
                return false;

            _syncdChannels.Remove(syncdChannels);
            using (var db = new WbContext())
            {
                db.SyncChannels.Remove(syncdChannels);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public async Task ReloadChannelSyncAsync()
        {
            _syncdChannels.Clear();
            _initialized = false;
            await EnsureInitializedAsync();
        }
    }
}
