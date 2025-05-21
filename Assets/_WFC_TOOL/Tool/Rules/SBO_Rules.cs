using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{
    [CreateAssetMenu(fileName = "New Rules", menuName = "WFC_Tool/Rules")]
    public class SBO_Rules : ScriptableObject
    {
        public SBO_TileSet tileSet;

        public TileRule[] tileRules;

        public const int MATRIX_COLOR_COUNT = 16;
        public TileColor[] colorTable; //Color compatibility matrix

        public SBO_Rules()
        {
            //Initialise colorTable diagonal
            colorTable = new TileColor[MATRIX_COLOR_COUNT];

            for (int i = 0; i < MATRIX_COLOR_COUNT; i++)
            {
                TileColor color = (TileColor)(1 << i);
                colorTable[i] = color;
            }
        }

        //Color compatibility
        //Check if two colors are compatible (unidirectional), asumes at least color1 is a single color (not multiple)
        public bool AreCompatible(TileColor color1, TileColor color2)
        {
            //Only checking one side
            int index1 = GetColorIndex(color1);
            if (index1 < 0) return false;
            return (colorTable[index1] & color2) != 0;
        }

        //Check if two faces (with different color combinations) are compatible taking into account color Matrix
        public bool AreFacesCompatible(TileColor face1, TileColor face2)
        {
            TileColor face1Compatibles = TileColor.None;

            //Check the colors face1 has, then add the compatibilities of those colors to the total compatibilities
            for (int i = 0; i < MATRIX_COLOR_COUNT; i++)
            {
                if (((ushort)face1 & (1 << i)) != 0)
                {
                    //If it contains color (i), add compatibilities of (i)
                    face1Compatibles |= colorTable[i];
                }
            }

            //Compare to face2
            return (face1Compatibles & face2) != 0;
        }

        //Toggle compatibility between color1 and color2 (bidirectional)
        public bool SwitchCompatibility(TileColor color1, TileColor color2)
        {
            int index1 = GetColorIndex(color1);
            int index2 = GetColorIndex(color2);
            if (index1 < 0 || index2 < 0) return false;

            bool isNowCompatible;

            if ((colorTable[index1] & color2) != 0)
            {
                // Remove both directions
                colorTable[index1] &= ~color2;
                colorTable[index2] &= ~color1;
                isNowCompatible = false;
            }
            else
            {
                // Add both directions
                colorTable[index1] |= color2;
                colorTable[index2] |= color1;
                isNowCompatible = true;
            }

            return isNowCompatible;
        }

        // Helper: Get index 0-15 from TileColor (assumes only one flag is active)
        private int GetColorIndex(TileColor color)
        {
            if (color == TileColor.None) return -1;

            ushort value = (ushort)color;
            for (int i = 0; i < MATRIX_COLOR_COUNT; i++)
            {
                if ((value & (1 << i)) != 0)
                    return i;
            }

            Debug.LogWarning("Error: checking color compatibility failed.");
            return -1; // Invalid color
        }
    }
}