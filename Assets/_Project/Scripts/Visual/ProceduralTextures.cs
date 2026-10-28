using UnityEngine;

namespace FartRafters
{
    public static class ProceduralTextures
    {
        static Texture2D checker;

        public static Texture2D Checker
        {
            get
            {
                if (checker != null)
                {
                    return checker;
                }
                checker = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    name = "Checker",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Repeat
                };
                checker.SetPixels32(new[]
                {
                    new Color32(25, 25, 25, 255), new Color32(245, 245, 245, 255),
                    new Color32(245, 245, 245, 255), new Color32(25, 25, 25, 255)
                });
                checker.Apply();
                return checker;
            }
        }
    }
}
