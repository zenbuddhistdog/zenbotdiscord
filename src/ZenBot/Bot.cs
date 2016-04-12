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
        private static string AuthorID = "some id or something";
        private static string DenyMsg = "I can't do that, {0}.";

        private const string CommandPrefix = @"&";
        private static Regex CommandPattern = new Regex(@"^" + CommandPrefix + @"([^\s]+)\s?(.*)");

        private static List<Command> Commands = new List<Command>
        {
            new Command("Logout",
                (c, m) => m.author.ID == AuthorID,
                (c, m) => {
                    c.SendMessageToChannel("Goodbye!", m.Channel);
                    c.Logout();
                },
                (c, m) => c.SendMessageToChannel(string.Format(DenyMsg, m.author), m.Channel),
                (c, m, e) => Console.WriteLine(m.ToString() + Environment.NewLine + e.ToString())
            ),
        };

        private DiscordClient Client;

        public Bot(DiscordClient Client)
        {
            this.Client = Client;
            Client.MessageReceived += Client_MessageReceived;
            Client.MentionReceived += Client_MentionReceived;
            Client.Connected += Client_Connected;
        }

        private void Client_Connected(object sender, DiscordConnectEventArgs e)
        {
            var result = Client.SendLoginRequest(); // detect failed logins maybe.
        }

        private void Client_MentionReceived(object sender, DiscordMessageEventArgs e)
        {
            // cleverbot like stupidity here.
            throw new NotImplementedException();
        }

        private void Client_MessageReceived(object sender, DiscordMessageEventArgs e)
        {
            if (e.author.ID == Client.Me.ID)
                return;

            var extracted = CommandPattern.Match(e.message.Content);

            if (!extracted.Success)
                return;

            var cmdName = extracted.Groups[1].Value.ToUpper();
            Commands.Where(c => c.Name.ToUpper() == cmdName).Single().Invoke(Client, e);
        }
    }
}
