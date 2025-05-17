using System.Collections.Generic;
using UnityEngine;

namespace PCG_Tool
{

    [CreateAssetMenu(fileName = "New TileSet", menuName = "WFC_Tool/TileSet")]
    public class SBO_TileSet : ScriptableObject
    {
        public Vector3 tileSize = Vector3.one;
        public bool allowAir = true;
        public List<GameObject> tiles;

        public GameObject GetPrefab(short id)
        {
            if(id < 0 || id >= tiles.Count)
            {
                Debug.LogWarning("TileSet: ID " + id + " out of range.");
                return null;
            }

            return tiles[id];
        }

        public int GetTileCount()
        {
            return tiles.Count;
        }
    }

}
