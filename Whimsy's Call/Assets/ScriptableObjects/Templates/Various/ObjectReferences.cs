using System.Collections.Generic;
using UnityEngine;

namespace Whimsy
{
    [CreateAssetMenu(fileName = "New Reference Object", menuName = "Whimsy/Misc/ReferenceObject", order = 4)]
    public class ObjectReferences : ScriptableObject
    {
        #region Public Variables
        public _DamageCounter damageCounter;
        #endregion

        #region Classes
        [System.Serializable]
        public class _DamageCounter
        {
            public GameObject prefab;
            public float upForce = 5;
            public float randomForce = 1;
        }

        #endregion

        #region Enums

        #endregion
    }
}