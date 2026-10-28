using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public static class SpectatorFactory
    {
        static readonly string[] Cheers =
        {
            "BAS GAZA!", "HAYDİ SAL!", "SÜPER SAL!", "FASULYE\nGÜCÜ", "PASTIRMA\nŞART", "OSUR\nGİTSİN!", "ŞAMPİYON\nSAL", "NOHUT YİYEN\nKAZANIR", "GAZ KOKUSU\nVAR!"
        };

        static readonly string[] Jeers =
        {
            "YUH!", "BATTINIZ!", "YAVAŞ\nSAL!", "TİMSAH\nKAZANSIN", "KOKUDAN\nBAYILDIK", "EVE DÖN!"
        };

        public static string RandomSign(bool jeer, System.Random random)
        {
            string[] pool = jeer ? Jeers : Cheers;
            return pool[random.Next(pool.Length)];
        }

        public static string RandomSign(bool jeer, System.Random random, CourseTheme theme)
        {
            string[] extra = theme == null ? null : jeer ? theme.extraJeers : theme.extraCheers;
            if (extra != null && extra.Length > 0 && random.NextDouble() < 0.45)
            {
                return extra[random.Next(extra.Length)];
            }
            string text = RandomSign(jeer, random);
            if (theme != null && !string.IsNullOrEmpty(theme.predatorName) && theme.biome != Biome.River)
            {
                text = text.Replace("TİMSAH", theme.predatorName.ToUpper(new System.Globalization.CultureInfo("tr-TR")));
            }
            return text;
        }

        public static Spectator Build(Transform parent, SpectatorLook look, CourseTheme theme, SpectatorAct act, string text, int snack)
        {
            var holder = new GameObject("Spectator").transform;
            holder.SetParent(parent, false);
            var spectator = holder.gameObject.AddComponent<Spectator>();
            spectator.act = act;

            var root = new GameObject("Root").transform;
            root.SetParent(holder, false);
            spectator.root = root;
            Material material = theme.sculptMaterial;
            Part("Body", root, look.body, material, Vector3.zero);

            var head = new GameObject("Head").transform;
            head.SetParent(root, false);
            head.localPosition = look.headPivot;
            spectator.head = head;
            Part("Skull", head, look.head, material, Vector3.zero);

            spectator.eyes = new Transform[2];
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 position = look.eyePosition;
                position.x *= s;
                spectator.eyes[s < 0 ? 0 : 1] = Part("Eye", head, look.eye, theme.eyeMaterial != null ? theme.eyeMaterial : material, position).transform;
            }

            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 position = look.shoulder;
                position.x *= s;
                var arm = new GameObject(s < 0 ? "ArmLeft" : "ArmRight").transform;
                arm.SetParent(root, false);
                arm.localPosition = position;
                Mesh mesh = s < 0 ? look.armLeft : look.armRight;
                Part("Arm", arm, mesh, material, Vector3.zero);
                if (s < 0)
                {
                    spectator.armLeft = arm;
                }
                else
                {
                    spectator.armRight = arm;
                    if (act == SpectatorAct.Eat && theme.snackMeshes != null && theme.snackMeshes.Length > 0 && mesh != null)
                    {
                        Mesh food = theme.snackMeshes[Mathf.Abs(snack) % theme.snackMeshes.Length];
                        Vector3 hand = new Vector3(0.05f, mesh.bounds.min.y + 0.02f, 0.12f);
                        GameObject piece = Part("Snack", arm, food, material, hand);
                        piece.transform.localRotation = Quaternion.Euler(-60f, 0f, 0f);
                    }
                }
            }

            if (act == SpectatorAct.Sign && theme.signMesh != null)
            {
                var sign = new GameObject("Sign").transform;
                sign.SetParent(root, false);
                spectator.signBaseHeight = look.shoulder.y + (look.armLeft != null ? -look.armLeft.bounds.min.y : 0.3f) - 0.12f;
                sign.localPosition = new Vector3(0f, spectator.signBaseHeight, 0.08f);
                Part("Board", sign, theme.signMesh, material, Vector3.zero);
                spectator.sign = sign;
                spectator.signText = Label(sign, text);
            }
            var lod = holder.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(0.025f, holder.GetComponentsInChildren<Renderer>()) });
            return spectator;
        }

        static TextMesh Label(Transform sign, string text)
        {
            var label = new GameObject("Text");
            label.transform.SetParent(sign, false);
            label.transform.localPosition = new Vector3(0f, 0.75f, 0.05f);
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var mesh = label.AddComponent<TextMesh>();
            mesh.font = UiKit.Font;
            mesh.fontSize = 64;
            mesh.characterSize = 0.02f;
            mesh.fontStyle = FontStyle.Bold;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.lineSpacing = 0.9f;
            mesh.color = new Color(0.12f, 0.08f, 0.08f);
            mesh.text = text;
            var renderer = label.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = UiKit.Font.material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return mesh;
        }

        static GameObject Part(string name, Transform parent, Mesh mesh, Material material, Vector3 position)
        {
            var part = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            return part;
        }
    }
}
