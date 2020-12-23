using Discord.WebSocket;
using System;

namespace YukiChang
{
    public class Log
    {
        /// <summary>
        /// ボタンを押したログを取得する。
        /// </summary>
        /// <param name="user">ユーザーUID。</param>
        /// <param name="time">時刻。</param>
        /// <param name="react">リアクションUID。</param>
        public Log(ulong user, ulong time, string react)
        {
            User = user;
            Time = time;
            React = react;
        }

        /// <summary>
        /// リアクションが同じであるかチェックする
        /// </summary>
        /// <param name="log">ログ</param>
        /// <returns>リアクションが同じであるか。</returns>
        public bool SameReact(Log log)
        {
            return log.React == React && log.User == User;
        }

        /// <summary>
        /// UIDから名前を取得する。
        /// </summary>
        /// <returns>ニックネームまたは名前</returns>
        internal string GetName(SocketGuild socketGuild)
        {
            return DiscordUtil.GetName(User, socketGuild);
        }

        /// <summary>
        /// 時刻を取得する。
        /// </summary>
        /// <returns>時刻。</returns>
        internal DateTime GetDateTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds((long)Time).LocalDateTime;
        }

        public ulong User { get; private set; }
        public ulong Time { get; private set; }
        public string React { get; private set; }
    }
}
