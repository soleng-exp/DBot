using System;
using System.Threading.Tasks;
using DBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;

namespace DBot
{
    public class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;
        static InteractivityModule interactivity;
        static VoiceNextClient voice;

        static void Main(string[] args)
        {
            try
            {
                MainAsync(args).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was an exception: {e.ToString()}");
            }
        }

        static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "your_token",
                TokenType = TokenType.Bot,
            });

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
            };

            interactivity = discord.UseInteractivity(new InteractivityConfiguration());
            voice = discord.UseVoiceNext(new VoiceNextConfiguration()
            {
                EnableIncoming = true
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = ";;",
                EnableDms = false
            });
            commands.RegisterCommands<Generals>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}