using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WazeBotDiscord.Announce
{
    public class AnnounceService
    {
        List<AnnounceChannel> _AnnounceChannels = new List<AnnounceChannel>();
        DiscordSocketClient _client;
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
                    _AnnounceChannels = await EntityFrameworkQueryableExtensions.ToListAsync(db.AnnounceList);
                }
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<List<AnnounceChannel>> GetAnnounceChannels()
        {
            await EnsureInitializedAsync();
            return _AnnounceChannels;
        }

        public IReadOnlyCollection<SocketGuild> GetBotGuilds()
        {
            return _client.Guilds;
        }
    }
}
