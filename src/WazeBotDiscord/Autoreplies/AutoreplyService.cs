using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;

//using System.Linq;
using System.Threading.Tasks;

namespace WazeBotDiscord.Autoreplies
{
    public class AutoreplyService
    {
        List<Autoreply> _autoreplies;
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
                    _autoreplies = await db.Autoreplies.ToListAsync();
                }
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<List<Autoreply>> GetAllAutoreplies(ulong channelId, ulong guildId)
        {
            await EnsureInitializedAsync();
            return BuildList(channelId, guildId);
        }

        public async Task<Autoreply> SearchForAutoreply(string content, SocketTextChannel channel)
        {
            await EnsureInitializedAsync();
            var autoreplyList = BuildList(channel.Id, channel.Guild.Id);

            return autoreplyList.Find(r => content.StartsWith($"!{r.Trigger}"));
        }

        public async Task<Autoreply> GetExactAutoreply(ulong channelId, ulong guildId, string trigger)
        {
            await EnsureInitializedAsync();
            return _autoreplies.Find(r => r.ChannelId == channelId
                                          && r.GuildId == guildId
                                          && string.CompareOrdinal(r.Trigger, trigger) == 0);
        }

        public async  Task<bool> AddOrModifyAutoreply(Autoreply reply)
        {
            await EnsureInitializedAsync();
            var existing = await GetExactAutoreply(reply.ChannelId, reply.GuildId, reply.Trigger);
            if (existing == null)
            {
                _autoreplies.Add(reply);

                using (var db = new WbContext())
                {
                    db.Autoreplies.Add(reply);
                    await db.SaveChangesAsync();
                }

                return true;
            }

            existing.Reply = reply.Reply;
            existing.AddedById = reply.AddedById;
            existing.AddedAt = reply.AddedAt;

            using (var db = new WbContext())
            {
                db.Autoreplies.Update(existing);
                await db.SaveChangesAsync();
            }

            return false;
        }

        public async Task<bool> RemoveAutoreply(ulong channelId, ulong guildId, string trigger)
        {
            await EnsureInitializedAsync();
            var autoreply = await GetExactAutoreply(channelId, guildId, trigger);
            if (autoreply == null)
                return false;

            _autoreplies.Remove(autoreply);

            using (var db = new WbContext())
            {
                db.Autoreplies.Remove(autoreply);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<List<Autoreply>> GetChannelAutoreplies(ulong channelId)
        {
            await EnsureInitializedAsync();
            return _autoreplies.FindAll(a => a.ChannelId == channelId);
        }

        public async Task<List<Autoreply>> GetGuildAutoreplies(ulong guildId)
        {
            await EnsureInitializedAsync();
            return _autoreplies.FindAll(a => a.ChannelId == 1 && a.GuildId == guildId);
        }

        public async Task<List<Autoreply>> GetGlobalAutoreplies()
        {
            await EnsureInitializedAsync();
            return _autoreplies.FindAll(a => a.ChannelId == 1 && a.GuildId == 1);
        }

        public async Task<Autoreply> GetGlobalAutoreply(string msg)
        {
            await EnsureInitializedAsync();
            var globals = _autoreplies.FindAll(a => a.ChannelId == 1 && a.GuildId == 1);
            return globals.Find(r => msg.StartsWith($"!{r.Trigger}"));
        }

        List<Autoreply> BuildList(ulong channelId, ulong guildId)
        {
            var autoreplyList = _autoreplies.FindAll(a => a.ChannelId == channelId);
            autoreplyList.AddRange(_autoreplies.FindAll(a => a.ChannelId == 1 && a.GuildId == guildId));
            autoreplyList.AddRange(_autoreplies.FindAll(a => a.GuildId == 1));

            return autoreplyList;
        }

        public async Task ReloadAutorepliesAsync()
        {
            _autoreplies.Clear();
            _initialized = false;
            await EnsureInitializedAsync();
        }
    }
}
