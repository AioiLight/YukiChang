using Discord.WebSocket;
using System.Linq;

namespace YukiChang
{
    internal static class DiscordUtil
    {
        /// <summary>
        /// 管理者かどうかを取得する。
        /// </summary>
        /// <param name="message">メッセージ。</param>
        /// <returns>管理者であるかどうか。</returns>
        internal static bool IsAdmin(SocketMessage message)
        {
            return (message.Author as SocketGuildUser).GuildPermissions.Administrator;
        }

        /// <summary>
        /// YukiChang を操作できるかどうかを取得する。
        /// </summary>
        /// <param name="message">メッセージ。</param>
        /// <param name="server">サーバ。</param>
        /// <returns>操作できるかどうか。</returns>
        internal static bool CanHandle(SocketMessage message, Server server)
        {
            return IsAdmin(message) || (message.Author as SocketGuildUser).Roles.Any(r => r.Id == server.AdminRole);
        }
    }
}
