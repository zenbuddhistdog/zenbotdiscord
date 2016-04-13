using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Zen.Zenbot
{
    public class BotData
    {
        public string Username;
        public string Secret;
        public string Token;
        public string BotID;
        public string ClientID;
        public string OwnerID;
        public string Prefix;

        public static IEnumerable<BotData> Load(string settingsFileName)
        {
            var root = XElement.Load(settingsFileName);

            return root.Elements("Bot").Select(el => new BotData
            {
                Username = el.Element("Username").Value,
                Secret = el.Element("Secret").Value,
                Token = el.Element("Token").Value,
                BotID = el.Element("BotID").Value,
                ClientID = el.Element("ClientID").Value,
                OwnerID = el.Element("OwnerID").Value,
                Prefix = el.Element("Prefix").Value,
            });
        }
    }
}
