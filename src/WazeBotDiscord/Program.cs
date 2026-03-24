using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using WazeBotDiscord.Abbreviation;
using WazeBotDiscord.Announce;
using WazeBotDiscord.Autoreplies;
using WazeBotDiscord.BotManagement;
using WazeBotDiscord.ChannelSync;
using WazeBotDiscord.DND;
using WazeBotDiscord.Events;
using WazeBotDiscord.Find;
using WazeBotDiscord.Fun;
using WazeBotDiscord.Glossary;
using WazeBotDiscord.Keywords;
using WazeBotDiscord.Lookup;
using WazeBotDiscord.Outreach;
using WazeBotDiscord.Scripts;
using WazeBotDiscord.ServerJoin;
using WazeBotDiscord.ServerLeave;
using WazeBotDiscord.Wikisearch;

namespace WazeBotDiscord
{
    public class Program
    {
        DiscordSocketClient client;
        CommandService commands;
        IServiceProvider services;
        static HttpClient httpClient;
        bool isDev;

        //public static void Main(string[] args)
        //    => new Program().RunAsync().GetAwaiter().GetResult();

        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                        LogLevel = LogSeverity.Info,
                        AlwaysDownloadUsers = true,
                        GatewayIntents = GatewayIntents.AllUnprivileged
                                       | GatewayIntents.MessageContent
                                       | GatewayIntents.GuildMembers
                                       | GatewayIntents.Guilds
                }));
                services.AddSingleton(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false
                });
                services.AddSingleton<CommandService>();
                services.AddHostedService<BotHostedService>();
                services.AddSingleton<InteractionService>(sp => new InteractionService(
                    sp.GetRequiredService<DiscordSocketClient>(),
                    new InteractionServiceConfig
                    {
                        DefaultRunMode = Discord.Interactions.RunMode.Async
                    }));

                services.AddHttpClient("WazeBot", client =>
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("WazeBotDiscord/1.0");
                });

                // Services that need HttpClient - inject IHttpClientFactory instead
                services.AddSingleton<GlossaryService>();
                services.AddSingleton<LookupService>();
                services.AddSingleton<OutreachService>();
                services.AddSingleton<ScriptsService>();
                services.AddSingleton<ServerJoinService>();
                services.AddSingleton<ServerLeaveService>();
                services.AddSingleton<DNDService>();
                services.AddSingleton<AbbreviationService>();
                services.AddSingleton<BotManagementService>();
                services.AddSingleton<AnnounceService>();
                services.AddSingleton<AutoreplyService>();
                services.AddSingleton<ChannelSyncService>();
                services.AddSingleton<FindService>();
                services.AddSingleton<FunService>();
                services.AddSingleton<KeywordService>();
                services.AddSingleton<WikisearchService>();
            });

        //public async Task RunAsync()
        //{
        //    isDev = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAZEBOT_ISDEV"));

        //    var token = Environment.GetEnvironmentVariable("DISCORD_API_TOKEN");
        //    if (token == null)
        //        throw new ArgumentNullException(nameof(token), "No Discord API token env var found");

        //    VerifyEnvironmentVariables();

        //    var validationKey = Environment.GetEnvironmentVariable("VALIDATION_KEY");
        //    var endpointURL = Environment.GetEnvironmentVariable("BOT_ENDPOINT_URL");

        //    var clientConfig = new DiscordSocketConfig
        //    {
        //        LogLevel = isDev ? LogSeverity.Info : LogSeverity.Warning,
        //        AlwaysDownloadUsers = true,
        //        GatewayIntents = GatewayIntents.AllUnprivileged
        //           | GatewayIntents.MessageContent
        //           | GatewayIntents.GuildMembers
        //           | GatewayIntents.Guilds,
        //        MessageCacheSize = 100
        //    };

        //    client = new DiscordSocketClient(clientConfig);
        //    client.Log += Log;

        //    var commandsConfig = new CommandServiceConfig
        //    {
        //        CaseSensitiveCommands = false
        //    };

        //    commands = new CommandService(commandsConfig);
        //    httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WazeBotDiscord/1.0");

        //    var autoreplyService = new AutoreplyService();
        //    await autoreplyService.InitAutoreplyServiceAsync();

        //    var keywordService = new KeywordService();
        //    await keywordService.InitKeywordServiceAsync();

        //    var glossaryService = new GlossaryService(httpClient);
        //    await glossaryService.InitAsync();

        //    var lookupService = new LookupService(httpClient);
        //    await lookupService.InitAsync();

        //    var outreachService = new OutreachService(httpClient);
        //    await outreachService.InitAsync();

        //    var scriptsService = new ScriptsService(httpClient);
        //    //await scriptsService.InitAsync();

        //    var serverLeaveService = new ServerLeaveService(httpClient);
        //    await serverLeaveService.InitServerLeaveServiceAsync();

        //    var funService = new FunService();

        //    var dndService = new DNDService(httpClient);
        //    await dndService.InitAsync();

        //    var announceService = new AnnounceService();
        //    await announceService.InitAnnounceServiceAsync(client);

        //    var serverJoinService = new ServerJoinService();
        //    await serverJoinService.InitAsync();

        //    var wikisearchService = new WikisearchService();

        //    var abbreviationService = new AbbreviationService(httpClient);

        //    var channelSyncService = new ChannelSyncService();
        //    await channelSyncService.InitAsync();

        //    var findService = new FindService();

        //    var botmanagementservice = new BotManagementService(httpClient, endpointURL, validationKey);


        //    var serviceCollection = new ServiceCollection();
        //    serviceCollection.AddSingleton(commands);
        //    serviceCollection.AddSingleton(autoreplyService);
        //    serviceCollection.AddSingleton(keywordService);
        //    serviceCollection.AddSingleton(lookupService);
        //    serviceCollection.AddSingleton(glossaryService);
        //    serviceCollection.AddSingleton(httpClient);
        //    serviceCollection.AddSingleton(scriptsService);
        //    serviceCollection.AddSingleton(outreachService);
        //    serviceCollection.AddSingleton(serverLeaveService);
        //    serviceCollection.AddSingleton(funService);
        //    serviceCollection.AddSingleton(dndService);
        //    serviceCollection.AddSingleton(announceService);
        //    serviceCollection.AddSingleton(serverJoinService);
        //    serviceCollection.AddSingleton(wikisearchService);
        //    serviceCollection.AddSingleton(abbreviationService);
        //    serviceCollection.AddSingleton(channelSyncService);
        //    serviceCollection.AddSingleton(findService);
        //    serviceCollection.AddSingleton(botmanagementservice);

        //    //client.Ready += async () => await client.SetGameAsync("with email addresses");

        //    services = serviceCollection.BuildServiceProvider();

        //    client.MessageReceived += async (SocketMessage msg) =>
        //        await AutoreplyHandler.HandleAutoreplyAsync(msg, autoreplyService); //, client.Guilds);

        //    client.MessageReceived += async (SocketMessage msg) =>
        //        await KeywordHandler.HandleKeywordAsync(msg, keywordService, client, dndService);

        //    client.MessageReceived += async (SocketMessage msg) =>
        //        await ChannelSyncHandler.HandleChannelSyncAsync(msg, channelSyncService, client);

        //    client.UserJoined += async (SocketGuildUser user) => await UserJoinedRoleSyncEvent.SyncRoles(user, client);
        //    client.UserLeft += async (SocketGuild guild, SocketUser user) => await UserLeftEvent.Alert(guild, user, serverLeaveService);
        //    client.UserJoined += async (SocketGuildUser user) => await UserJoinMessageEvent.SendMessage(user, client, serverJoinService);

        //    await InstallCommands(services);

        //    await client.LoginAsync(TokenType.Bot, token);
        //    await client.StartAsync();

        //    await Task.Delay(-1);
        //}

        //public async Task InstallCommands(IServiceProvider services)
        //{
        //    client.MessageReceived += HandleCommand;
        //    await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        //}

        //public async Task HandleCommand(SocketMessage messageParam)
        //{
        //    var message = messageParam as SocketUserMessage;
        //    if (message == null || message.Author.Id == client.CurrentUser.Id)
        //        return;

        //    if (isDev)
        //    {
        //        var appInfo = await client.GetApplicationInfoAsync();
        //        if (message.Author.Id != appInfo.Owner.Id)
        //            return;
        //    }

        //    int argPos = 0;
        //    if (!(message.HasStringPrefix("!lisa ", ref argPos) || message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos) ))
        //        return;
        //    //if (message.ToString().StartsWith("/"))
        //    //    argPos = 0;

        //    var context = new CommandContext(client, message);
        //    var result = await commands.ExecuteAsync(context, argPos, services);
        //    if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
        //    {
        //        await context.Channel.SendMessageAsync(result.ErrorReason);
        //    }
        //    else if (!result.IsSuccess && result.Error == CommandError.BadArgCount)
        //    {
        //        await context.Channel.SendMessageAsync($"{context.Message.Author.Mention}: You didn't specify the right " +
        //            "parameters. If you're using a role command, you probably forgot to specify the user.");
        //        await context.Channel.SendMessageAsync(result.ErrorReason);
        //    }
        //}

        //Task Log(LogMessage msg)
        //{
        //    Console.WriteLine(msg.ToString());
        //    return Task.CompletedTask;
        //}

        void VerifyEnvironmentVariables()
        {
            if (Environment.GetEnvironmentVariable("WAZEBOT_DB_CONNECTIONSTRING") == null)
                throw new ArgumentNullException("DB connection string env var not found", innerException: null);
            //if (Environment.GetEnvironmentVariable("COMMUNITYBOT_WAZE_LOGIN") == null)
            //    throw new ArgumentNullException("Waze account login not found", innerException: null);
            //if (Environment.GetEnvironmentVariable("COMMUNITYBOT_WAZE_PASSWORD") == null)
            //    throw new ArgumentNullException("Waze account password not found", innerException: null);
        }
    }
}
