using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whimsy.Environment
{
    [CreateAssetMenu(fileName = "Tile Prop List", menuName = "Whimsy/Environment/Tile Prop List", order = 5)]
    public class TilePropList : ScriptableObject
    {
        public List<GroundTile> Tiles = new List<GroundTile>();
        [Space(10)]
        public List<Prop> Props = new List<Prop>();
    }

    #region Classes
    #endregion

    #region Enums
    #endregion
}
