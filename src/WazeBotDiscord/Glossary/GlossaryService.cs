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
                //await UpdateGlossaryItemsAsync();
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

            var tblRows = doc.QuerySelectorAll("tr");

            _items.Clear();

            foreach (var thisRow in tblRows)
            {
                var row = (IHtmlTableRowElement)thisRow;
                if (row.Cells.Length > 2)
                {
                    var dtString = row.Cells[3].TextContent.Trim();
                    dtString = dtString.Split(null)[0];

                    var dt = DateTime.ParseExact(dtString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime.SpecifyKind(dt, DateTimeKind.Utc);

                    var alternates = row.Cells[1].TextContent.Trim();
                    if (string.IsNullOrEmpty(alternates) || alternates == "~")
                        alternates = "_(none)_";

                    var term = row.Cells[0].Children.First(c => c.TagName == "B").TextContent.Trim();
                    var ids = row.Cells[0].Children.Where(c => c.TagName == "SPAN").Select(c => c.Id.Trim());
                    row.Cells[2].InnerHtml = row.Cells[2].InnerHtml.Replace("<p>", "\n").Replace("</p>", "").Replace("<br>", "\n");
                    _items.Add(new GlossaryItem
                    {
                        Ids = ids.ToList(),
                        Term = term,
                        Alternates = alternates,
                        Description = row.Cells[2].TextContent.Trim(),
                        ModifiedAt = dt
                    });
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
