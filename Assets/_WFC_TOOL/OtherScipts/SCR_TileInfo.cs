using System;
using UnityEngine;


namespace PCG_Tool
{
    [System.Serializable]
    public struct TileInfo
    {
        public short id;
        public TileOrientation orient;

        public TileInfo(short id, TileOrientation orientation = TileOrientation.None)
        {
            this.id = id;
            this.orient = orientation;
        }

        public Quaternion GetRotation() => GetRotation(orient);
        public Vector3 GetMirroredScale() => GetMirroredScale(orient);

        public static Quaternion GetRotation(TileOrientation orientation)
        {
            return Quaternion.Euler(
                (((orientation & TileOrientation.X_Rot_90) != 0) ? 90 : 0) +
                (((orientation & TileOrientation.X_Rot_180) != 0) ? 180 : 0),
                (((orientation & TileOrientation.Y_Rot_90) != 0) ? 90 : 0) +
                (((orientation & TileOrientation.Y_Rot_180) != 0) ? 180 : 0),
                (((orientation & TileOrientation.Z_Rot_90) != 0) ? 90 : 0) +
                (((orientation & TileOrientation.Z_Rot_180) != 0) ? 180 : 0)
                );
        }

        public static Vector3 GetMirroredScale(TileOrientation orientation)
        {
            if ((orientation & TileOrientation.Mirrored) != 0) return new Vector3(-1, 1, 1);
            return Vector3.one;
        }
    }

    [Flags]
    public enum TileOrientation
    {
        None = 0,
        Mirrored = 1,
        X_Rot_90 = 2,
        X_Rot_180 = 4,
        Y_Rot_90 = 8,
        Y_Rot_180 = 16,
        Z_Rot_90 = 32,
        Z_Rot_180 = 64,
    }
}