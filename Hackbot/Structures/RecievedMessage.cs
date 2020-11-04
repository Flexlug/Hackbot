using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace Hackbot.Structures
{
    public class RecievedMessage
    {
        public string InlineData { get; set; }
        public string Text { get; set; }
        public User From { get; set; }
        public Chat Chat { get; set; }
    }
}
