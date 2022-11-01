using System.Collections.Generic;
using UnityEngine;


namespace Whimsy.Environment
{
    [CreateAssetMenu(fileName = "Generation Info", menuName = "Whimsy/Environment/Generation Info", order = 2)]
    public class GenerationInformation : ScriptableObject
    {
        public int areaAmount = 300;
        public Vector3Int maxAreaSize = new Vector3Int(100, 30, 100);
        public Vector2Int worldChunkSize = new Vector2Int(1000, 1000);

        public BiomeList biomeList;
        public TilePropList tilePropList;
        [Space(20)]
        public GameObject doorWay;
        public GameObject invisibleWall;

        [Tooltip("Only Change With Tile Models Change")]
        public Vector3Int tileSize = new Vector3Int(2, 1, 2);
        [Space (20)]
        public bool randomlyGenerated = true;
        public int seed = -1;

        public int maxCorridorLength = 2;
        
    }
}