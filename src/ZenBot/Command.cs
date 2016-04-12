using DiscordSharp;
using DiscordSharp.Events;
using System;

namespace Zen.Zenbot
{
    public delegate bool Predicate(DiscordClient Client, DiscordMessageEventArgs Message);
    public delegate void Implment(DiscordClient Client, DiscordMessageEventArgs Message);
    public delegate void Failure(DiscordClient Client, DiscordMessageEventArgs Message, Exception Error);

    class Command
    {
        public string Name { get; private set; }
        private Predicate Pred;
        private Implment Impl;
        private Implment Deny;
        private Failure Fail;

        public Command(string Name, Predicate Pred, Implment Impl, Implment Deny, Failure Fail)
        {
            this.Name = Name;
            this.Pred = Pred;
            this.Impl = Impl;
            this.Deny = Deny;
            this.Fail = Fail;
        }

        public void Invoke(DiscordClient Client, DiscordMessageEventArgs Message)
        {
            try
            {
                if (Pred(Client, Message))
                {
                    Impl(Client, Message);
                }
                else
                {
                    Deny(Client, Message);
                }
            }
            catch (Exception e)
            {
                try
                {
                    Fail(Client, Message, e);
                }
                catch { /* Maybe we should handle this case some time. */ };
            }
        }
    }
}
