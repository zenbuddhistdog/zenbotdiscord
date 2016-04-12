using System;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using DiscordSharp;

namespace Zen.Zenbot
{
    static class Program
    {
        private const string SettingsFile = "Zenbot.xml";

        private static string token;
        private static Bot bot;
        private static DiscordClient client;
        private static Thread thread;

        public static void LoadSettings(string settingsFileName)
        {
            var root = XElement.Load(settingsFileName);

            XElement tokenElement = root.Elements("//Token").Single();

            if (tokenElement == null)
                throw new Exception("Token element not found.");

            if (string.IsNullOrWhiteSpace(tokenElement.Value))
                throw new Exception("Token element does not contain a token.");

            token = tokenElement.Value;
        }

        public static void Main(string[] args)
        {
            LoadSettings(SettingsFile);

            client = new DiscordClient(token, true, true);
            bot = new Bot(client);

            thread = new Thread(client.Connect);
            thread.Start();
        }
    }
}
