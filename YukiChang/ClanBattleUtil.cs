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
        internal static string CalcPercent(int a, int b)
        {
            return $"{a}/{b} ({1.0 * a / b:##.##%})";
        }

        internal static async Task<AttackResult> CalcAttack(IMessage m, SocketRole targetRole)
        {
            var result = new AttackResult();
            targetRole.Members.ToList().ForEach(mr => result.Users.Add(new AttackUser(mr.Id)));
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

        internal static string AttackUser(AttackResult result, SocketGuild socketGuild, int target)
        {
            var l = result.Users.Where(u => u.Attacked == target).ToList();
            var text = "";
            foreach (var item in l)
            {
                var name = DiscordUtil.GetName(item.UserID, socketGuild);
                text += $"{name} さん\n";
            }
            return text;
        }

        /// <summary>
        /// CSV出力する。
        /// </summary>
        /// <param name="result"></param>
        /// <returns>CSVフォーマットのString。</returns>
        internal static string ToCSV(IReadOnlyList<Log> logs, SocketGuild socketGuild)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\"時刻\",\"プレイヤー\",\"リアクション\"");
            foreach (var item in logs)
            {
                sb.AppendLine($"\"{item.GetDateTime()}\",\"{item.GetName(socketGuild)}\",\"{item.React}\"");
            }
            return sb.ToString();
        }
    }
}
