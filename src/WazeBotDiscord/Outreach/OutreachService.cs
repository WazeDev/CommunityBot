using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WazeBotDiscord.Outreach
{
    public class OutreachService
    {
        readonly SheetsService _sheetsService;
        List<OutreachSheetToSearch> _sheets;
        readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        bool _initialized = false;

        public OutreachService()
        {
            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
                ApplicationName = "WazeBotDiscord"
            });
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return;
                using (var db = new WbContext())
                {
                    _sheets = await EntityFrameworkQueryableExtensions.ToListAsync(db.OutreachSheetsToSearch);
                }
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task WarmupAsync()
        {
            await EnsureInitializedAsync();
        }

        public async Task<string> GetChannelSheetUrl(ulong channelId)
        {
            await EnsureInitializedAsync();
            var sheet = _sheets.Find(s => s.ChannelId == channelId);
            if (sheet == null)
                return "This channel is not configured to search a spreadsheet.";

            var sheetId = ExtractSheetId(sheet.SheetId);
            return $"<https://docs.google.com/spreadsheets/d/{sheetId}/edit>";
        }

        public async Task<string> SearchSheetAsync(ulong channelId, string origSearchString)
        {
            await EnsureInitializedAsync();
            var searchString = origSearchString.ToLowerInvariant();

            var sheet = _sheets.Find(s => s.ChannelId == channelId);
            if (sheet == null)
                return "This channel is not configured to search a spreadsheet.";

            try
            {
                var sheetId = ExtractSheetId(sheet.SheetId);
                string range;

                if (!string.IsNullOrEmpty(sheet.GId))
                {
                    var tabName = await GetSheetNameFromGidAsync(sheetId, sheet.GId);
                    if (tabName == null)
                        return "Spreadsheet tab not found. The GID may be incorrect.";
                    range = $"'{tabName}'!A:Z";
                }
                else
                    range = "A:Z";

                var request = _sheetsService.Spreadsheets.Values.Get(sheetId, range);
                var response = await request.ExecuteAsync();
                var rows = response.Values;

                if (rows == null || rows.Count == 0)
                    return "Spreadsheet is not configured correctly.";

                var headers = rows[0].Select(h => h.ToString()).ToList();
                var matches = rows.Skip(1)
                    .Where(row => row.Any(cell => cell.ToString().ToLowerInvariant().Contains(searchString)))
                    .ToList();

                return GenerateResult(headers, matches, origSearchString);
            }
            catch (Exception ex)
            {
                return $"Error accessing spreadsheet. Make sure the sheet is shared with 'Anyone with the link can view'. ({ex.Message})";
            }
        }

        async Task<string> GetSheetNameFromGidAsync(string sheetId, string gid)
        {
            var request = _sheetsService.Spreadsheets.Get(sheetId);
            var response = await request.ExecuteAsync();

            var sheet = response.Sheets
                .FirstOrDefault(s => s.Properties.SheetId == int.Parse(gid));

            return sheet?.Properties.Title;
        }

        string ExtractSheetId(string sheetIdOrUrl)
        {
            if (!sheetIdOrUrl.Contains("/"))
                return sheetIdOrUrl;

            var match = Regex.Match(sheetIdOrUrl, @"/spreadsheets/d/([a-zA-Z0-9-_]+)");
            return match.Success ? match.Groups[1].Value : sheetIdOrUrl;
        }

        string GenerateResult(List<string> headers, List<IList<object>> matches, string searchString)
        {
            var result = new StringBuilder();

            var matchCount = matches.Count;
            if (matchCount == 0)
                return $"No results found for `{searchString}`.";
            else if (matchCount > 4)
            {
                matchCount = 4;
                result.Append($"Total of {matches.Count} results for `{searchString}`; only the top four are shown.\n");
            }
            else
                result.Append($"{matchCount} results found for `{searchString}`.\n");

            for (var i = 0; i < matchCount; i++)
            {
                for (var j = 0; j < matches[i].Count; j++)
                {
                    if (string.IsNullOrWhiteSpace(matches[i][j].ToString()))
                        continue;

                    result.Append(matches[i][j]);
                    result.Append(" | ");
                }
                result.Remove(result.Length - 3, 3);
                if (i != matchCount - 1)
                    result.AppendLine(Environment.NewLine);
            }

            var resultString = result.ToString();
            var regURL = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)");
            foreach (Match itemMatch in regURL.Matches(resultString))
                resultString = resultString.Replace(itemMatch.ToString(), "<" + itemMatch.ToString() + ">");

            return resultString;
        }

        public async Task<bool> AddSheetIDAsync(ulong guildID, ulong channelID, string sheetID, string gid = "")
        {
            await EnsureInitializedAsync();
            var existing = await GetExistingOutreachSheet(channelID, guildID);
            if (existing == null)
            {
                var dbSheet = new OutreachSheetToSearch
                {
                    GuildId = guildID,
                    ChannelId = channelID,
                    SheetId = ExtractSheetId(sheetID),
                    GId = gid
                };

                using (var db = new WbContext())
                {
                    db.OutreachSheetsToSearch.Add(dbSheet);
                    await db.SaveChangesAsync();
                }

                _sheets.Add(dbSheet);
                return true;
            }

            existing.GuildId = guildID;
            existing.ChannelId = channelID;
            existing.SheetId = ExtractSheetId(sheetID);
            existing.GId = gid;

            using (var db = new WbContext())
            {
                db.OutreachSheetsToSearch.Update(existing);
                await db.SaveChangesAsync();
            }

            return false;
        }

        public async Task<bool> RemoveSheetIDAsync(ulong guildID, ulong channelID)
        {
            await EnsureInitializedAsync();
            var existing = await GetExistingOutreachSheet(channelID, guildID);
            if (existing == null)
                return false;

            _sheets.Remove(existing);

            using (var db = new WbContext())
            {
                db.OutreachSheetsToSearch.Remove(existing);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<OutreachSheetToSearch> GetExistingOutreachSheet(ulong channelId, ulong guildId)
        {
            await EnsureInitializedAsync();
            return _sheets.Find(r => r.ChannelId == channelId && r.GuildId == guildId);
        }

        public async Task ReloadOutreachAsync()
        {
            await _initLock.WaitAsync();
            try
            {
                _initialized = false;
                _sheets.Clear();
            }
            finally
            {
                _initLock.Release();
            }
            await EnsureInitializedAsync();
        }
    }
}