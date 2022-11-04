using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whimsy.Creatures
{
    [CreateAssetMenu(fileName = "Creature List", menuName = "Whimsy/Creature/List", order = 1)]
    public class CreatureList : ScriptableObject
    {
        public List<CreatureInfo> creatureList = new List<CreatureInfo>();
    }

    #region Classes
    [System.Serializable]
    public class _evoRequirements
    {
        public int reqlevel = 0;
        public daySection reqTime = daySection.anyTime;
    }
    [System.Serializable]
    public class _attackInfo
    {
        public AttackObject attack;
        public int levelReq = 0;
    }

    [System.Serializable]
    public class _stats
    {
        public int health;  //Health Before Blackout
        public int attack;  //Outgoing Damage Increase
        public int defense; //Incoming Damage Reduction
        public int power;   //Attack Size
    }
    #endregion

    #region Enums
    public enum daySection { anyTime, dayTime, nightTime };
    #endregion
}

