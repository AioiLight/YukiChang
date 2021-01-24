using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace YukiChang
{
    internal static class Util
    {
        internal static async void Error(SocketMessage arg, string error)
        {
            await arg.Channel.SendMessageAsync(error);
        }

        internal static async Task Save(Settings settings)
        {
            var json = JsonConvert.SerializeObject(settings);
            using (var s = new StreamWriter("settings.json", false, Encoding.UTF8))
            {
                await s.WriteAsync(json);
            }
        }
    }
}
