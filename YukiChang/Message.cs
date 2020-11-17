using System.Collections.Generic;

namespace YukiChang
{
    public class Message
    {
        /// <summary>
        /// タイトル。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// メッセージのUID。
        /// </summary>
        public ulong ID { get; set; }

        public List<Log> Logs { get; set; }    }
}
