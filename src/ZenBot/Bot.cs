using DiscordSharp;
using DiscordSharp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Zen.Zenbot
{
    public class Bot
    {
        private static string OwnerID;
        private static string DenyMsg = "I can't do that, {0}.";

        private const string CommandPrefix = @"&";
        private static Regex CommandPattern = new Regex(@"^" + CommandPrefix + @"([^\s]+)\s?(.*)");

        private static List<Command> Commands = new List<Command>
        {
            new Command("Logout",
                (c, m) => m.Author.ID == OwnerID,
                (c, m) => {
                    c.SendMessageToChannel("Goodbye!", m.Channel);
                    c.Logout();
                },
                (c, m) => c.SendMessageToChannel(string.Format(DenyMsg, m.Author.Username), m.Channel),
                (c, m, e) => Console.WriteLine(m.ToString() + Environment.NewLine + e.ToString())
            ),
        };

        private DiscordClient Client;
        private BotData Data;

        public Bot(DiscordClient Client, BotData Data)
        {
            this.Client = Client;
            this.Data = Data;

            OwnerID = Data.OwnerID;

            Client.MessageReceived += Client_MessageReceived;
            Client.MentionReceived += Client_MentionReceived;
            Client.Connected += Client_Connected;
            Client.SocketClosed += Client_SocketClosed;
            Client.UnknownMessageTypeReceived += Client_UnknownMessageTypeReceived;
            Client.TextClientDebugMessageReceived += Client_TextClientDebugMessageReceived;
        }

        private void Client_TextClientDebugMessageReceived(object sender, LoggerMessageReceivedArgs e)
        {
            Console.WriteLine("Debug: " + e.message.Message);
        }

        private void Client_UnknownMessageTypeReceived(object sender, UnknownMessageEventArgs e)
        {
            Console.WriteLine("Unknown Message Type Recieved: ");
            Console.WriteLine(e.RawJson.ToString());
        }

        private void Client_SocketClosed(object sender, DiscordSocketClosedEventArgs e)
        {
            Console.WriteLine("Connection closed: [" + e.Code + "] [" + e.WasClean + "] " + e.Reason);
        }

        private void Client_Connected(object sender, DiscordConnectEventArgs e)
        {
            Console.WriteLine("Connected.");
            Console.WriteLine("Username: " + e.User.Username);
            Console.WriteLine("ID: " + e.User.ID);
        }

        private void Client_MentionReceived(object sender, DiscordMessageEventArgs e)
        {
            // cleverbot like stupidity here.
            throw new NotImplementedException();
        }

        private void Client_MessageReceived(object sender, DiscordMessageEventArgs e)
        {
            if (e.Author.ID == Client.Me.ID)
                return;

            var extracted = CommandPattern.Match(e.Message.Content);

            if (!extracted.Success)
                return;

            var cmdName = extracted.Groups[1].Value.ToUpper();
            Commands.Where(c => c.Name.ToUpper() == cmdName).Single().Invoke(Client, e);
        }
    }
}
