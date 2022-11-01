using System.Collections.Generic;
using UnityEngine;


namespace Whimsy.Environment
{
    [CreateAssetMenu(fileName = "New Biome", menuName = "Whimsy/Environment/Biome", order = 3)]
    public class Biome : ScriptableObject
    {
        [Header("Terrain Shape")]
        public Vector2Int waterLevel = new Vector2Int(1, 4);

        public Vector2Int sizeRequirements = new Vector2Int(1, 20);

        [Header("Tiles & Props")]
        public List<GroundTile> availableGroundTiles = new List<GroundTile>();
        public List<Prop> availableProps = new List<Prop>();

        [Header("Likelihood")]
        public float baseLikelihood = 1;
    }
}