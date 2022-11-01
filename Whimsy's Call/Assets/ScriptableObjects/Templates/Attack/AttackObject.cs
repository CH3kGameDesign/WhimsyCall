using System.Collections.Generic;
using UnityEngine;

namespace Whimsy.Creatures
{
    [CreateAssetMenu(fileName = "New Attack Object", menuName = "Whimsy/Attack/Object", order = 2)]
    public class AttackObject : ScriptableObject
    {
        #region Public Variables
        [Header("Identifying Information")]
        public string name;
        public int id;
        public string description;
        [Header("Attack Information")]

        public _attackObject attack1;
        public _attackObject attack2;
        public float cooldown;
        #endregion

        #region Classes
        [System.Serializable]
        public class _attackObject
        {
            public GameObject prefab;
            public float damage;
            [Space(10)]
            public float burnChance = 0f;
            public float poisonChance = 0f;
            public float freezeChance = 0f;
        }
        #endregion

        #region Enums

        #endregion
    }
}