using System;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using DiscordSharp;

namespace Zen.Zenbot
{
    struct BotData
    {
        public string Username;
        public string Secret;
        public string Token;
        public string BotID;
        public string ClientID;
    }

    static class Program
    {
        private const string SettingsFile = "Zenbot.xml";

        private static Bot bot;
        private static BotData data;
        private static DiscordClient client;
        private static Thread thread;

        private static void LoadSettings(string settingsFileName)
        {
            try
            {
                Console.WriteLine("Loading settings");

                var root = XElement.Load(settingsFileName);

                data = root.Elements("//Bot").Select(el => new BotData
                {
                    Username = el.Element("Username").Value,
                    Secret = el.Element("Secret").Value,
                    Token = el.Element("Token").Value,
                    BotID = el.Element("BotID").Value,
                    ClientID = el.Element("ClientID").Value
                }).Single();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load settings: " + Environment.NewLine + e.ToString());
            }
        }

        private static void CreateBot()
        {
            try
            {
                Console.WriteLine("Creating client.");

                client = new DiscordClient(data.Token, true, true);
                client.ClientPrivateInformation.Email = "";
                client.ClientPrivateInformation.Password = "";
                client.ClientPrivateInformation.Username = data.Username;

                Console.WriteLine("Creating bot.");

                bot = new Bot(client);

                Console.WriteLine("Creating thread.");

                thread = new Thread(client.Connect);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load create bot: " + Environment.NewLine + e.ToString());
            }
        }

        private static void Run()
        {
            try
            {
                Console.WriteLine("Starting.");
                thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Client threw exception: " + Environment.NewLine + e.ToString());
            }
        }

        public static void Main(string[] args)
        {
            LoadSettings(SettingsFile);
            CreateBot();
            
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();

            Environment.Exit(0);
        }
    }
}
