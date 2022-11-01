using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whimsy.Creatures
{
    [CreateAssetMenu(fileName = "Attack List", menuName = "Whimsy/Attack/List", order = 1)]
    public class AttackList : ScriptableObject
    {
        public List<AttackObject> attackList = new List<AttackObject>();
    }
}

