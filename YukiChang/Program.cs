using System;
using System.IO;
using System.Linq;
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

            var _config = new DiscordSocketConfig { MessageCacheSize = 100, AlwaysDownloadUsers = true };
			Client = new DiscordSocketClient(_config);

			await Client.LoginAsync(TokenType.Bot, Token);
			await Client.StartAsync();

			Client.Ready += () =>
			{
				Console.WriteLine("Bot is connected!");
				return Task.CompletedTask;
			};

            Client.MessageReceived += Client_MessageReceived;
            Client.ReactionAdded += Client_ReactionAdded;
            Client.ReactionRemoved += Client_ReactionRemoved;
			
			await Task.Delay(-1);
		}

        private async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
			var server = (arg2 as SocketGuildChannel).Guild;
			if (Settings.Servers.Any(s => s.ID == server.Id))
			{
				var target = Settings.Servers.First(s => s.ID == server.Id);
				if (target.LogChannel.HasValue && target.Messages.Any(m => m.MessageID == arg3.MessageId))
				{
					var ch = server.GetChannel(target.LogChannel.Value) as ISocketMessageChannel;
					var message = target.Messages.First(m => m.MessageID == arg3.MessageId);
					await ch?.SendMessageAsync($"[{DateTime.Now}] {arg3.User.Value.Username} さんが " +
						$"{message.Title} をリアクション {arg3.Emote.Name} を削除しました。");
				}
			}
			return;
		}

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
			var server = (arg2 as SocketGuildChannel).Guild;
			if (Settings.Servers.Any(s => s.ID == server.Id))
            {
				var target = Settings.Servers.First(s => s.ID == server.Id);
				if (target.LogChannel.HasValue && target.Messages.Any(m => m.MessageID == arg3.MessageId))
                {
					var ch = server.GetChannel(target.LogChannel.Value) as ISocketMessageChannel;
					var message = target.Messages.First(m => m.MessageID == arg3.MessageId);
					await ch?.SendMessageAsync($"[{DateTime.Now}] {arg3.User.Value.Username} さんが " +
						$"{message.Title} にリアクション {arg3.Emote.Name} を付与しました。");
				}
            }
			return;
		}

        private async Task Client_MessageReceived(SocketMessage arg)
        {
			var text = arg.Content.Trim();
			if (text.StartsWith("!yuki"))
            {
				var line = text.Substring("!yuki ".Length);

				// コマンドチェック
				if (line.Length <= 0)
                {
					// なし
					Error(arg, "コマンドが指定されていません。");
					return;
                }

				// パラメーターで分割
				var cmd = line.Split(" ").First();
				var param = line.Split(" ").Skip(1).ToArray();
				var server = (arg.Channel as SocketGuildChannel).Guild;

				if (cmd == "init" && IsAdmin(arg))
                {
					if (param.Length >= 2)
                    {
                        try
                        {
							// 既に初期化済みの場合、登録済みの情報を消去
							Settings.Servers.RemoveAll((s) => s.ID == server.Id);

							// 追加
							var s = new Server()
							{
								ID = server.Id,
								AdminRole = ulong.Parse(param[0]),
								UserRole = ulong.Parse(param[1])
							};
							Settings.Servers.Add(s);

							await arg.Channel.SendMessageAsync($"サーバー {server.Name} の初期設定が完了しました。\n" +
								$"管理者役職: {server.GetRole(ulong.Parse(param[0])).Name}\n" +
								$"集計対象役職: {server.GetRole(ulong.Parse(param[1])).Name}");
                        }
                        catch (Exception)
                        {
							Error(arg, "パラメータの値が不正です。");
						}
                    }
					else
                    {
						Error(arg, "パラメーターが不足しています。");
					}
                }

				// 以下、初期化済みの場合のみ実行可能なコマンド。
				if (!Settings.Servers.Any(s => s.ID == server.Id))
                {
					Error(arg, "サーバーで1度も初期設定を行っていません。");
					return;
                }

				var srv = Settings.Servers.First(s => s.ID == server.Id);

				// 役職で操作を制限する
				if (!CanHandle(arg, srv))
                {
					Error(arg, "bot を操作する権限がありません。");
					return;
				}

				if (cmd == "begin")
                {
					// 集計の開始。
					if (param.Length >= 1)
                    {
						var title = string.Join(" ", param);
						if (srv.Messages.Any(m => m.Title == title))
                        {
							// 重複チェック
							Error(arg, "タイトルが重複しています。別のタイトルを指定してください。");
							return;
                        }

						var m = await arg.Channel.SendMessageAsync($"凸集計: {title}\n" +
							$"本戦に挑戦し、凸が完了したらボタンを押して進捗を記録します。\n");

						await m.AddReactionAsync(new Emoji("1️⃣"));
						await m.AddReactionAsync(new Emoji("2️⃣"));
						await m.AddReactionAsync(new Emoji("3️⃣"));

						srv.Messages.Add(new Message() { MessageID = m.Id, ChannelID = m.Channel.Id, Title = title });
					}
					else
                    {
						Error(arg, "パラメーターが不足しています。");
					}
                }
				else if (cmd == "calc")
                {
					// 集計
					if (param.Length >= 1)
                    {
						var title = string.Join(" ", param);
						try
                        {
							if (!srv.Messages.Any(m => m.Title == title))
                            {
								Error(arg, "そのメッセージは集計対象ではありません。");
							}

							var f = srv.Messages.First(m => m.Title == title);
							var m = await server.GetTextChannel(f.ChannelID).GetMessageAsync(f.MessageID);
							var role = server.GetRole(srv.UserRole);

							// 集計
							var result = await CalcAttack(m, server.GetRole(srv.UserRole));

							await arg.Channel.SendMessageAsync($"{f.Title} の凸集計\n" +
								$"集計日時: {DateTime.Now}\n" +
								$"合計凸数: {CalcPercent(result.Users.Sum(u => u.Attacked), role.Members.Count() * 3)}\n" +
								$"残凸数: {CalcPercent(result.Users.Sum(u => u.Remain), role.Members.Count() * 3)}\n" +
								$"完凸済者: {CalcPercent(result.Users.Count(u => u.IsCompleted), role.Members.Count())}\n" +
								$"未完凸済者: {CalcPercent(result.Users.Count(u => !u.IsCompleted), role.Members.Count())}");
						}
						catch (Exception)
                        {
							Error(arg, "パラメータの値が不正です。");
						}
                    }
					else
                    {
						Error(arg, "パラメーターが不足しています。");
					}
                }
				else if (cmd == "log")
                {
					// ログ設定
					if (param.Length >= 1)
                    {
						// 設定
						try
                        {
							var ch = server.GetTextChannel(ulong.Parse(param[0]));
							srv.LogChannel = ch.Id;
							await arg.Channel.SendMessageAsync($"リアクションログを流すチャンネルを {ch.Name} にしました。");
                        }
						catch (Exception)
                        {
							Error(arg, "正しくリアクションを流すチャンネルを設定することができませんでした。");
						}
                    }
					else
                    {
						// 解除
						srv.LogChannel = null;
						await arg.Channel.SendMessageAsync($"リアクションログを流すチャンネルを 未設定 にしました。");
					}
                }
				else if (cmd == "dispose")
                {
					srv.Messages.Clear();
					await arg.Channel.SendMessageAsync($"全てのメッセージを管理対象から除外しました。\n" +
						$"リアクションログの記録を消去しました。");
				}

				// 保存
				await Save();
			}

			return;
        }

		private static string CalcPercent(int a, int b)
        {
			return $"{a}/{b} ({1.0 * a / b:##.##%})";
        }

		private static async Task<AttackResult> CalcAttack(IMessage m, SocketRole targetRole)
        {
			var result = new AttackResult();
			var reacts = new Emoji[] { new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣") };
			for (var i = 0; i < reacts.Length; i++)
			{
				var reactions = await m.GetReactionUsersAsync(reacts[i], 100).FlattenAsync();

				foreach (var item in reactions)
				{
					if (targetRole.Members.Any(e => e.Id == item.Id))
					{
						result.Attack(item.Id);
					}
				}
			}
			return result;
		}

		private static async void Error(SocketMessage arg, string error)
        {
			await arg.Channel.SendMessageAsync(error + "``!yuki help``でヘルプを表示");
        }

		private static bool IsAdmin(SocketMessage message)
		{
			return (message.Author as SocketGuildUser).GuildPermissions.Administrator;
		}

		private static bool CanHandle(SocketMessage message, Server server)
        {
			return IsAdmin(message) || (message.Author as SocketGuildUser).Roles.Any(r => r.Id == server.AdminRole);
		}

		private static async Task Save()
        {
			var json = JsonConvert.SerializeObject(Settings);
			await File.WriteAllTextAsync("settings.json", json, Encoding.UTF8);
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
