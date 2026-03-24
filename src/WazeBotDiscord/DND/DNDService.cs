using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WazeBotDiscord.DND
{
    public class DNDService
    {
        //readonly HttpClient _client;
        List<DNDListItem> _dnds;
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
                    _dnds = await db.DndList.ToListAsync();
                }
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        //public DNDService(IHttpClientFactory clientFactory)
        //{
        //    _client = clientFactory.CreateClient("WazeBot");
        //}

        public async Task<string> GetDNDTime(ulong userID)
        {
            await EnsureInitializedAsync();
            var dnd =  _dnds.Find(s => s.UserId == userID);
            if (dnd == null || dnd.EndTime < DateTime.Now)
            {
                if (dnd != null)
                    await RemoveDND(userID);
                return "DND is not currently enabled";
            }

            TimeSpan timeLeft = dnd.EndTime - DateTime.Now;
            string returnVal = "";
            if (timeLeft.Days > 0)
                returnVal += $"{timeLeft.Days} days ";
            if (timeLeft.Hours > 0)
                returnVal += $"{timeLeft.Hours} hours ";
            if (timeLeft.Minutes > 0 || (timeLeft.Minutes == 0 && timeLeft.Seconds > 0))
                returnVal += $"{timeLeft.Minutes}.{Math.Round((double)timeLeft.Seconds / 60, 1).ToString().Substring(2)} minutes ";
            return $"{returnVal}left";
        }

        public async Task<bool> AddDND(ulong userID, DateTime endTime)
        {
            await EnsureInitializedAsync();
            var existing = GetExistingDND(userID);
            if (existing == null)
            {
                var dbSheet = new DNDListItem
                {
                    UserId = userID,
                    EndTime = endTime
                };

                using (var db = new WbContext())
                {
                    db.DndList.Add(dbSheet);
                    await db.SaveChangesAsync();
                }

                _dnds.Add(dbSheet);
                return true;
            }

            existing.UserId = userID;
            existing.EndTime = endTime;

            using (var db = new WbContext())
            {
                db.DndList.Update(existing);
                await db.SaveChangesAsync();
            }

            return false;
        }

        public async Task<bool> RemoveDND(ulong userID)
        {
            await EnsureInitializedAsync();
            var existing = GetExistingDND(userID);
            if (existing == null)
                return false;

            _dnds.Remove(existing);

            using (var db = new WbContext())
            {
                db.DndList.Remove(existing);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public DNDListItem GetExistingDND(ulong userID)
        {
            return _dnds.Find(r => r.UserId == userID);
        }
    }
}
