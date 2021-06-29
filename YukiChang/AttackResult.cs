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
                var u = Users.First(uf => uf.UserID == uid);
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

        public AttackUser SetLastAttack(ulong uid, int index, string emoji)
        {
            if (Users.Any(u => u.UserID == uid))
            {
                var u = Users.First(uf => uf.UserID == uid);
                u.AddLastAttack(index, emoji);
                return u;
            }
            else
            {
                Users.Add(new AttackUser(uid));
                var u = Users.Last();
                u.AddLastAttack(index, emoji);
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
        /// ラストアタックする。
        /// </summary>
        public void AddLastAttack(int index, string emoji)
        {
            RemainLastAttack[index] = emoji;
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
        /// 現在の持越し保持数。
        /// </summary>
        public string[] RemainLastAttack { get; private set; } = new string[3];

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
        /// ラストアタックの数。
        /// </summary>
        public int RemainLastAttackCount
        {
            get
            {
                return RemainLastAttack.Where(x => !string.IsNullOrEmpty(x)).Count();
            }
        }

        /// <summary>
        /// ユーザーID。
        /// </summary>
        public ulong UserID { get; private set; }
    }
}
