using System.Threading;
using DiscordSharp;

namespace Zen.Zenbot
{
    static class Program
    {
        private static Bot bot;
        private static DiscordClient client;
        private static Thread thread;

        public static void Main(string[] args)
        {
            client = new DiscordClient("Token", true, true);
            bot = new Bot(client);

            thread = new Thread(client.Connect);
            thread.Start();
        }
    }
}
