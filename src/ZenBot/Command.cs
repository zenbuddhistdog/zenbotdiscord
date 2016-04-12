using DiscordSharp;
using DiscordSharp.Events;
using System;

namespace Zen.Zenbot
{
    public delegate bool Predicate(DiscordMessageEventArgs Message);
    public delegate void Implment(DiscordMessageEventArgs Message);
    public delegate void Failure(DiscordMessageEventArgs Message, Exception Error);

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

        public void Invoke(DiscordMessageEventArgs MessageEvent)
        {
            try
            {
                if (Pred(MessageEvent))
                {
                    Impl(MessageEvent);
                }
                else
                {
                    Deny(MessageEvent);
                }
            }
            catch (Exception e)
            {
                try
                {
                    Fail(MessageEvent, e);
                }
                catch { /* Maybe we should handle this case some time. */ };
            }
        }
    }
}
