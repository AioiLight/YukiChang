using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukiChang
{
    /// <summary>
    /// プリンセスコネクト！Re:Dive のプレイヤー情報を格納するクラス。
    /// </summary>
    public class Player
    {
        public Player(ulong uid)
        {
            UID = uid;
        }

        /// <summary>
        /// ボスを討伐した。
        /// </summary>
        internal void Kill()
        {
            if (LastAttackCount < 3)
            {
                LastAttackCount++;
            }
        }

        /// <summary>
        /// ラストアタックを消費した。
        /// </summary>
        internal void ConsumeLastAttack()
        {
            if (LastAttackRemain > 0)
            {
                DoneLastAttackCount++;
            }
        }

        /// <summary>
        /// UID。
        /// </summary>
        public ulong UID { get; private set; }
        /// <summary>
        /// 持越しを持った回数。
        /// </summary>
        public uint LastAttackCount { get; private set; }
        /// <summary>
        /// 持越しを消費した回数。
        /// </summary>
        public uint DoneLastAttackCount { get; private set; }
        /// <summary>
        /// 残りの持越し数。
        /// </summary>
        public uint LastAttackRemain
        {
            get
            {
                return LastAttackCount - DoneLastAttackCount;
            }
        }
    }
}
