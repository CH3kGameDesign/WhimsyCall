using System.Collections.Generic;
using UnityEngine;


namespace Whimsy.Environment
{
    [CreateAssetMenu(fileName = "New Prop", menuName = "Whimsy/Environment/Prop", order = 1)]
    public class Prop : ScriptableObject
    {
        public GameObject model;
    }
}