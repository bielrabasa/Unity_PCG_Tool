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

            //Possible axis vectors for each direction TODO: Check if possible to just use one array
            Vector3[] ups = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            Vector3[] forwards = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            //ups will be Vector3.Up in case of only rotate Y
            //forwards will be Vector3.Forward in case of only rotate Z
            //right will be Vector3.Right in case of only rotate X (this has to be checked appart

            //Original TileColors
            TileColor[] originalTileColors = { rule.Up, rule.Down, rule.Left, rule.Right, rule.Forward, rule.Back };
            TileColor[] originalMirroredColors = { rule.Up, rule.Down, rule.Right, rule.Left, rule.Forward, rule.Back };

            foreach (Vector3 upDir in ups)
            {
                foreach (Vector3 forwardDir in forwards)
                {
                    //Ignore non-orthogonal directions
                    float dot = Vector3.Dot(upDir, forwardDir);
                    if (dot > 0.1f || dot < -0.1f) continue;

                    Quaternion rot = Quaternion.LookRotation(forwardDir, upDir);

                    //Rotated Variant
                    TileVariant variant = new TileVariant();
                    variant.tileInfo.id = tileId;

                    ProcessTileRotation(rot, originalTileColors, ref variant);
                    variants.Add(variant);

                    //Mirrored Variant
                    if ((rule.constraints & TileConstraints.AllowMirror) != 0)
                    {
                        TileVariant mirroredVariant = new TileVariant();
                        mirroredVariant.tileInfo.id = tileId;

                        ProcessTileRotation(rot, originalMirroredColors, ref mirroredVariant);
                        mirroredVariant.tileInfo.orient |= TileOrientation.Mirrored;

                        variants.Add(mirroredVariant);
                    }
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