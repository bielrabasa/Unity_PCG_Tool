using System.Collections.Generic;
using UnityEngine;


namespace PCG_Tool
{

    public class TileVariant
    {
        TileInfo tileInfo;
        float weight;

        TileColor Up;
        TileColor Down;
        TileColor Left;
        TileColor Right;
        TileColor Forward;
        TileColor Back;

        public static List<TileVariant> GenerateVariantsFromTileRule(TileRule rule, short tileId)
        {
            //TODO

            return new List<TileVariant>() { new TileVariant() };

            return null;
        }
    }

}