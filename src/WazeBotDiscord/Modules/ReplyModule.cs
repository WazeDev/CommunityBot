using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using WazeBotDiscord.Autoreplies;

namespace WazeBotDiscord.Modules
{
    public class ReplyModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly AutoreplyService _arService;

        public ReplyModule(AutoreplyService arService)
        {
            _arService = arService;
        }

        [SlashCommand("reply", "Post an autoreply")]
        public async Task Reply(
            [Summary("trigger", "The autoreply to post"), Autocomplete(typeof(AutoreplyAutocompleteHandler))] string trigger)
        {
            var key = trigger.Trim().TrimStart('!').ToLowerInvariant();
            var available = await _arService.GetAvailableAutorepliesAsync(Context.Channel?.Id, Context.Guild?.Id);
            var ar = available.FirstOrDefault(a => a.Trigger == key);

            if (ar == null)
            {
                await RespondAsync($"No autoreply found for `{key}`. Use `/autoreply list` to see what's available here.", ephemeral: true);
                return;
            }

            // Allow user pings in replies, but never @everyone/@here or role pings
            await RespondAsync(ar.Reply, allowedMentions: new AllowedMentions(AllowedMentionTypes.Users));
        }
    }

    public class AutoreplyAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            var arService = services.GetRequiredService<AutoreplyService>();
            var typed = (autocompleteInteraction.Data.Current.Value as string ?? "").Trim().TrimStart('!').ToLowerInvariant();

            var available = await arService.GetAvailableAutorepliesAsync(context.Channel?.Id, context.Guild?.Id);

            // Discord shows at most 25 suggestions; prefix matches first
            var suggestions = available
                .Where(a => a.Trigger.Contains(typed))
                .OrderBy(a => !a.Trigger.StartsWith(typed))
                .ThenBy(a => a.Trigger)
                .Take(25)
                .Select(a => new AutocompleteResult(a.Trigger, a.Trigger));

            return AutocompletionResult.FromSuccess(suggestions);
        }
    }
}
