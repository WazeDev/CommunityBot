using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WazeBotDiscord.Fun;

namespace WazeBotDiscord.Modules
{
    public class FunModule : InteractionModuleBase<SocketInteractionContext>
    {

        readonly FunService _funSvc;

        public FunModule(FunService funSvc)
        {
            _funSvc = funSvc;
        }

        #region "Slap"
        //[Command("/slap")]
        //public async Task SlapUser([Remainder]string message = null)
        //{
        //    string nickname = ((Discord.WebSocket.SocketGuildUser)Context.User).Nickname;
        //    if (nickname == null)
        //        nickname = Context.User.Username;
        //    await ReplyAsync($"{nickname} slaps {message} around a bit with a large trout.");
        //}
        [SlashCommand("slap", "Slap someone with a large trout")]
        public async Task SlapUser([Summary("target", "Who to slap")] string target)
        {
            var user = (SocketGuildUser)Context.User;
            var name = user.Nickname ?? user.GlobalName ?? user.Username;
            await RespondAsync($"{name} slaps {target} around a bit with a large trout.");
        }
        #endregion

        #region "Diceroll"
        //[Command("diceroll")]
        //public async Task Diceroll([Remainder]string message = null)
        [SlashCommand("diceroll", "Roll some dice")]
        public async Task Diceroll([Summary("dice", "Dice to roll e.g. 2d6")] string dice = "1d6")

        {
            StringBuilder sb = new StringBuilder();
            Regex regURL = new Regex(@"(\d+)d(\d+)");

            if (dice == null)
                dice = "1d6";

            if (regURL.Matches(dice).Count > 1)
            {
                int sum = 0;

                sb.Append($"`{dice}` =");
                foreach (Match itemMatch in regURL.Matches(dice))
                {
                    int numDie = Convert.ToInt32(itemMatch.Groups[1].ToString());
                    int sides = Convert.ToInt32(itemMatch.Groups[2].ToString());

                    var result = RollTheDice(numDie, sides);
                    if (sum > 0)
                        sb.Append("+");
                    sb.Append(result.Item2);
                    sum += result.Item1;
                }
                sb.Append($" = {sum}");
            }
            else
            {
                Match die = regURL.Match(dice);
                int numDie = Convert.ToInt32(die.Groups[1].ToString());
                int sides = Convert.ToInt32(die.Groups[2].ToString());
                int sum = 0;

                sb.Append($"`{die.Groups[0].ToString()}`");
                var result = RollTheDice(numDie, sides);
                sum += result.Item1;
                if (numDie > 1)
                    sb.Append(" =");
                sb.Append(result.Item2);
                sb.Append($" = {sum}");
            }

            //await ReplyAsync(sb.ToString());
            await RespondAsync(sb.ToString());
        }

        private Tuple<int, string> RollTheDice(int numDie, int numSides)
        {
            Random rnd = new Random();
            int roll, sum = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < numDie; i++)
            {
                roll = rnd.Next(1, numSides);
                if (numDie > 1)
                {
                    if (i > 0)
                        sb.Append("+");
                    else
                        sb.Append(" (");
                    sb.Append(roll);
                }
                sum += roll;
            }
            if (numDie > 1)
                sb.Append(")");
            return Tuple.Create(sum, sb.ToString());
        }
        #endregion

        #region "Dad jokes"
        //[Command("dadjoke")]
        //public async Task GetDadJoke([Remainder]string message = null)
        //{
        //    var result = _funSvc.GetWebRequest("https://icanhazdadjoke.com/slack", "application/json; charset=utf-8").Result;

        //    var dadJoke = JsonConvert.DeserializeObject<TheDadJoke>(result);
        //    await ReplyAsync(dadJoke.attachments[0].text);
        //}
        [SlashCommand("dadjoke", "Get a dad joke")]
        public async Task GetDadJoke()
        {
            await DeferAsync(); // HTTP call may take > 3 seconds
            var result = await _funSvc.GetWebRequest("https://icanhazdadjoke.com/slack", "application/json; charset=utf-8");
            var dadJoke = JsonConvert.DeserializeObject<TheDadJoke>(result);
            await FollowupAsync(dadJoke.attachments[0].text);
        }
        public class TheDadJoke
        {
            public List<Attachment> attachments { get; set; }
            public string response_type { get; set; }
            public string username { get; set; }
        }

        public class Attachment
        {
            public string fallback { get; set; }
            public string footer { get; set; }
            public string text { get; set; }
        }
        #endregion

        #region "Facts"
        [SlashCommand("dogfact", "Get a dog fact")]
        public async Task GetDogFact()
        {
            await DeferAsync();
            var result = await _funSvc.GetWebRequest("https://dogapi.dog/api/v2/facts?limit=1", "application/json; charset=utf-8");
            var dogFact = JsonConvert.DeserializeObject<DogFacts>(result);
            await FollowupAsync(dogFact.data[0].attributes.body);
        }

        //{"facts":["An American Animal Hospital Association poll found that 33% of dog owners admit to talking to their dogs on the phone and leaving answering machine messages for them while away."],"success":true}
        public class DogFacts
        {
            public List<DogFact> data { get; set; }
        }

        public class DogFact
        {
            public string id { get; set; }
            public string type { get; set; }
            public DogFactAttributes attributes { get; set; }
        }

        public class DogFactAttributes
        {
            public string body { get; set; }
        }


        #endregion
    }
}
