using System;
using UnityEngine;

namespace PCG_Tool
{

    [System.Serializable]
    public struct TileRule
    {
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
                0 => new Color(0.8f, 0.8f, 0.8f, 0.5f), // Air (invisible)
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
}
