namespace NameBadger.Bot
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Enums;
    using DSharpPlus.Interactivity.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NameBadger.Bot.Contexts;
    using NameBadger.Bot.Services;

    internal static class Program
    {
        private static IConfiguration        _config;
        private static DiscordClient         _discordClient;
        private static IServiceProvider      _service;
        private static CommandsNextExtension _commands;

        private static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            BuildConfiguration();

            CreateDiscordClient();

            BuildServiceProvider();

            ConfigureCommands();

            ConfigureInteractivity();

            _commands.RegisterCommands(Assembly.GetExecutingAssembly());

            await _discordClient.ConnectAsync(new DiscordActivity("your funny names", ActivityType.ListeningTo));

            await using var db = new NameBadgeContext();
            await db.Database.MigrateAsync();

            _service.GetRequiredService<NameBadgeService>();

            await Task.Delay(-1);
        }

        private static void BuildConfiguration()
        {
            _config = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                     .AddJsonFile("appsettings.json", false)
                     .Build();
        }

        private static void CreateDiscordClient()
        {
            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token              = _config["Bot:Token"],
                TokenType          = TokenType.Bot,
                MinimumLogLevel    = LogLevel.Debug,
                LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt"
            });
        }

        private static void BuildServiceProvider()
        {
            _service = new ServiceCollection()
                      .AddSingleton(_discordClient)
                      .AddSingleton<NameBadgeService>()
                      .BuildServiceProvider();
        }

        private static void ConfigureCommands()
        {
            _commands = _discordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"<B "},
                Services       = _service
            });
        }

        private static void ConfigureInteractivity()
        {
            _discordClient.UseInteractivity(new InteractivityConfiguration
            {
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout       = TimeSpan.FromSeconds(30)
            });
        }
    }
}