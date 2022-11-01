using System.Collections.Generic;
using UnityEngine;


namespace Whimsy.Creatures
{
    public class CreatureObject : ScriptableObject
    {
        #region Public Variables
        [Header("Identifying Information")]
        public string nickname;
        public int id;
        [Header("Attack Information")]
        public List<_attackInfo> attacks = new List<_attackInfo>();
        public int level;
        public _stats currentStats;
        public _stats baseStats;
        #endregion
    }
}