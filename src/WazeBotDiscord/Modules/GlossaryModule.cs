using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using WazeBotDiscord.Glossary;

namespace WazeBotDiscord.Modules
{
    [Group("glossary", "Waze glossary commands")]
    public class GlossaryModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly GlossaryService _glossarySvc;

        public GlossaryModule(GlossaryService glossarySvc)
        {
            _glossarySvc = glossarySvc;
        }

        [SlashCommand("help", "Get help with the glossary command")]
        public async Task Help()
        {
            await RespondAsync("Use `/glossary search` to search the glossary for a term. Search terms must currently match exactly.\nThe glossary is located at: <https://www.waze.com/discuss/t/glossary/377948>", ephemeral: true);
        }

        [SlashCommand("search", "Search the Waze glossary for a term")]
        public async Task Search([Summary("term", "The term to search for")] string term)
        {
            await DeferAsync();
            var item = await _glossarySvc.GetGlossaryItem(term.ToLowerInvariant());
            if (item == null)
            {
                await FollowupAsync($"No match for `{term}`.");
                return;
            }
            var embed = CreateEmbed(item);
            await FollowupAsync(embed: embed);
        }

        Embed CreateEmbed(GlossaryItem item)
        {
            var urlID = item.Ids.Count > 0 ? item.Ids[0] : item.Term;
            return new EmbedBuilder()
            {
                Color = new Color(147, 196, 211),
                Title = item.Term,
                Url = $"https://www.waze.com/discuss/t/glossary/377948#{urlID}",
                Description = item.Description,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Last updated on {item.ModifiedAt.Date:yyyy-MM-dd}"
                }
            }.Build();
        }
    }
}