using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukiChang
{
    internal static class ClanBattleUtil
    {
        /// <summary>
        /// メッセージのタイトルに近いMessageを取得する。
        /// もし複数見つかった場合は、最後にbeginされた凸集計が選ばれる。
        /// 存在しないとnullを返す。
        /// </summary>
        /// <param name="messages">Message[]。</param>
        /// <param name="title">メッセージのタイトル。</param>
        /// <returns>見つかったMessage。存在しない場合、nullを返す。</returns>
        internal static Message GetProperMessage(Message[] messages, string title)
        {
            var result = messages.LastOrDefault(m => m.Title.StartsWith(title));

            return result;
        }

        internal static string CalcPercent(int a, int b)
        {
            return $"{a}/{b} ({1.0 * a / b:##.##%})";
        }

        internal static async Task<AttackResult> CalcAttack(IMessage m, SocketRole targetRole)
        {
            var result = new AttackResult();
            targetRole.Members.ToList().ForEach(mr => result.Users.Add(new AttackUser(mr.Id)));

            var la = await m.GetReactionUsersAsync(new Emoji("☠️"), 100).FlattenAsync();

            var reacts = new Emoji[] { new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣") };
            for (var i = 0; i < reacts.Length; i++)
            {
                var reactions = await m.GetReactionUsersAsync(reacts[i], 100).FlattenAsync();

                foreach (var item in reactions)
                {
                    if (targetRole.Members.Any(e => e.Id == item.Id))
                    {
                        result.Attack(item.Id, false);
                    }
                }
            }

            foreach (var item in la)
            {
                if (targetRole.Members.Any(e => e.Id == item.Id))
                {
                    result.Attack(item.Id, true);
                }
            }

            return result;
        }

        internal static string AttackUser(AttackResult result, SocketGuild socketGuild, int target)
        {
            var l = result.Users.Where(u => u.Attacked == target).ToList();
            var text = "";
            foreach (var item in l)
            {
                var name = DiscordUtil.GetName(item.UserID, socketGuild);
                if (item.LastAttack)
                {
                    text += $"⚠️ ";
                }
                text += $"{name} さん";
                text += $"\n";
            }
            return text;
        }

        /// <summary>
        /// CSV出力する。
        /// </summary>
        /// <param name="result"></param>
        /// <returns>CSVフォーマットのString。</returns>
        internal static string ToCSV(IReadOnlyList<Log> logs, SocketGuild socketGuild, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\"時刻\",\"タイトル\",\"プレイヤー\",\"リアクション\"");
            foreach (var item in logs)
            {
                sb.AppendLine($"\"{item.GetDateTime()}\",\"{title}\",\"{item.GetName(socketGuild)}\",\"{item.React}\"");
            }
            return sb.ToString();
        }
    }
}
