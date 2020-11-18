using System.Collections.Generic;

namespace YukiChang
{
    public class Message
    {
        public Message()
        {
            Logs = new List<Log>();
        }

        /// <summary>
        /// タイトル。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// メッセージのUID。
        /// </summary>
        public ulong ID { get; set; }

        public List<Log> Logs { get; set; }
    }
}
