using System.Collections.Generic;

namespace YukiChang
{
    public class Message
    {
        public Message()
        {
            // Init
            Logs = new List<Log>();
            LastAttacks = new Dictionary<ulong, int>();
        }

        /// <summary>
        /// ラストアタックをしたことにする。
        /// </summary>
        /// <param name="uid">DiscordのUID。</param>
        public void AddLastAttack(ulong uid)
        {
            if (!LastAttacks.ContainsKey(uid))
            {
                // 新規追加
                LastAttacks.Add(uid, 1);
            }
            else
            {
                // 既存のに加算。
                if (LastAttacks[uid] < 3)
                {
                    LastAttacks[uid]++;
                }
            }
        }

        /// <summary>
        /// タイトル。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// チャンネルのUID。
        /// </summary>
        public ulong ChannelID { get; set; }

        /// <summary>
        /// メッセージのUID。
        /// </summary>
        public ulong MessageID { get; set; }

        public List<Log> Logs { get; set; }

        /// <summary>
        /// ラストアタック集計用。
        /// </summary>
        public Dictionary<ulong, int> LastAttacks { get; set; }
    }
}
