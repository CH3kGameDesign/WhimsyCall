using System.Collections.Generic;
using UnityEngine;


namespace Whimsy.Environment
{
    [CreateAssetMenu(fileName = "EnvironmentObjects", menuName = "Whimsy/Environment/Ground Tile", order = 2)]
    public class GroundTile : ScriptableObject
    {
        public GameObject palette;
        public Vector2Int requiredSeaLevel = new Vector2Int(-20, 20);
    }
}