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

        public List<_attackObject> attacks = new List<_attackObject>();
        [Space(10)]
        [Tooltip("How much faster should Player/Creature move while active")]
        public float moveMultiplier = 1f;
        public float rotMultiplier = 1f;
        public float multiplierDuration = 1f;
        [Space(10)]
        public float cooldown;
        #endregion

        #region Classes
        [System.Serializable]
        public class _attackObject
        {
            public GameObject prefab;
            public float damage;
            public bool breakOnHit;
            public bool moveWithPlayer = true;
            
            public float lifetime = 1;
            [Space(10)]
            public float burnChance = 0f;
            public float poisonChance = 0f;
            public float freezeChance = 0f;
            [Space(10)]
            public bool hitEnemies = true;
            public bool hitFriendlies = false;
            public bool hitSelf = false;
            [HideInInspector]
            public bool hitPlayer = false;
            [HideInInspector]
            public bool hitPlayerCreatures = false;
            [HideInInspector]
            public bool hitNonPlayerCreatures = true;
            [HideInInspector]
            public GameObject self;
        }
        #endregion

        #region Enums

        #endregion
    }
}