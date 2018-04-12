using System.Threading;
using DSharpPlus.Interactivity;

namespace DBot.Entities
{
    public class Dependencies
    {
        internal InteractivityModule Interactivity { get; set; }
//        internal StartTimes StartTimes { get; set; }
        internal CancellationTokenSource Cts { get; set; }
    }
}