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
        public Log(ulong user, ulong time, uint react)
        {
            User = user;
            Time = time;
            React = react;
        }

        public override bool Equals(object obj)
        {
            if (obj is Log log)
            {
                if (log.User == User
                    && log.Time == Time
                    && log.React == React)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// UIDから名前を取得する。
        /// </summary>
        /// <returns>ニックネームまたは名前</returns>
        internal string GetName(SocketGuild socketGuild)
        {
            var user = socketGuild.GetUser(User);
            return user.Nickname ?? user.Username;
        }

        /// <summary>
        /// 時刻を取得する。
        /// </summary>
        /// <returns>時刻。</returns>
        internal DateTime GetDateTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds((long)Time).DateTime;
        }

        public override int GetHashCode()
        {
            int hashCode = -1793829422;
            hashCode = hashCode * -1521134295 + User.GetHashCode();
            hashCode = hashCode * -1521134295 + Time.GetHashCode();
            hashCode = hashCode * -1521134295 + React.GetHashCode();
            return hashCode;
        }

        public ulong User { get; private set; }
        public ulong Time { get; private set; }
        public uint React { get; private set; }
    }
}
