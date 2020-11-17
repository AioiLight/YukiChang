using System;
using System.Collections.Generic;
using System.Text;

namespace YukiChang
{
    public class Server
    {
        /// <summary>
        /// サーバーのGuild ID。
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>
        /// ボット操作者の役職。
        /// </summary>
        public ulong AdminRole { get; set; }

        /// <summary>
        /// 集計対象の役職。
        /// </summary>
        public ulong UserRole { get; set; }

        /// <summary>
        /// ログを流すチャンネル。
        /// </summary>
        public ulong LogChannel { get; set; }

        /// <summary>
        /// 集計メッセージ。
        /// </summary>
        public List<Message> Messages { get; set; }
    }
}
