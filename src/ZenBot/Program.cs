using System;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using DiscordSharp;

namespace Zen.Zenbot
{
    public struct BotData
    {
        public string Username;
        public string Secret;
        public string Token;
        public string BotID;
        public string ClientID;
        public string OwnerID;
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

                data = root.Elements("Bot").Select(el => new BotData
                {
                    Username = el.Element("Username").Value,
                    Secret = el.Element("Secret").Value,
                    Token = el.Element("Token").Value,
                    BotID = el.Element("BotID").Value,
                    ClientID = el.Element("ClientID").Value,
                    OwnerID = el.Element("OwnerID").Value
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
                
                client = new DiscordClient(data.Token, true);
                client.WriteLatestReady = true;

                Console.WriteLine("Creating bot.");

                bot = new Bot(client, data);

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
                Console.WriteLine("Logging in.");
                var request = client.SendLoginRequest();

                if (request == null)
                    throw new Exception("Client failed to login.");
                
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
            Run();
            
            Console.ReadKey();

            client.Logout();
            client.Dispose();
            
            Thread.Sleep(2000);

            Environment.Exit(0);
        }
    }
}
