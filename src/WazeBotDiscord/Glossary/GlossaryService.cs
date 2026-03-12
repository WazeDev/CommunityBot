using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WazeBotDiscord.Glossary
{
    public class GlossaryService
    {
        readonly HttpClient _httpClient;

        List<GlossaryItem> _items;

        public GlossaryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _items = new List<GlossaryItem>();
        }

        public async Task InitAsync()
        {
            await UpdateGlossaryItemsAsync();
        }

        public GlossaryItem GetGlossaryItem(string term)
        {
            var item = _items.FirstOrDefault(i => i.Ids.Select(d => d.ToLowerInvariant().Replace('_', ' ')).Contains(term));
            if (item != null)
                return item;

            return _items.FirstOrDefault(i => i.Term.ToLowerInvariant() == term);
        }

        async Task UpdateGlossaryItemsAsync()
        {
            var parser = new HtmlParser();

            // 1. Fetch the JSON endpoint instead of the raw web page
            var jsonString = await _httpClient.GetStringAsync("https://www.waze.com/discuss/t/glossary/377948.json");

            // 2. Parse the JSON to navigate the Discourse structure
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;

            // Traverse: post_stream -> posts -> first post -> cooked (the HTML content)
            var cookedHtml = root
                .GetProperty("post_stream")
                .GetProperty("posts")[0]
                .GetProperty("cooked")
                .GetString();

            if (string.IsNullOrEmpty(cookedHtml)) return;

            // 3. Parse ONLY the HTML of the post itself
            var doc = await parser.ParseDocumentAsync(cookedHtml);

            // Because we are only looking at the post content, we can grab the table directly
            var tblRows = doc.QuerySelectorAll("table tr").Skip(1); // Skip the header row

            _items.Clear();

            foreach (var thisRow in tblRows)
            {
                var row = (IHtmlTableRowElement)thisRow;

                // Ensure the row has enough cells
                if (row.Cells.Length >= 3)
                {
                    try
                    {
                        // Parse Term (Fallback to cell text if bold tags are missing)
                        var termElement = row.Cells[0].QuerySelector("b, strong") ?? row.Cells[0];
                        var term = termElement.TextContent.Trim();

                        // Extract IDs/Anchors (Discourse sometimes uses 'name' on anchor tags instead of spans)
                        var ids = row.Cells[0].QuerySelectorAll("span[id], a[name]")
                                              .Select(c => c.Id?.Trim() ?? c.GetAttribute("name")?.Trim())
                                              .Where(id => !string.IsNullOrEmpty(id));

                        // Alternates
                        var alternates = row.Cells[1].TextContent.Trim();
                        if (string.IsNullOrEmpty(alternates) || alternates == "~")
                            alternates = "_(none)_";

                        // Description (Handle line breaks)
                        var descriptionCell = row.Cells[2];
                        var description = descriptionCell.InnerHtml
                            .Replace("<p>", "\n")
                            .Replace("</p>", "")
                            .Replace("<br>", "\n")
                            .Trim();

                        // Modified Date
                        DateTime dt = DateTime.MinValue;
                        if (row.Cells.Length > 3)
                        {
                            var dtString = row.Cells[3].TextContent.Trim().Split(' ')[0];
                            if (DateTime.TryParse(dtString, out var parsedDate))
                            {
                                dt = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                            }
                        }

                        _items.Add(new GlossaryItem
                        {
                            Ids = ids.ToList(),
                            Term = term,
                            Alternates = alternates,
                            Description = description,
                            ModifiedAt = dt
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log or handle individual row parsing errors safely
                        Console.WriteLine($"Error parsing row '{row.TextContent.Substring(0, 10)}...': {ex.Message}");
                    }
                }
            }
        }

        public async Task ReloadGlossaryAsync()
        {
            _items.Clear();
            await InitAsync();
        }
    }
}
