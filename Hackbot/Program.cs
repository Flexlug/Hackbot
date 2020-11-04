using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hackbot
{
    class Program
    {
        static void Main(string[] args)
            => Task.Run(AsyncMain).GetAwaiter().GetResult();

        public static async Task AsyncMain()
        {
            Settings botSettings;

            using (StreamReader sw = new StreamReader("config.json"))
                botSettings = JsonConvert.DeserializeObject<Settings>(sw.ReadToEnd());

            Bot bot = new Bot(botSettings);
            await bot.Start();

            await Task.Delay(-1);
        }
    }
}
