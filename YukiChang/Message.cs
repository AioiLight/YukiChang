using System.Collections.Generic;

namespace YukiChang
{
    public class Message
    {
        public Message()
        {
            // Init
            Logs = new List<Log>();
            LastAttacks = new Dictionary<ulong, Player>();
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
                LastAttacks.Add(uid, new Player());
                LastAttacks[uid].Kill();
            }
            else
            {
                // 既存のに加算。
                LastAttacks[uid].Kill();
            }
        }

        /// <summary>
        /// ラストアタックを消費したことにする。
        /// </summary>
        /// <param name="uid">DiscordのUID。</param>
        public void ConsumeLastAttack(ulong uid)
        {
            if (LastAttacks.ContainsKey(uid))
            {
                // ラストアタックを消費
                LastAttacks[uid].ConsumeLastAttack();
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
        public Dictionary<ulong, Player> LastAttacks { get; set; }
    }
}
