using System.Collections.Generic;
using UnityEngine;


namespace PCG_Tool
{

    public class TileVariant
    {
        public TileInfo tileInfo;
        public float weight;

        public TileColor Up;
        public TileColor Down;
        public TileColor Left;
        public TileColor Right;
        public TileColor Forward;
        public TileColor Back;

        public TileVariant() { }
        public TileVariant(TileVariant tileVariant)
        {
            tileInfo = tileVariant.tileInfo;
            weight = tileVariant.weight;
            Up = tileVariant.Up;
            Down = tileVariant.Down;
            Left = tileVariant.Left;
            Right = tileVariant.Right;
            Forward = tileVariant.Forward;
            Back = tileVariant.Back;
        }

        private static readonly Vector3[] originalDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        public static List<TileVariant> GenerateVariantsFromTileRule(TileRule rule, short tileId)
        {
            List<TileVariant> variants = new List<TileVariant>();

            //Original TileColors
            TileColor[] originalTileColors = { rule.Up, rule.Down, rule.Left, rule.Right, rule.Forward, rule.Back };
            TileColor[] originalMirroredColors = { rule.Up, rule.Down, rule.Right, rule.Left, rule.Forward, rule.Back };

            List<Quaternion> rotations = rule.GetPossibleRotations();

            foreach(Quaternion rot in rotations)
            {
                //Rotated Variant
                TileVariant variant = new TileVariant();
                variant.tileInfo.id = tileId;
                variant.weight = rule.weight;

                ProcessTileRotation(rot, originalTileColors, ref variant);
                variants.Add(variant);

                //Mirrored Variant
                if ((rule.constraints & TileConstraints.AllowMirror) != 0)
                {
                    TileVariant mirroredVariant = new TileVariant();
                    mirroredVariant.tileInfo.id = tileId;
                    variant.weight = rule.weight;

                    ProcessTileRotation(rot, originalMirroredColors, ref mirroredVariant);
                    mirroredVariant.tileInfo.orient |= TileOrientation.Mirrored;

                    variants.Add(mirroredVariant);
                }
            }
            
            return variants;
        }

        public static void ProcessTileRotation(Quaternion rot, TileColor[] originalTileColors, ref TileVariant variant)
        {
            //Determine orientation from Quaternion
            variant.tileInfo.orient = TileInfo.GetOrientation(rot);

            //Rotate every face and assign a color
            for (int i = 0; i < originalDirections.Length; i++)
            {
                Vector3 rotated = rot * originalDirections[i];
                //Round rotated face values to get perfect ints
                Vector3 rounded = new Vector3(
                    Mathf.Round(rotated.x),
                    Mathf.Round(rotated.y),
                    Mathf.Round(rotated.z)
                );

                //Rotate colors
                if (rounded == Vector3.up) variant.Up = originalTileColors[i];
                else if (rounded == Vector3.down) variant.Down = originalTileColors[i];
                else if (rounded == Vector3.left) variant.Left = originalTileColors[i];
                else if (rounded == Vector3.right) variant.Right = originalTileColors[i];
                else if (rounded == Vector3.forward) variant.Forward = originalTileColors[i];
                else if (rounded == Vector3.back) variant.Back = originalTileColors[i];
                else Debug.LogWarning("ERROR: Direction values not corresponding to orthogonal directions.");
            }
        }
    }

}