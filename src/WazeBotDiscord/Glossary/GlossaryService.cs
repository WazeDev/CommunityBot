using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WazeBotDiscord.Glossary
{
    public class GlossaryService
    {
        readonly HttpClient _httpClient;
        List<GlossaryItem> _items;
        readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        bool _initialized = false;

        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return; // double check after acquiring lock
                await UpdateGlossaryItemsAsync();
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public GlossaryService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("WazeBot");
            _items = new List<GlossaryItem>();
        }

        public async Task<GlossaryItem> GetGlossaryItem(string term)
        {
            await EnsureInitializedAsync();
            var item = _items.FirstOrDefault(i => i.Ids.Select(d => d.ToLowerInvariant().Replace('_', ' ')).Contains(term));
            if (item != null)
                return item;

            return _items.FirstOrDefault(i => i.Term.ToLowerInvariant() == term);
        }

        async Task UpdateGlossaryItemsAsync()
        {
            var parser = new HtmlParser();
            var body = await _httpClient.GetStringAsync("https://www.waze.com/discuss/t/glossary/377948");
            var doc = await parser.ParseDocumentAsync(body);
            var tblRows = doc.QuerySelectorAll("table tr");
            _items.Clear();

            foreach (var thisRow in tblRows)
            {
                var row = thisRow as IHtmlTableRowElement;
                if (row == null || row.Cells.Length < 4)
                    continue;

                // Skip header row
                var firstCell = row.Cells[0].TextContent.Trim();
                if (firstCell == "Glossary Term" || string.IsNullOrEmpty(firstCell))
                    continue;

                try
                {
                    var term = firstCell;
                    var alternates = row.Cells[1].TextContent.Trim();
                    if (string.IsNullOrEmpty(alternates) || alternates == "~")
                        alternates = "_(none)_";

                    var description = row.Cells[2].TextContent.Trim();

                    var dtString = row.Cells[3].TextContent.Trim();
                    dtString = dtString.Split('\n').Last(s => !string.IsNullOrWhiteSpace(s)).Trim();
                    // Extract just the date portion - dates are in format "yyyy-MM-dd"
                    var dateMatch = System.Text.RegularExpressions.Regex.Match(dtString, @"\d{4}-\d{2}-\d{2}");
                    if (!dateMatch.Success)
                        continue;

                    var dt = DateTime.ParseExact(dateMatch.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    // Build IDs from the term for anchor linking
                    var ids = new List<string> { term.Replace(" ", "_") };

                    _items.Add(new GlossaryItem
                    {
                        Ids = ids,
                        Term = term,
                        Alternates = alternates,
                        Description = description,
                        ModifiedAt = dt
                    });
                }
                catch
                {
                    // Skip malformed rows
                    continue;
                }
            }
        }

        public async Task ReloadGlossaryAsync()
        {
            await _initLock.WaitAsync();
            try
            {
                _initialized = false;
                _items.Clear();
            }
            finally
            {
                _initLock.Release();
            }
            await EnsureInitializedAsync();
        }
    }
}
