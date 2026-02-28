using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WazeBotDiscord.Lookup
{
    public class LookupService
    {
        readonly HttpClient _client;
        List<SheetToSearch> _sheets;

        public LookupService(HttpClient client)
        {
            _client = client;
        }

        public async Task InitAsync()
        {
            using (var db = new WbContext())
            {
                //_sheets = await db.SheetsToSearch.ToListAsync();
                _sheets = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(db.SheetsToSearch);
            }
        }

        public string GetChannelSheetUrl(ulong channelId)
        {
            var sheet = _sheets.Find(s => s.ChannelId == channelId);
            if (sheet == null)
                return "This channel is not configured to search a spreadsheet.";

            return $"<https://docs.google.com/spreadsheets/d/{sheet.SheetId}/edit>";
        }

        public async Task<string> SearchSheetAsync(ulong channelId, string origSearchString)
        {
            var searchString = origSearchString.ToLowerInvariant();

            var sheet = _sheets.Find(s => s.ChannelId == channelId);
            if (sheet == null)
                return "This channel is not configured to search a spreadsheet.";

            var parser = new HtmlParser();
            string sheetURL;
            if (sheet.GId != "") //gid has been specified
                sheetURL = $"https://docs.google.com/spreadsheets/d/{sheet.SheetId}/pubhtml/sheet?gid={sheet.GId}&single=true";
            else
                sheetURL = $"https://docs.google.com/spreadsheets/d/{sheet.SheetId}/pubhtml/sheet?gid=0";
            var resp = await _client.GetAsync(sheetURL);

            if (!resp.IsSuccessStatusCode)
                return "Spreadsheet is not configured correctly.";

            var doc = await parser.ParseDocumentAsync(await resp.Content.ReadAsStringAsync());

            var tblHeader = doc.QuerySelectorAll("table.waffle > tbody > tr:first-of-type");
            var headerRowRaw = tblHeader.FirstOrDefault();
            if (headerRowRaw == null)
                return "Spreadsheet is not configured correctly.";

            var headerRow = (IHtmlTableRowElement)headerRowRaw;
            var headerFields = headerRow.Cells
                .Where(c => !string.IsNullOrWhiteSpace(c.TextContent))
                .Select(c => c.TextContent)
                .ToList();

            var contentRows = doc.QuerySelectorAll("table.waffle > tbody > tr:not(:first-of-type)");
            var matches = new List<List<string>>();

            foreach (var thisRow in contentRows)
            {
                var row = (IHtmlTableRowElement)thisRow;
                var match = false;

                foreach (var cell in row.Cells)
                {
                    if (cell.TextContent.ToLowerInvariant().Contains(searchString))
                    {
                        match = true;
                        break;
                    }
                }

                if (match)
                    matches.Add(row.Cells.Select(c => c.TextContent).ToList());
            }

            return GenerateResult(headerFields, matches, origSearchString);
        }

        string GenerateResult(List<string> headers, List<List<string>> matches, string searchString)
        {
            var result = new StringBuilder();

            var matchCount = matches.Count;
            if (matchCount == 0)
            {
                return $"No results found for `{searchString}`.";
            }
            else if (matchCount > 4)
            {
                matchCount = 4;
                result.Append($"Total of {matches.Count} results for `{searchString}`; only the top four are shown.\n");
            }
            else
                result.Append($"{matchCount} results found for `{searchString}`.\n");

            for (var i = 0; i < matchCount; i++)
            {
                //result.AppendLine("```");

                for (var j = 0; j < matches[i].Count; j++)
                {
                    if (string.IsNullOrWhiteSpace(matches[i][j]))
                        continue;

                    result.Append(matches[i][j]);
                    result.Append(" | ");
                }
                result.Remove(result.Length - 3, 3);
                if (matchCount > 0 && i != matchCount - 1)
                    result.AppendLine(Environment.NewLine);

                //result.AppendLine("```");
            }
            string resultString = result.ToString();
            Regex regURL =  new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)");
            Match matchNA = regURL.Match(resultString);
            foreach (Match itemMatch in regURL.Matches(resultString))
            {
                resultString = resultString.Replace(itemMatch.ToString(), "<" + itemMatch.ToString() + ">");
            }

            return resultString;
        }

        /// <summary>
        /// Adds a lookup sheet to the database for the server & channel the command is run in.  User just needs to specify the sheet ID which can be pulled from the Google Sheet URL
        /// </summary>
        /// <param name="guildID"></param>
        /// <param name="channelID"></param>
        /// <param name="sheetID"></param>
        /// <returns>Returns true if it is a new add, false if there was an entry and we are modifying</returns>
        public async Task<bool> AddSheetIDAsync(ulong guildID, ulong channelID, string sheetID, string gid = "")
        {
            var existing = GetExistingLookupSheet(channelID, guildID);
            if (existing == null) { 
                var dbSheet = new SheetToSearch
                {
                    GuildId = guildID,
                    ChannelId = channelID,
                    SheetId = sheetID,
                    GId = gid
                };

                using (var db = new WbContext())
                {
                    db.SheetsToSearch.Add(dbSheet);
                    await db.SaveChangesAsync();
                }

                _sheets.Add(dbSheet);
                return true;
            }

            existing.GuildId = guildID;
            existing.ChannelId = channelID;
            existing.SheetId = sheetID;
            existing.GId = gid;

            using (var db = new WbContext())
            {
                db.SheetsToSearch.Update(existing);
                await db.SaveChangesAsync();
            }

            return false;
        }

        public async Task<bool> RemoveSheetIDAsync(ulong guildID, ulong channelID)
        {
            var existing = GetExistingLookupSheet(channelID, guildID);
            if (existing == null)
                return false;

            _sheets.Remove(existing);

            using (var db = new WbContext())
            {
                db.SheetsToSearch.Remove(existing);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public SheetToSearch GetExistingLookupSheet(ulong channelId, ulong guildId)
        {
            return _sheets.Find(r => r.ChannelId == channelId && r.GuildId == guildId);
        }

        public async Task ReloadSheetsAsync()
        {
            _sheets.Clear();

            await InitAsync();
        }
    }
}
