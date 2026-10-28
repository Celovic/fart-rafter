using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public static class AvatarVisualFactory
    {
        public const string VisualName = "Visual";
        public const string MarkerName = "PlayerMarker";

        public static Vector3 PuffPosition(AvatarDefinition avatar)
        {
            return avatar.puffOffset;
        }

        public static void AddPlayerMarker(Transform rider, CourseTheme theme, int player)
        {
            if (theme.playerMarkers == null || player < 0 || player >= theme.playerMarkers.Length)
            {
                return;
            }
            Transform old = rider.Find(MarkerName);
            if (old != null)
            {
                Object.DestroyImmediate(old.gameObject);
            }
            GameObject marker = CourseBuilder.CreateVisual(PrimitiveType.Cylinder, MarkerName, rider, theme.playerMarkers[player]);
            marker.transform.localPosition = new Vector3(0f, 0.012f, 0f);
            marker.transform.localScale = new Vector3(1.05f, 0.012f, 1.05f);
            marker.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        public static AvatarRig Build(Transform parent, AvatarDefinition avatar, CourseTheme theme, int costume = 0)
        {
            var visual = new GameObject(VisualName).transform;
            visual.SetParent(parent, false);
            var rig = visual.gameObject.AddComponent<AvatarRig>();
            rig.kind = avatar.kind;

            var root = new GameObject("Root").transform;
            root.SetParent(visual, false);
            rig.root = root;

            Material material = avatar.material;
            Part("Body", root, avatar.bodyMesh, material, Vector3.zero);

            var head = new GameObject("Head").transform;
            head.SetParent(root, false);
            head.localPosition = avatar.headPivot;
            rig.head = head;
            Part("Skull", head, avatar.headMesh, material, Vector3.zero);
            Costume outfit = avatar.GetCostume(costume);
            if (outfit != null)
            {
                if (outfit.head != null)
                {
                    Part("CostumeHead", head, outfit.head, material, Vector3.zero);
                }
                if (outfit.body != null)
                {
                    Part("CostumeBody", root, outfit.body, material, Vector3.zero);
                }
            }

            rig.eyes = new Transform[2];
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 position = avatar.eyePosition;
                position.x *= s;
                rig.eyes[s < 0 ? 0 : 1] = Part(s < 0 ? "EyeLeft" : "EyeRight", head, avatar.eyeMesh, theme != null && theme.eyeMaterial != null ? theme.eyeMaterial : material, position).transform;
            }

            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 position = avatar.shoulder;
                position.x *= s;
                var arm = new GameObject(s < 0 ? "ArmLeft" : "ArmRight").transform;
                arm.SetParent(root, false);
                arm.localPosition = position;
                Part("Arm", arm, s < 0 ? avatar.armLeftMesh : avatar.armRightMesh, material, Vector3.zero);
                Mesh sleeve = outfit != null ? (s < 0 ? outfit.armLeft : outfit.armRight) : null;
                if (sleeve != null)
                {
                    Part("CostumeArm", arm, sleeve, material, Vector3.zero);
                }
                if (s < 0)
                {
                    rig.armLeft = arm;
                }
                else
                {
                    rig.armRight = arm;
                }
            }

            return rig;
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
