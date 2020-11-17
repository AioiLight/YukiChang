using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace YukiChang
{
    class Program
    {
		private DiscordSocketClient Client;
		static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
			Client = new DiscordSocketClient(_config);

			await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("YukiChang"));
			await Client.StartAsync();

			Client.Ready += () =>
			{
				Console.WriteLine("Bot is connected!");
				return Task.CompletedTask;
			};
			
			await Task.Delay(-1);
		}
	}
}
