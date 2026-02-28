using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Discord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WazeBotDiscord.Abbreviation
{
    public class AbbreviationService
    {
        readonly HttpClient _client;
        //List<SheetToSearch> _sheets;

        public AbbreviationService(HttpClient client)
        {
            _client = client;
        }

        //public async Task InitAsync()
        //{
        //    using (var db = new WbContext())
        //    {
        //        _sheets = await db.SheetsToSearch.ToListAsync();
        //    }
        //}

        public async Task<AbbreviationResponse> SearchSheetAsync(ulong channelId, string origSearchString)
        {
            var searchString = origSearchString.ToLowerInvariant();
                        var parser = new HtmlParser();
            string sheetURL = $"https://docs.google.com/spreadsheets/d/1-K3YMyIgos-fidRtMbKHFpIRzlAQu9DGYFk7PoedwzY/pubhtml/sheet?gid=1231483217&single=true";

            var resp = await _client.GetAsync(sheetURL);

            if (!resp.IsSuccessStatusCode)
                return new AbbreviationResponse { message = "Spreadsheet is not configured correctly." };

            var doc = await parser.ParseDocumentAsync(await resp.Content.ReadAsStringAsync());

            var tblHeader = doc.QuerySelectorAll("table.waffle > tbody > tr:nth-child(3)");
            var headerRowRaw = tblHeader.FirstOrDefault();
            if (headerRowRaw == null)
                return new AbbreviationResponse { message = "Spreadsheet is not configured correctly." };

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

                if (row.Cells[2].TextContent != "")
                { 
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
            }

            return GenerateResult(headerFields, matches, origSearchString);
        }

        AbbreviationResponse GenerateResult(List<string> headers, List<List<string>> matches, string searchString)
        {
            var result = new StringBuilder();
            AbbreviationResponse searchResult = new AbbreviationResponse();

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

            List<string> fullNames = new List<string>();
            List<string> mappedAs = new List<string>(); ;
            for (var i = 0; i < matchCount; i++)
            {
                //result.AppendLine("```");
                fullNames.Add(matches[i][1]);
                mappedAs.Add(matches[i][2]);
                if (matchCount > 0 && i != matchCount - 1)
                {
                    fullNames.Add(Environment.NewLine);
                    mappedAs.Add(Environment.NewLine);
                }

                //for (var j = 1; j < matches[i].Count; j++)
                //{
                //    if (string.IsNullOrWhiteSpace(matches[i][j]))
                //        continue;

                    //    result.Append(matches[i][j]);
                    //    result.Append(" | ");
                    //}
                    //result.Remove(result.Length - 3, 3);
                    //if (matchCount > 0 && i != matchCount - 1)
                    //    result.AppendLine(Environment.NewLine);

                    //result.AppendLine("```");
            }
            
                        
            var embed = new EmbedBuilder()
            {
                Color = new Color(147, 196, 211)
            };
            embed.AddField("Full name", String.Join("", fullNames.ToArray()));
            embed.AddField("Mapped as", String.Join("", mappedAs.ToArray()));
            searchResult.results = embed.Build();
            //string resultString = result.ToString();
            //Regex regURL = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)");
            //Match matchNA = regURL.Match(resultString);
            //foreach (Match itemMatch in regURL.Matches(resultString))
            //{
            //    resultString = resultString.Replace(itemMatch.ToString(), "<" + itemMatch.ToString() + ">");
            //}

            return searchResult;
        }
    }
}

