using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WazeBotDiscord.Autoreplies;
using WazeBotDiscord.ChannelSync;
using WazeBotDiscord.DND;
using WazeBotDiscord.Events;
using WazeBotDiscord.Find;
using WazeBotDiscord.Keywords;
using WazeBotDiscord.Lookup;
using WazeBotDiscord.ServerJoin;
using WazeBotDiscord.ServerLeave;

namespace WazeBotDiscord
{
    public class BotHostedService : IHostedService
    {
        readonly DiscordSocketClient _client;
        readonly CommandService _commands;
        readonly IServiceProvider _services;
        readonly AutoreplyService _autoreplyService;
        readonly KeywordService _keywordService;
        readonly DNDService _dndService;
        readonly ChannelSyncService _channelSyncService;
        readonly ServerLeaveService _serverLeaveService;
        readonly ServerJoinService _serverJoinService;
        readonly InteractionService _interactions;
        readonly LookupService _lookupService;
        bool isDev;

        public BotHostedService(DiscordSocketClient client, CommandService commands, IServiceProvider services,
            AutoreplyService autoreplyService,
            KeywordService keywordService,
            DNDService dndService,
            ChannelSyncService channelSyncService,
            ServerLeaveService serverLeaveService,
            ServerJoinService serverJoinService,
            InteractionService interactions,
            LookupService lookupService)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _autoreplyService = autoreplyService;
            _keywordService = keywordService;
            _dndService = dndService;
            _channelSyncService = channelSyncService;
            _serverLeaveService = serverLeaveService;
            _serverJoinService = serverJoinService;
            _interactions = interactions;
            _lookupService = lookupService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            isDev = true;// !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAZEBOT_ISDEV"));
            var token = Environment.GetEnvironmentVariable("DISCORD_API_TOKEN");
            if (token == null)
                throw new ArgumentNullException(nameof(token), "No Discord API token env var found");

            _client.Log += Log;

            // Hook up message handlers
            _client.MessageReceived += async (SocketMessage msg) => await AutoreplyHandler.HandleAutoreplyAsync(msg, _autoreplyService);
            _client.MessageReceived += async (SocketMessage msg) => await KeywordHandler.HandleKeywordAsync(msg, _keywordService, _client, _dndService);
            _client.MessageReceived += async (SocketMessage msg) => await ChannelSyncHandler.HandleChannelSyncAsync(msg, _channelSyncService, _client);
            _client.MessageReceived += HandleCommand;

            // Hook up user events
            _client.UserJoined += async (SocketGuildUser user) => await UserJoinedRoleSyncEvent.SyncRoles(user, _client);
            _client.UserLeft += async (SocketGuild guild, SocketUser user) => await UserLeftEvent.Alert(guild, user, _serverLeaveService);
            _client.UserJoined += async (SocketGuildUser user) => await UserJoinMessageEvent.SendMessage(user, _client, _serverJoinService);


            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services); 

            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.InteractionCreated += async interaction =>
            {
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactions.ExecuteCommandAsync(ctx, _services);
            };

            // Register commands with Discord on Ready
            _client.Ready += async () =>
            {
                //if (isDev)
                //    await _interactions.RegisterCommandsToGuildAsync(123);
                if (Environment.GetEnvironmentVariable("REGISTER_COMMANDS") == "true")
                {
                    await _interactions.RegisterCommandsGloballyAsync();
                    Console.WriteLine("Commands registered globally.");
                }
            };

            await _lookupService.WarmupAsync();
            //await _keywordService.WarmupAsync();
            // etc for other services

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null || message.Author.Id == _client.CurrentUser.Id)
                return;
            //if (isDev)
            //{
            //    var appInfo = await _client.GetApplicationInfoAsync();
            //    if (message.Author.Id != appInfo.Owner.Id)
            //        return;
            //}

            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            var context = new CommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
            else if (!result.IsSuccess && result.Error == CommandError.BadArgCount)
            {
                await context.Channel.SendMessageAsync($"{context.Message.Author.Mention}: You didn't specify the right " +
                    "parameters. If you're using a role command, you probably forgot to specify the user.");
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
            await _client.LogoutAsync();
        }

        Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
