using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PCG_Tool
{

    public class GridCell
    {
        public Vector3Int coords;
        private SBO_Rules _rules;
        private List<TileVariant> possibleVariants;
        public int entropy { get { return possibleVariants.Count; } }
        public bool collapsed { get; private set; }
        public TileVariant chosenVariant;

        public GridCell(Vector3Int coords, List<TileVariant> possibleVariants, SBO_Rules rules)
        {
            this.coords = coords;
            this.possibleVariants = possibleVariants;
            this.collapsed = false;
            this.chosenVariant = null;
            this._rules = rules;
        }

        public void CollapseCell()
        {
            //TODO: backtracking will probably change this (first will erase chosen variant from the possible variants)
            //TODO: use custom-made randomiser?

            //Security
            if (possibleVariants.Count == 0)
            {
                Debug.LogError("ERROR: Trying to collapse a GridCell without possible variants");
                collapsed = true;
                return;
            }

            //Group variants by tileId
            var groups = possibleVariants
                .GroupBy(v => v.tileInfo.id)
                .Select(g => new
                {
                    TileId = g.Key,
                    Weight = g.First().weight, //All of same tileId have the same Weight
                    Variants = g.ToList()
                })
                .ToList();

            //Calculate sum of weights and weighted random
            float totalWeight = groups.Sum(g => g.Weight);
            float r = Random.value * totalWeight;
            float accum = 0f;
            List<TileVariant> chosenGroup = null;
            foreach (var grp in groups)
            {
                accum += grp.Weight;
                if (r <= accum)
                {
                    chosenGroup = grp.Variants;
                    break;
                }
            }

            //In case of error, chose the first group
            if (chosenGroup == null) chosenGroup = groups[0].Variants;

            //Choose random variant in group
            int idx = Random.Range(0, chosenGroup.Count);

            chosenVariant = chosenGroup[idx];
            collapsed = true;
        }

        public void CheckAllVariantsToFace(TileVariant nbVariant, FaceDirection nbDir)
        {
            switch (nbDir)
            {
                case FaceDirection.Up:
                    for (int i = possibleVariants.Count - 1; i >= 0; i--)
                        if (!_rules.AreFacesCompatible(nbVariant.Down, possibleVariants[i].Up))
                        {
                            possibleVariants.RemoveAt(i);
                        }
                    break;

                case FaceDirection.Down:
                    for (int i = possibleVariants.Count - 1; i >= 0; i--)
                        if (!_rules.AreFacesCompatible(nbVariant.Up, possibleVariants[i].Down))
                        {
                            possibleVariants.RemoveAt(i);
                        }
                    break;

                case FaceDirection.Left:
                    for (int i = possibleVariants.Count - 1; i >= 0; i--)
                        if (!_rules.AreFacesCompatible(nbVariant.Right, possibleVariants[i].Left))
                        {
                            possibleVariants.RemoveAt(i);
                        }
                    break;

                case FaceDirection.Right:
                    for (int i = possibleVariants.Count - 1; i >= 0; i--)
                        if (!_rules.AreFacesCompatible(nbVariant.Left, possibleVariants[i].Right))
                        {
                            possibleVariants.RemoveAt(i);
                        }
                    break;

                case FaceDirection.Forward:
                    for (int i = possibleVariants.Count - 1; i >= 0; i--)
                        if (!_rules.AreFacesCompatible(nbVariant.Back, possibleVariants[i].Forward))
                        {
                            possibleVariants.RemoveAt(i);
                        }
                    break;
                case FaceDirection.Back:
                    for (int i = possibleVariants.Count - 1; i >= 0; i--)
                        if (!_rules.AreFacesCompatible(nbVariant.Forward, possibleVariants[i].Back))
                        {
                            possibleVariants.RemoveAt(i);
                        }
                    break;

            }
        }
    }
}
