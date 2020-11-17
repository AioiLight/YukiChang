using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace YukiChang
{
    class Program
    {
		private DiscordSocketClient Client;
		static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			Settings = File.Exists("settings.json")
                ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json", Encoding.UTF8))
                : new Settings();

            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
			Client = new DiscordSocketClient(_config);

			await Client.LoginAsync(TokenType.Bot, Token);
			await Client.StartAsync();

			Client.Ready += () =>
			{
				Console.WriteLine("Bot is connected!");
				return Task.CompletedTask;
			};

            Client.MessageReceived += Client_MessageReceived;
			
			await Task.Delay(-1);
		}

        private Task Client_MessageReceived(SocketMessage arg)
        {
			var text = arg.Content.Trim();
			if (text.StartsWith("!yuki"))
            {
				arg.Channel.SendMessageAsync("pong!");
            }

			return Task.CompletedTask;
        }

        private static string Token
		{
			get
			{
				return File.ReadAllText("token.txt", Encoding.UTF8);
			}
		}

		private static Settings Settings { get; set; }
	}
}
