using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DBot.Commands;
using DBot.Entities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;

namespace DBot
{
    public class Bot : IDisposable
    {
        private DiscordClient _discord;
        private CommandsNextModule _commands;
        private InteractivityModule _interactivity;
        private Config _config;
        
        private CancellationTokenSource _cts;

        public Bot()
        {
            if (!File.Exists("config.json"))
            {
                configNotFound();
            }
            _config = Config.LoadFromFile("config.json");
            
            _discord = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect = true,
                EnableCompression = true,
                LogLevel = LogLevel.Debug,
                Token = _config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });
            
            _cts = new CancellationTokenSource();
            _interactivity = _discord.UseInteractivity(new InteractivityConfiguration());
            
            ////
            DependencyCollection dep = null;
            using (var d = new DependencyCollectionBuilder())
            {
                d.AddInstance(new Dependencies()
                {
                    Interactivity = _interactivity,
//                    StartTimes = _starttimes,
                    Cts = _cts
                });
                dep = d.Build();
            }
            /// 

            _commands = _discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = _config.Prefix,
                EnableDms = false,
                Dependencies = dep,
                IgnoreExtraArguments = true,
                EnableDefaultHelp = true,
                CaseSensitive = false,
                EnableMentionPrefix = true,
            });
            _commands.RegisterCommands<Generals>();
            _commands.RegisterCommands<Owner>();
            
            ///
            _discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
            };
        }
        
        public async Task RunAsync()
        {
            await _discord.ConnectAsync();
            await WaitForCancellationAsync();
        }

        private async Task WaitForCancellationAsync()
        {
            while (!_cts.IsCancellationRequested)
                await Task.Delay(500);
        }
        
        public void Dispose()
        {
            _discord.Dispose();
            _interactivity = null;
            _commands = null;
            _config = null;
        }
        
        
        public void configNotFound()
        {
            new Config().SaveToFile("config.json");

            #region !! Report to user that config has not been set yet !! (aesthetics)

            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            WriteCenter("▒▒▒▒▒▒▒▒▒▄▄▄▄▒▒▒▒▒▒▒", 2);
            WriteCenter("▒▒▒▒▒▒▄▀▀▓▓▓▀█▒▒▒▒▒▒");
            WriteCenter("▒▒▒▒▄▀▓▓▄██████▄▒▒▒▒");
            WriteCenter("▒▒▒▄█▄█▀░░▄░▄░█▀▒▒▒▒");
            WriteCenter("▒▒▄▀░██▄░░▀░▀░▀▄▒▒▒▒");
            WriteCenter("▒▒▀▄░░▀░▄█▄▄░░▄█▄▒▒▒");
            WriteCenter("▒▒▒▒▀█▄▄░░▀▀▀█▀▒▒▒▒▒");
            WriteCenter("▒▒▒▄▀▓▓▓▀██▀▀█▄▀▀▄▒▒");
            WriteCenter("▒▒█▓▓▄▀▀▀▄█▄▓▓▀█░█▒▒");
            WriteCenter("▒▒▀▄█░░░░░█▀▀▄▄▀█▒▒▒");
            WriteCenter("▒▒▒▄▀▀▄▄▄██▄▄█▀▓▓█▒▒");
            WriteCenter("▒▒█▀▓█████████▓▓▓█▒▒");
            WriteCenter("▒▒█▓▓██▀▀▀▒▒▒▀▄▄█▀▒▒");
            WriteCenter("▒▒▒▀▀▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒");
            Console.BackgroundColor = ConsoleColor.Yellow;
            WriteCenter("WARNING", 3);
            Console.ResetColor();
            WriteCenter("Thank you Mario!", 1);
            WriteCenter("But our config.json is in another castle!");
            WriteCenter("(Please fill in the config.json that was generated.)", 2);
            WriteCenter("Press any key to exit..", 1);
            Console.SetCursorPosition(0, 0);
            Console.ReadKey();

            #endregion

            Environment.Exit(0);
        }

        internal void WriteCenter(string value, int skipline = 0)
        {
            for (int i = 0; i < skipline; i++)
                Console.WriteLine();

            Console.SetCursorPosition((Console.WindowWidth - value.Length) / 2, Console.CursorTop);
            Console.WriteLine(value);
        }
    }
}