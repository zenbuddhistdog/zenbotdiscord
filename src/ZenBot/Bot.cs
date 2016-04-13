using ChatterBotAPI;
using DiscordSharp;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Zen.Zenbot
{
    public class Bot
    {
        private static string DenyMsg = "I can't do that, {0}.";
        
        private Regex CommandPattern;
        private List<Command> Commands;
        private DiscordClient Client;
        
        private ChatterBotSession ChatSession;
        private Thread ChatThread;

        private BotData Data;
        private bool Reconnecting;

        public Bot(DiscordClient Client, BotData Data)
        {
            this.Client = Client;
            this.Data = Data;

            Reconnecting = false;

            Client.MessageReceived += Client_MessageReceived;
            Client.MentionReceived += Client_MentionReceived;
            Client.Connected += Client_Connected;
            Client.SocketClosed += Client_SocketClosed;
            Client.SocketOpened += Client_SocketOpened;
            Client.UnknownMessageTypeReceived += Client_UnknownMessageTypeReceived;
            Client.TextClientDebugMessageReceived += Client_TextClientDebugMessageReceived;

            CommandPattern = new Regex(@"^" + Data.Prefix + @"([^\s]+)\s?(.*)");

            MakeChatSession();
            BuildCommands();
        }

        private Command SimpleCommand(string n, Implment i)
        {
            return new Command(n, (m) => true, i,
                (m) => Client.SendMessageToChannel(string.Format(DenyMsg, m.Author.Username), m.Channel),
                (m, e) => Console.WriteLine(m.ToString() + Environment.NewLine + e.ToString()));
        }

        private Command OwnerCommand(string n, Implment i)
        {
            return new Command(n, (m) => m.Author.ID == Data.OwnerID, i,
                (m) => Client.SendMessageToChannel(string.Format(DenyMsg, m.Author.Username), m.Channel),
                (m, e) => Console.WriteLine(m.ToString() + Environment.NewLine + e.ToString()));
        }

        private string TrimCmd(DiscordMessage Message)
        {
            return CommandPattern.Match(Message.Content).Groups[2].Value;
        }

        private void BuildCommands()
        {
            Commands = new List<Command> {
                OwnerCommand("Logout", (m) => {
                    Client.SendMessageToChannel("Goodbye!", m.Channel);
                    Client.Logout();
                }),

                OwnerCommand("Game", (m) => {
                    Client.UpdateCurrentGame(TrimCmd(m.Message));
                    Client.SendMessageToChannel("Game Set!", m.Channel);
                }),

                OwnerCommand("Name", (m) => {
                    var info = Client.ClientPrivateInformation.Copy();
                    info.Username = TrimCmd(m.Message);

                    Client.ChangeClientInformation(info);
                    Client.SendMessageToChannel("Name Set!", m.Channel);
                }),

                OwnerCommand("Say", (m) => {
                    Client.SendMessageToChannel(TrimCmd(m.Message), m.Channel);
                }),
            };
        }

        private void Reconnect()
        {
            Reconnecting = true;

            while (Reconnecting)
            {
                Client.Connect();
                Thread.Sleep(10000);
            }
        }

        private void MakeChatSession()
        {
            ChatSession = new ChatterBotFactory().Create(ChatterBotType.CLEVERBOT).CreateSession();
        }

        private void Chat(string message, DiscordChannel channel)
        {
            if (ChatThread != null && ChatThread.IsAlive)
                ChatThread.Join();

            ChatThread = new Thread(() => Client.SendMessageToChannel(ChatSession.Think(message), channel));
            ChatThread.Start();
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

            if (!e.WasClean)
                Reconnect();
        }
 
        private void Client_SocketOpened(object sender, EventArgs e)
        {
            Reconnecting = false;
        }

        private void Client_Connected(object sender, DiscordConnectEventArgs e)
        {
            Console.WriteLine("Connected.");
            Console.WriteLine("Username: " + e.User.Username);
            Console.WriteLine("ID: " + e.User.ID);
        }

        private void Client_MentionReceived(object sender, DiscordMessageEventArgs e)
        {
            Chat(e.MessageText.Replace("@" + Client.Me.ID, "Cleverbot"), e.Channel);
        }

        private void Client_MessageReceived(object sender, DiscordMessageEventArgs e)
        {
            if (e.Author.ID == Client.Me.ID)
                return;

            var extracted = CommandPattern.Match(e.MessageText);

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
