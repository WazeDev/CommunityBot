using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WazeBotDiscord.BotManagement
{
    public class BotManagementService
    {
        HttpClient _client;
        string _endpointURL;
        string _validationKey;

        public BotManagementService(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("WazeBot");
            _endpointURL = Environment.GetEnvironmentVariable("BOT_ENDPOINT_URL");
            _validationKey = Environment.GetEnvironmentVariable("VALIDATION_KEY");
        }

        public async Task<Boolean> ExecuteBotService(string command)
        {
            try
            {
                await _client.GetAsync(_endpointURL + command + "/" + _validationKey);
                return true;
            }
            catch(Exception ex)
            {
                return false;
                /*var log = "Endpoint: " + _endpointURL +
                    "\nValidation key: " + _validationKey +
                    "\n\n Error: " + ex.ToString();
                Console.WriteLine(log);
                return log;*/
            }
            
        }
    }
}
