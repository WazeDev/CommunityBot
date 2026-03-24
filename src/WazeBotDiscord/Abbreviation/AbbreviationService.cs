using Discord;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WazeBotDiscord.Abbreviation
{
    public class AbbreviationService
    {
        readonly SheetsService _sheetsService;
        const string SheetId = "1-K3YMyIgos-fidRtMbKHFpIRzlAQu9DGYFk7PoedwzY";
        const string Range = "A:C"; // Columns A, B, C
        readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        bool _initialized = false;

        public AbbreviationService()
        {
            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
                ApplicationName = "WazeBotDiscord"
            });
        }

        public async Task<AbbreviationResponse> SearchSheetAsync(ulong channelId, string origSearchString)
        {
            var searchString = origSearchString.ToLowerInvariant();

            var request = _sheetsService.Spreadsheets.Values.Get(SheetId, Range);
            request.Key = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            var response = await request.ExecuteAsync();
            var rows = response.Values;

            if (rows == null || rows.Count == 0)
                return new AbbreviationResponse { message = "Spreadsheet is not configured correctly." };

            // Skip header rows — first two rows are headers
            var matches = rows.Skip(3)
                .Where(row => row.Count >= 3
                    && row.Any(cell => cell.ToString().ToLowerInvariant().Contains(searchString)))
                .ToList();

            return GenerateResult(matches, origSearchString);
        }

        AbbreviationResponse GenerateResult(List<IList<object>> matches, string searchString)
        {
            var searchResult = new AbbreviationResponse();

            var matchCount = matches.Count;
            if (matchCount == 0)
            {
                searchResult.message = $"No results found for `{searchString}`.";
                return searchResult;
            }
            else if (matchCount > 10)
            {
                matchCount = 10;
                searchResult.message = $"Total of {matches.Count} results for `{searchString}`; only the top ten are shown.\n";
            }
            else
                searchResult.message = $"{matchCount} results found for `{searchString}`.\n";

            var fullNames = new List<string>();
            var mappedAs = new List<string>();

            for (var i = 0; i < matchCount; i++)
            {
                fullNames.Add(matches[i][0].ToString());
                mappedAs.Add(matches[i][1].ToString());
                if (i != matchCount - 1)
                {
                    fullNames.Add(Environment.NewLine);
                    mappedAs.Add(Environment.NewLine);
                }
            }

            var embed = new EmbedBuilder()
            {
                Color = new Color(147, 196, 211)
            };
            embed.AddField("Full name", string.Join("", fullNames));
            embed.AddField("Mapped as", string.Join("", mappedAs));
            searchResult.results = embed.Build();

            return searchResult;
        }
    }
}