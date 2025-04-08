using UnityEngine;

public static class SCR_CheckerTextureUtility
{
    public static Texture2D GetCheckerTexture(int size = 4)
    {
        Texture2D checkerTexture = new Texture2D(size, size);
        checkerTexture.wrapMode = TextureWrapMode.Repeat;
        checkerTexture.filterMode = FilterMode.Point;

        Color color1 = new Color(0.9f, 0.9f, 0.9f);
        Color color2 = new Color(0.4f, 0.4f, 0.4f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isEven = (x + y) % 2 == 0;
                checkerTexture.SetPixel(x, y, isEven ? color1 : color2);
            }
        }

        checkerTexture.Apply();
        return checkerTexture;
    }
}
