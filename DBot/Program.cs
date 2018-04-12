using System;

namespace DBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var b = new Bot())
                {
                    b.RunAsync().Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was an exception: {e.ToString()}");
            }
        }
    }
}