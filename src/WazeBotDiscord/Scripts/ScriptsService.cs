using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WazeBotDiscord.Classes.Servers;

namespace WazeBotDiscord.Scripts
{
    public class ScriptsService
    {
        readonly SheetsService _sheetsService;
        const string SheetId = "1yrEZMrQyMjhgBAJuNj7Y8z0GxdKWgIEkHIQBhUM2H9k";
        const string Range = "A:Z";

        public ScriptsService()
        {
            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
                ApplicationName = "WazeBotDiscord"
            });
        }

        public string GetChannelSheetUrl(ulong channelId)
        {
            return $"<https://docs.google.com/spreadsheets/d/{SheetId}/edit>";
        }

        public async Task<string> SearchSheetAsync(string origSearchString, ulong guildID)
        {
            var searchString = origSearchString.ToLowerInvariant();

            try
            {
                var request = _sheetsService.Spreadsheets.Values.Get(SheetId, Range);
                var response = await request.ExecuteAsync();
                var rows = response.Values;

                if (rows == null || rows.Count == 0)
                    return "Spreadsheet is not configured correctly.";

                //Sheets api only returns filled columns, so we need to pad out the missing columns so we can just loop through and read each, even if it's empty
                var headerCount = rows[0].Count;
                var paddedRows = rows.Select(row =>
                {
                    var padded = row.ToList();
                    while (padded.Count < headerCount)
                        padded.Add("");
                    return (IList<object>)padded;
                }).ToList();

                var matches = new List<IList<object>>();

                foreach (var row in paddedRows.Skip(1))
                {
                    var restrictedGuildsStr = row.Count > 6 ? row[6].ToString() : "";
                    var restrictedGuilds = restrictedGuildsStr.Split(",");
                    var hasRestrictedGuilds = restrictedGuilds.Length >= 1 && restrictedGuilds[0].Trim() != "";

                    // Block if restricted and came from a DM
                    if (hasRestrictedGuilds && guildID == 0)
                        continue;

                    // If restricted, only allow WazeScripts server or servers in the restricted list
                    if (hasRestrictedGuilds
                        && guildID != Servers.WazeScripts
                        && !Array.Exists(restrictedGuilds, element => element.Trim() == guildID.ToString()))
                        continue;

                    if (row.Any(cell => cell.ToString().ToLowerInvariant()
                        .Replace("-", " ").Contains(searchString.Replace("-", " "))))
                    {
                        matches.Add(row);
                    }
                }

                return GenerateResult(matches, origSearchString);
            }
            catch (Exception ex)
            {
                return $"Error accessing spreadsheet. Make sure the sheet is shared with 'Anyone with the link can view'. ({ex.Message})";
            }
        }

        string GenerateResult(List<IList<object>> matches, string searchString)
        {
            var result = new StringBuilder();

            var matchCount = matches.Count;
            if (matchCount == 0)
                return $"No results found for `{searchString}`.";
            else if (matchCount > 10)
            {
                matchCount = 10;
                result.Append($"Total of {matches.Count} results for `{searchString}`; only the top 10 are shown.\n");
            }
            else
                result.Append($"{matchCount} results found for `{searchString}`.\n");

            for (var i = 0; i < matchCount; i++)
            {
                //We want to return the first 4 columns of the sheet - script name, version, author, install link and forum/discuss link
                for (var j = 0; j < 5; j++)
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
            var regURL = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");
            foreach (Match itemMatch in regURL.Matches(resultString))
                resultString = resultString.Replace(itemMatch.ToString(), "<" + itemMatch.ToString() + ">");

            resultString = Regex.Replace(resultString, "<{2,}", "<");
            resultString = Regex.Replace(resultString, ">{2,}", ">");

            return resultString;
        }
    }
}