using System.Collections.Generic;
using UnityEngine;


namespace Whimsy.Environment
{
    [CreateAssetMenu(fileName = "Biome List", menuName = "Whimsy/Environment/Biome List", order = 4)]
    public class BiomeList : ScriptableObject
    {
        public List<Biome> Biomes = new List<Biome>();
        [HideInInspector]
        public float totalLikelihoodPercent;
    }

    #region Classes
    #endregion

    #region Enums
    #endregion
}