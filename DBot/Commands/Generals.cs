using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;

namespace DBot.Commands
{
    public class Generals
    {
        private ConcurrentDictionary<uint, Process> ffmpegs;

        [Command("hi")]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!");

            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Content.ToLower() == "how are you?", TimeSpan.FromMinutes(1));
            if (msg != null)
                await ctx.RespondAsync($"I'm fine, thank you!");
        }

        [Command("random")]
        public async Task Random(CommandContext ctx, int min, int max)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"🎲 Your random number is: {rnd.Next(min, max)}");
        }

        [Command("join"), System.ComponentModel.Description("Joins a voice channel.")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync($"Already connected in this guild.");
                return;
            }

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"You need to be in a voice channel.");
                return;
            }

            vnc = await vnext.ConnectAsync(chn);

            await ctx.RespondAsync($"👌 Connected to `{chn.Name}`");
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync($"Not connected in this guild.");
                return;
            }

            vnc.Disconnect();
            await ctx.RespondAsync("👌");
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string file)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync($"Not connected in this guild.");
                return;
            }

            if (!File.Exists(file))
            {
                await ctx.RespondAsync($"File was not found.");
                return;
            }

            await ctx.RespondAsync("👌");
            await vnc.SendSpeakingAsync(true); // send a speaking indicator

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{file}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi);
            var ffout = ffmpeg.StandardOutput.BaseStream;

            var buff = new byte[3840];
            var br = 0;
            while ((br = ffout.Read(buff, 0, buff.Length)) > 0)
            {
                if (br < buff.Length) // not a full sample, mute the rest
                    for (var i = br; i < buff.Length; i++)
                        buff[i] = 0;

                await vnc.SendAsync(buff, 20);
            }

            await vnc.SendSpeakingAsync(false); // we're not speaking anymore
        }


        [Command("join-s")]
        public async Task JoinS(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync($"Already connected in this guild.");
                return;
            }

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync($"You need to be in a voice channel.");
                return;
            }

            vnc = await vnext.ConnectAsync(chn);

            ffmpegs = new ConcurrentDictionary<uint, Process>();
            vnc.VoiceReceived += OnVoiceReceived;

            Console.WriteLine("ffmpegs - start");
            await ctx.RespondAsync($"👌 Connected to `{chn.Name}`");
        }

        [Command("leave-s")]
        public async Task LeaveS(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            vnc.VoiceReceived -= OnVoiceReceived;
            foreach (var kvp in this.ffmpegs)
            {
                await kvp.Value.StandardInput.BaseStream.FlushAsync();
                kvp.Value.StandardInput.Dispose();
                kvp.Value.WaitForExit();
            }

            Console.WriteLine("ffmpegs - stop");
            ffmpegs = null;

            vnc.Disconnect();

            await ctx.RespondAsync("👌");
        }

        public async Task OnVoiceReceived(VoiceReceiveEventArgs ea)
        {
            if (!ffmpegs.ContainsKey(ea.SSRC))
            {
                Console.WriteLine($"create input : [{ea.User.Username}]");
                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $@"-ac 2 -f s16le -ar 48000 -i pipe:0 -ac 2 -ar 44100 {ea.SSRC}.wav",
                    RedirectStandardInput = true
                };

                ffmpegs.TryAdd(ea.SSRC, Process.Start(psi));
            }
            Console.WriteLine($"Current works... [{ea.User.Username}]");

            var buff = ea.Voice.ToArray();

            var ffmpeg = ffmpegs[ea.SSRC];
            await ffmpeg.StandardInput.BaseStream.WriteAsync(buff, 0, buff.Length);
            await ffmpeg.StandardInput.BaseStream.FlushAsync();
        }
    }
}