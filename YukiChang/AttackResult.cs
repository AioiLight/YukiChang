using System.Collections.Generic;
using System.Linq;

namespace YukiChang
{
    public class AttackResult
    {
        public readonly List<AttackUser> Users = new List<AttackUser>();
        public AttackUser Attack(ulong uid)
        {
            if (Users.Any(u => u.UserID == uid))
            {
                var u = Users.First(u => u.UserID == uid);
                u.Attack();
                return u;
            }
            else
            {
                Users.Add(new AttackUser(uid));
                var u = Users.Last();
                u.Attack();
                return u;
            }
        }
    }

    public class AttackUser
    {
        public AttackUser(ulong uid)
        {
            UserID = uid;
        }

        /// <summary>
        /// 凸する。
        /// </summary>
        public void Attack()
        {
            if (Remain > 0)
            {
                Attacked++;
            }
        }

        /// <summary>
        /// 残り凸。
        /// </summary>
        public int Remain
        {
            get
            {
                return 3 - Attacked;
            }
        }

        /// <summary>
        /// 凸数。
        /// </summary>
        public int Attacked { get; private set; } = 0;

        /// <summary>
        /// 完凸したか？
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return Remain == 0;
            }
        }

        /// <summary>
        /// ユーザーID。
        /// </summary>
        public ulong UserID { get; private set; }
    }
}
