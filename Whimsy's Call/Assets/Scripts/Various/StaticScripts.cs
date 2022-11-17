using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whimsy
{
    public static class StaticScripts
    {
        //Delete All Children Of An Object
        public static void DeleteChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        public static ObjectReferences objRef;
    }
}

