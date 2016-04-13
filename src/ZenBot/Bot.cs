using DiscordSharp;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChatterBotAPI;

namespace Zen.Zenbot
{
    public class Bot
    {
        private static string DenyMsg = "I can't do that, {0}.";
        
        private Regex CommandPattern;
        private List<Command> Commands;
        private DiscordClient Client;
        private ChatterBot Chatter;
        private ChatterBotSession ChatSession;
        private BotData Data;

        public Bot(DiscordClient Client, BotData Data)
        {
            this.Client = Client;
            this.Data = Data;

            Chatter = new ChatterBotFactory().Create(ChatterBotType.CLEVERBOT);
            ChatSession = Chatter.CreateSession();

            Client.MessageReceived += Client_MessageReceived;
            Client.MentionReceived += Client_MentionReceived;
            Client.Connected += Client_Connected;
            Client.SocketClosed += Client_SocketClosed;
            Client.UnknownMessageTypeReceived += Client_UnknownMessageTypeReceived;
            Client.TextClientDebugMessageReceived += Client_TextClientDebugMessageReceived;

            CommandPattern = new Regex(@"^" + Data.Prefix + @"([^\s]+)\s?(.*)");

            BuildCommands();
        }

        private void BuildCommands()
        {
            Commands = new List<Command> {
                new Command("Logout",
                    (m) => m.Author.ID == Data.OwnerID,
                    (m) => {
                        Client.SendMessageToChannel("Goodbye!", m.Channel);
                        Client.Logout();
                    },
                    (m) => Client.SendMessageToChannel(string.Format(DenyMsg, m.Author.Username), m.Channel),
                    (m, e) => Console.WriteLine(m.ToString() + Environment.NewLine + e.ToString())
                ),
            };
        }

        private void Chat(string message, DiscordChannel channel)
        {
            Client.SendMessageToChannel(ChatSession.Think(message), channel);
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
            Chat(e.MessageText, e.Channel);
        }

        private void Client_MessageReceived(object sender, DiscordMessageEventArgs e)
        {
            if (e.Author.ID == Client.Me.ID)
                return;

            var extracted = CommandPattern.Match(e.Message.Content);

            if (!extracted.Success)
                return;

            var cmdName = extracted.Groups[1].Value.ToUpper();

            try
            {
                Commands.Where(c => c.Name.ToUpper() == cmdName).Single().Invoke(e);
            }
            catch (InvalidOperationException)
            {
                Client.SendMessageToChannel(
                    string.Format("Sorry {0}, I have no idea how to '{1}'.", e.Author.Username, extracted.Groups[1].Value),
                    e.Channel);
            }
        }
    }
}
