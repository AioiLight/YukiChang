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
				var role = server.GetRole(target.UserRole);
				if (target.LogChannel.HasValue
					&& target.Messages.Any(m => m.MessageID == arg3.MessageId)
					&& role.Members.Any(m => m.Id == arg3.UserId))
				{
					var ch = server.GetChannel(target.LogChannel.Value) as ISocketMessageChannel;
					var message = target.Messages.First(m => m.MessageID == arg3.MessageId);
					await ch?.SendMessageAsync($"[{DateTime.Now}] {DiscordUtil.GetName(arg3.UserId, server)} さんが " +
						$"{message.Title} をリアクション {arg3.Emote.Name} を削除しました。");

					// ログから削除
					var log = new Log(arg3.UserId, (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(), arg3.Emote.Name);
					var mes = target.Messages.First(m => m.MessageID == arg3.MessageId);
					mes.Logs.RemoveAll(e => e.SameReact(log));
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
				var role = server.GetRole(target.UserRole);
				if (target.LogChannel.HasValue
					&& target.Messages.Any(m => m.MessageID == arg3.MessageId)
					&& role.Members.Any(m => m.Id == arg3.UserId))
				{
					var ch = server.GetChannel(target.LogChannel.Value) as ISocketMessageChannel;
					var message = target.Messages.First(m => m.MessageID == arg3.MessageId);
					await ch?.SendMessageAsync($"[{DateTime.Now}] {DiscordUtil.GetName(arg3.UserId, server)} さんが " +
						$"{message.Title} にリアクション {arg3.Emote.Name} を付与しました。");

					// ログ取り
					var log = new Log(arg3.UserId, (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(), arg3.Emote.Name);
					var mes = target.Messages.First(m => m.MessageID == arg3.MessageId);
					mes.Logs.Add(log);
				}
			}
			return;
		}

        private async Task Client_MessageReceived(SocketMessage arg)
        {
			var text = arg.Content.Trim();
			if (text.StartsWith(Prefix))
            {
				var line = text.Substring(Prefix.Length).Trim();

				// コマンドチェック
				if (line.Length <= 0)
				{
					// なし
					Util.Error(arg, "コマンドが指定されていません。");
					return;
				}

				// パラメーターで分割
				var cmd = line.Split(' ').First();
				var param = line.Split(' ').Skip(1).ToArray();
				var server = (arg.Channel as SocketGuildChannel).Guild;

                // Typing... 演出
                _ = arg.Channel.TriggerTypingAsync();

				if (cmd == "init" && DiscordUtil.IsAdmin(arg))
                {
					if (param.Length >= 2)
                    {
                        try
                        {
							// 既に初期化済みの場合、登録済みの情報を消去
							Settings.Servers.RemoveAll((sr) => sr.ID == server.Id);

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
							Util.Error(arg, "パラメータの値が不正です。");
						}
                    }
					else
                    {
						Util.Error(arg, "パラメーターが不足しています。");
					}
                }

				// 以下、初期化済みの場合のみ実行可能なコマンド。
				if (!Settings.Servers.Any(s => s.ID == server.Id))
                {
					Util.Error(arg, "サーバーで1度も初期設定を行っていません。");
					return;
                }

				var srv = Settings.Servers.First(s => s.ID == server.Id);

				// 役職で操作を制限する
				if (!DiscordUtil.CanHandle(arg, srv))
                {
					Util.Error(arg, "bot を操作する権限がありません。");
					return;
				}

				if (cmd == "begin")
                {
					// 集計の開始。
					if (param.Length >= 1)
                    {
						var title = string.Join(" ", param);
						if (srv.Messages.Any(mt => mt.Title == title))
                        {
							// 重複チェック
							Util.Error(arg, "タイトルが重複しています。別のタイトルを指定してください。");
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
						Util.Error(arg, "パラメーターが不足しています。");
					}
                }
				else if (cmd == "list")
                {
					await arg.Channel.SendMessageAsync($"{server.Name} の凸管理一覧\n" +
						$"{string.Join("\n", srv.Messages.Select(m => m.Title).ToArray())}");
                }
				else if (cmd == "calc")
                {
					// 集計
					if (param.Length >= 1)
                    {
						var title = string.Join(" ", param);
						try
                        {
                            if (!srv.Messages.Any(mt => mt.Title == title))
                            {
                                Util.Error(arg, "そのメッセージは集計対象ではありません。");
                            }

                            var f = srv.Messages.First(mt => mt.Title == title);
                            var m = await server.GetTextChannel(f.ChannelID).GetMessageAsync(f.MessageID);
                            var role = server.GetRole(srv.UserRole);

                            // 集計
                            var result = await ClanBattleUtil.CalcAttack(m, server.GetRole(srv.UserRole));

                            await arg.Channel.SendMessageAsync(GetHeader(f) + GetCalcMessage(f, role, result));
                        }
                        catch (Exception)
                        {
							Util.Error(arg, "パラメータの値が不正です。");
						}
                    }
					else
                    {
						Util.Error(arg, "パラメーターが不足しています。");
					}
                }
				else if (cmd == "send")
                {
					// 勧告
					if (param.Length >= 1)
					{
						var title = string.Join(" ", param);
						try
                        {
                            if (!srv.Messages.Any(mt => mt.Title == title))
                            {
                                Util.Error(arg, "そのメッセージは集計対象ではありません。");
                            }

                            var f = srv.Messages.First(mt => mt.Title == title);
                            var m = await server.GetTextChannel(f.ChannelID).GetMessageAsync(f.MessageID);
                            var role = server.GetRole(srv.UserRole);

                            // 集計
                            var result = await ClanBattleUtil.CalcAttack(m, server.GetRole(srv.UserRole));

                            await arg.Channel.SendMessageAsync(GetHeader(f) + GetSendMessage(server, f, result));
                        }
                        catch (Exception)
						{
							Util.Error(arg, "パラメータの値が不正です。");
						}
					}
					else
					{
						Util.Error(arg, "パラメーターが不足しています。");
					}
				}
				else if (cmd == "all")
                {
					// 集計＆勧告
					if (param.Length >= 1)
					{
						var title = string.Join(" ", param);
						try
						{
							if (!srv.Messages.Any(mt => mt.Title == title))
							{
								Util.Error(arg, "そのメッセージは集計対象ではありません。");
							}

							var f = srv.Messages.First(mt => mt.Title == title);
							var m = await server.GetTextChannel(f.ChannelID).GetMessageAsync(f.MessageID);
							var role = server.GetRole(srv.UserRole);

							// 集計
							var result = await ClanBattleUtil.CalcAttack(m, server.GetRole(srv.UserRole));

							await arg.Channel.SendMessageAsync($"{GetHeader(f)}{GetCalcMessage(f, role, result)}\n\n{GetSendMessage(server, f, result)}");
						}
						catch (Exception)
						{
							Util.Error(arg, "パラメータの値が不正です。");
						}
					}
					else
					{
						Util.Error(arg, "パラメーターが不足しています。");
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
							Util.Error(arg, "正しくリアクションを流すチャンネルを設定することができませんでした。");
						}
                    }
					else
                    {
						// 解除
						srv.LogChannel = null;
						await arg.Channel.SendMessageAsync($"リアクションログを流すチャンネルを 未設定 にしました。");
					}
                }
				else if (cmd == "csv")
                {
					// CSV出力
					if (param.Length >= 1)
					{
						var title = string.Join(" ", param);
						try
						{
							if (!srv.Messages.Any(mt => mt.Title == title))
							{
								Util.Error(arg, "そのメッセージは集計対象ではありません。");
							}

							// CSV生成
							var f = srv.Messages.First(mt => mt.Title == title);
							var csv = ClanBattleUtil.ToCSV(f.Logs, server, f.Title);
							var name = $"{server.Id}-{f.ChannelID}-{f.MessageID}.csv";
							File.WriteAllText(name, csv, Encoding.UTF8);
							await arg.Channel.SendFileAsync(name, text: $"{f.Title} のログファイル:");
							File.Delete(name);
						}
						catch (Exception)
						{
							Util.Error(arg, "パラメータの値が不正です。");
						}
					}
					else
					{
						Util.Error(arg, "パラメーターが不足しています。");
					}
				}
				else if (cmd == "dispose")
                {
					if (DiscordUtil.IsAdmin(arg))
                    {
						srv.Messages.Clear();
						await arg.Channel.SendMessageAsync($"全てのメッセージを管理対象から除外しました。\n" +
							$"リアクションログの記録を消去しました。");
                    }
					else
                    {
						await arg.Channel.SendMessageAsync($"権限がありません。");
					}
				}
				else
                {
					// なし
					Util.Error(arg, "コマンドが存在しません。");
				}

				// 保存
				await Util.Save(Settings);
			}

			return;
        }

        private static string GetSendMessage(SocketGuild server, Message f, AttackResult result)
        {
            return $"完凸したユーザー:\n{ClanBattleUtil.AttackUser(result, server, 3)}\n" +
				$"残凸のあるユーザー:\n" +
				$"・残り1凸 (3凸+持ち越し)\n{ClanBattleUtil.AttackUser(result, server, 2)}\n" +
				$"・残り2凸 (2凸+持ち越し)\n{ClanBattleUtil.AttackUser(result, server, 1)}\n" +
				$"・残り3凸 (1凸+持ち越し)\n{ClanBattleUtil.AttackUser(result, server, 0)}";
        }

        private static string GetCalcMessage(Message f, SocketRole role, AttackResult result)
        {
            return $"合計凸数: {ClanBattleUtil.CalcPercent(result.Users.Sum(u => u.Attacked), role.Members.Count() * 3)}\n" +
                $"残凸数: {ClanBattleUtil.CalcPercent(result.Users.Sum(u => u.Remain), role.Members.Count() * 3)}\n" +
                $"完凸済者: {ClanBattleUtil.CalcPercent(result.Users.Count(u => u.IsCompleted), role.Members.Count())}\n" +
                $"未完凸済者: {ClanBattleUtil.CalcPercent(result.Users.Count(u => !u.IsCompleted), role.Members.Count())}";
        }

		private static string GetHeader(Message f)
        {
			return $"{f.Title} の凸集計\n" +
				$"集計日時: {DateTime.Now}\n\n";
		}

        private static string Token
		{
			get
			{
				return File.ReadAllText("token.txt", Encoding.UTF8);
			}
		}

		private static string Prefix
        {
			get
            {
				var y = "!yuki";
#if BETA
				return y + "b";
#endif
#if !BETA
				return y;
#endif
			}
        }

		private static Settings Settings { get; set; }
	}
}
