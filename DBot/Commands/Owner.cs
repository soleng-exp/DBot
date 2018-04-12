using System.Threading.Tasks;
using DBot.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DBot.Commands
{
    [Group("owner"), Aliases("o"), RequireOwner]
    public class Owner
    {
        private Dependencies dep;
        
        public Owner(Dependencies d)
        {
            this.dep = d;
        }

        [Command("shutdown")]
        public async Task ShutdownAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("Shutting down!");
            dep.Cts.Cancel();
        }
    }
}