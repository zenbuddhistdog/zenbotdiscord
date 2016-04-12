using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp;

namespace Zen.Zenbot
{
    public delegate bool Predicate(DiscordClient Client, string Message);
    public delegate void Implment(DiscordClient Client, string Message);
    public delegate void Failure(DiscordClient Client, string Message, Exception Error);

    class Command
    {
        private Predicate Pred;
        private Implment Impl;
        private Failure Fail;

        public Command(Predicate Pred, Implment Impl, Failure Fail)
        {
            this.Pred = Pred;
            this.Impl = Impl;
            this.Fail = Fail;
        }

        public void Invoke(DiscordClient Client, string Message)
        {
            try
            {
                if (Pred(Client, Message))
                {
                    Impl(Client, Message);
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
