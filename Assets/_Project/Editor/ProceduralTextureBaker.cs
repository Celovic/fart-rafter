using System.IO;
using UnityEditor;
using UnityEngine;

namespace FartRafters.EditorTools
{
    public static class ProceduralTextureBaker
    {
        const string Folder = "Assets/_Project/Generated/Textures";

        public struct Set
        {
            public Texture2D detail;
            public Texture2D detailNormal;
            public Texture2D waveNormal;
            public Texture2D foam;
            public Texture2D macro;
        }

        public static Set Bake()
        {
            Directory.CreateDirectory(Folder);
            float[] detail = Field(256, new[] { 8, 16, 32, 64 }, new[] { 0.45f, 0.3f, 0.17f, 0.08f }, 11);
            float[] waves = Field(256, new[] { 3, 6, 12, 24 }, new[] { 0.5f, 0.3f, 0.15f, 0.05f }, 23);
            float[] foam = Field(256, new[] { 6, 12, 24, 48 }, new[] { 0.45f, 0.3f, 0.17f, 0.08f }, 37);
            float[] macro = Field(256, new[] { 2, 4, 8, 16 }, new[] { 0.5f, 0.28f, 0.15f, 0.07f }, 51);
            Ridge(foam);

            var set = new Set
            {
                detail = Save("Detail", 256, detail, false, 0f),
                detailNormal = Save("DetailNormal", 256, detail, true, 5f),
                waveNormal = Save("WaveNormal", 256, waves, true, 3.2f),
                foam = Save("Foam", 256, foam, false, 0f),
                macro = Save("Macro", 256, macro, false, 0f)
            };
            return set;
        }

        static void Ridge(float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                float v = 1f - Mathf.Abs(values[i] * 2f - 1f);
                values[i] = Mathf.Pow(v, 3f);
            }
        }

        static float[] Field(int size, int[] periods, float[] weights, int seed)
        {
            var values = new float[size * size];
            for (int o = 0; o < periods.Length; o++)
            {
                int period = periods[o];
                var random = new System.Random(seed + o * 101);
                var gradients = new Vector2[period * period];
                for (int g = 0; g < gradients.Length; g++)
                {
                    float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                    gradients[g] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float fx = x / (float)size * period;
                        float fy = y / (float)size * period;
                        values[y * size + x] += Gradient(gradients, period, fx, fy) * weights[o];
                    }
                }
            }
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (float v in values)
            {
                min = Mathf.Min(min, v);
                max = Mathf.Max(max, v);
            }
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Mathf.InverseLerp(min, max, values[i]);
            }
            return values;
        }

        static float Gradient(Vector2[] gradients, int period, float x, float y)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = x - x0;
            float ty = y - y0;
            float u = tx * tx * tx * (tx * (tx * 6f - 15f) + 10f);
            float v = ty * ty * ty * (ty * (ty * 6f - 15f) + 10f);
            float a = Dot(gradients, period, x0, y0, tx, ty);
            float b = Dot(gradients, period, x0 + 1, y0, tx - 1f, ty);
            float c = Dot(gradients, period, x0, y0 + 1, tx, ty - 1f);
            float d = Dot(gradients, period, x0 + 1, y0 + 1, tx - 1f, ty - 1f);
            return Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
        }

        static float Dot(Vector2[] gradients, int period, int x, int y, float dx, float dy)
        {
            Vector2 g = gradients[((y % period + period) % period) * period + (x % period + period) % period];
            return g.x * dx + g.y * dy;
        }

        static Texture2D Save(string name, int size, float[] values, bool normal, float strength)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, normal);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float v = values[y * size + x];
                    if (!normal)
                    {
                        pixels[y * size + x] = new Color(v, v, v, 1f);
                        continue;
                    }
                    float l = values[y * size + (x - 1 + size) % size];
                    float r = values[y * size + (x + 1) % size];
                    float d = values[((y - 1 + size) % size) * size + x];
                    float u = values[((y + 1) % size) * size + x];
                    Vector3 n = new Vector3((l - r) * strength, (d - u) * strength, 1f).normalized;
                    pixels[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            string path = Folder + "/" + name + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.anisoLevel = 4;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
