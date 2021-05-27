using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WazeBotDiscord.XKCD;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace WazeBotDiscord.Modules
{
    public class XKCDModule : ModuleBase
    {
        XKCDResult xkcdresult;

        [Command("xkcd")]
        public async Task xkcd([Remainder]string comicNum = null)
        {
            if(comicNum == null || new Regex(@"^\d+$").IsMatch(comicNum))
            {
                await GetComic(comicNum);
                var embed = await Task.Run(() => CreateEmbed(xkcdresult));
                await ReplyAsync("", embed: embed);
            }
            else
            {
                await ReplyAsync("Incorrect parameters provided. For a random comic use `!xkcd`, for a specific comic provide the comic number ex: `!xkcd 149`.");
            }
            
        }

        async Task GetComic(string comicNum)
        {
            xkcdresult = new XKCDResult();
            string url = "https://c.xkcd.com/random/comic/"; //default to a random comic

            if (!(comicNum == null)) {
                url = "https://xkcd.com/" + comicNum;
            }
            HttpClient httpClient = new HttpClient();
            string body = await httpClient.GetStringAsync(url);
            
            Regex regTitle = new Regex("id=\"ctitle\">(.*?)</div>");
            Match matchTitle = regTitle.Match(body);
            Regex regImageURL = new Regex("<div id=\"comic\">(\\s|\\n)<img src=\"(.*?)\"");
            Match matchImageURL = regImageURL.Match(body);
            Regex regTitleText = new Regex("title=\"(.*?)\" alt");
            Match matchTitleText = regTitleText.Match(body);
            xkcdresult.Title = matchTitle.Groups[1].ToString();
            xkcdresult.ImageURL = matchImageURL.Groups[2].ToString().Replace("//", "https://");
            xkcdresult.AltText = WebUtility.HtmlDecode(matchTitleText.Groups[1].ToString());
        }

        Embed CreateEmbed(XKCDResult xkcdresult)
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(147, 196, 211),
                Title = "xkcd: " + xkcdresult.Title,
                Url = $"http://www.xkcd.com",
                ImageUrl = xkcdresult.ImageURL,

                Footer = new EmbedFooterBuilder
                {
                    Text = xkcdresult.AltText
                }
            };

            return embed.Build();
        }
    }
}
