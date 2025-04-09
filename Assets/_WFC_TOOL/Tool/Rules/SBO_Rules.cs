using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{
    [CreateAssetMenu(fileName = "New Rules", menuName = "WFC_Tool/Rules")]
    public class SBO_Rules : ScriptableObject
    {
        public SBO_TileSet tileSet;

        public TileRule[] tileRules;

        public TileColor[] colorTable = new TileColor[16]; //Color compatibility
    }

}