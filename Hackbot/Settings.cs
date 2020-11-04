using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Hackbot
{
    [JsonObject]
    public class Settings
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
