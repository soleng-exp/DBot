using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Chatbot
{
    public class Program
    {
        private const string Token = "NDMzMjAyNzkyMTQ0ODMwNDc0.Da5lIg.XPi51Srkgmf9EIWlJrUfAhtBc4A"; // Remember to keep this private!
        private DiscordSocketClient _client;
        
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;
//            _client.MessageReceived += MessageReceived;

            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        
        private static async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "!ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
        }
        
        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}