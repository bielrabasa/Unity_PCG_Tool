using UnityEngine;
using System.Collections.Generic;

namespace PCG_Tool
{

    public class GridCell
    {
        public Vector3Int coords;
        private List<TileVariant> possibleVariants;
        public bool collapsed { get; private set; }
        public TileVariant chosenVariant;

        public GridCell(Vector3Int coords, List<TileVariant> possibleVariants)
        {
            this.coords = coords;
            this.possibleVariants = possibleVariants;
            this.collapsed = false;
            this.chosenVariant = null;
        }

        public void CollapseCell()
        {
            //TODO: backtracking will probably change this (first will erase chosen variant from the possible variants)
            //TODO: use custom-made randomiser?
            //TODO: use weights

            chosenVariant = possibleVariants[Random.Range(0, possibleVariants.Count - 1)];
            collapsed = true;
        }
    }
}
