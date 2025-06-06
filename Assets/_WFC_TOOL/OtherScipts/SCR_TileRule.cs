using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PCG_Tool
{

    [System.Serializable]
    public struct TileRule
    {
        public TileConstraints constraints;
        public float weight; 

        public TileColor Up;
        public TileColor Down;
        public TileColor Left;
        public TileColor Right;
        public TileColor Forward;
        public TileColor Back;

        public static Color GetColor(int index)
        {
            return index switch
            {
                0 => new Color(0.8f, 0.8f, 0.8f), // Air (invisible)
                1 => Color.red,
                2 => Color.green,
                3 => Color.blue,
                4 => Color.yellow,
                5 => new Color(1f, 0.5f, 0f), // Orange
                6 => new Color(0.5f, 0f, 0.5f), // Purple
                7 => Color.cyan,
                8 => Color.magenta,
                9 => Color.white,
                10 => Color.black,
                11 => new Color(0.4f, 0.26f, 0.13f), // Brown
                12 => new Color(1f, 0.75f, 0.8f), // Pink
                13 => new Color(0.75f, 1f, 0f), // Lime
                14 => new Color(0.29f, 0f, 0.51f), // Indigo
                15 => new Color(0f, 0.5f, 0.5f), // Teal
                _ => Color.clear
            };
        }

        public static TileColor GetTileColorByIndex(int index)
        {
            if (index < 0 || index > 15)
                throw new System.ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 15.");

            return (TileColor)(1 << index);
        }

        public void SwitchColorToFace(FaceDirection dir, TileColor color)
        {
            TileColor current = GetFace(dir);

            current ^= color;

            SetFace(dir, current);
        }

        public bool HasColor(FaceDirection dir, TileColor color)
        {
            return (GetFace(dir) & color) != 0;
        }

        public TileColor GetFace(FaceDirection dir)
        {
            return dir switch
            {
                FaceDirection.Up => Up,
                FaceDirection.Down => Down,
                FaceDirection.Left => Left,
                FaceDirection.Right => Right,
                FaceDirection.Forward => Forward,
                FaceDirection.Back => Back,
                _ => TileColor.None
            };
        }

        private void SetFace(FaceDirection dir, TileColor value)
        {
            switch (dir)
            {
                case FaceDirection.Up: Up = value; break;
                case FaceDirection.Down: Down = value; break;
                case FaceDirection.Left: Left = value; break;
                case FaceDirection.Right: Right = value; break;
                case FaceDirection.Forward: Forward = value; break;
                case FaceDirection.Back: Back = value; break;
            }
        }
        
        public List<Quaternion> GetPossibleRotations()
        {
            List<Quaternion> rot = new List<Quaternion>();

            //NO ROTATIONS
            if (constraints == TileConstraints.None)
            {
                rot.Add(Quaternion.identity);
                return rot;
            }

            //Only X rotation
            if (constraints == TileConstraints.Allow_X_Rotation)
            {
                for (int i = 0; i < 4; i++) rot.Add(Quaternion.Euler(i * 90, 0, 0));
                return rot;
            }

            Vector3[] ups = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            Vector3[] forwards = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            //Only Y rotation
            if (constraints == TileConstraints.Allow_Y_Rotation)
            {
                ups = new Vector3[] { Vector3.up };
            }

            //Only Z rotation
            else if (constraints == TileConstraints.Allow_Z_Rotation)
            {
                forwards = new Vector3[] { Vector3.forward };
            }

            //All rotations
            foreach (Vector3 upDir in ups)
            {
                foreach (Vector3 forwardDir in forwards)
                {
                    //Ignore non-orthogonal directions
                    float dot = Vector3.Dot(upDir, forwardDir);
                    if (dot > 0.1f || dot < -0.1f) continue;

                    //Calculate complete rotation
                    rot.Add(Quaternion.LookRotation(forwardDir, upDir));
                }
            }

            return rot;
        }
    }

    public enum FaceDirection
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back
    }

    [System.Flags]
    public enum TileColor : ushort
    {
        None = 0,
        Air = 1 << 0,
        Red = 1 << 1,
        Green = 1 << 2,
        Blue = 1 << 3,
        Yellow = 1 << 4,
        Orange = 1 << 5,
        Purple = 1 << 6,
        Cyan = 1 << 7,
        Magenta = 1 << 8,
        White = 1 << 9,
        Black = 1 << 10,
        Brown = 1 << 11,
        Pink = 1 << 12,
        Lime = 1 << 13,
        Indigo = 1 << 14,
        Teal = 1 << 15
    }

    [System.Flags]
    public enum TileConstraints : byte
    {
        None = 0,
        AllowMirror = 1 << 0,
        Allow_X_Rotation = 1 << 1,
        Allow_Y_Rotation = 1 << 2,
        Allow_Z_Rotation = 1 << 3,
    }
}
