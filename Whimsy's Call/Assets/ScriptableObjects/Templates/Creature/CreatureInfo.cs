using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whimsy.Creatures;

namespace Whimsy.Creatures
{
    [CreateAssetMenu(fileName = "New Creature", menuName = "Whimsy/Creature/Creature Info", order = 2)]
    public class CreatureInfo : ScriptableObject
    {
        #region Public Variables
        [Header("Identifying Information")]
        public string name;
        public int id;
        public string description;
        [Header("Art Information")]
        public GameObject model;
        [Header("Attack Information")]
        public List<_attackInfo> availableAttacks = new List<_attackInfo>();
        [Header("Evolution Information")]
        public CreatureObject evolution;
        public _evoRequirements evoRequirements;
        public _stats lvl1Stats;
        public _stats lvlIncreaseStats;
        #endregion
    }

}
