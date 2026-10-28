using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public static partial class CourseBuilder
    {
        const float SpinnerMeshRadius = 4f;

        static void BuildSpinner(MillSpec spec, CourseTheme theme, Transform parent)
        {
            var spinner = new GameObject("Spinner").transform;
            spinner.SetParent(parent, false);
            spinner.SetPositionAndRotation(Lift(new Vector3(spec.position.x, 0f, spec.position.z)), Quaternion.Euler(0f, spec.yaw, 0f));

            GameObject post = CreateVisual(PrimitiveType.Cylinder, "Post", spinner, theme.finish);
            post.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            post.transform.localScale = new Vector3(0.9f, 1.1f, 0.9f);
            var postCollider = post.AddComponent<CapsuleCollider>();
            postCollider.radius = 0.5f;
            postCollider.height = 2f;
            postCollider.sharedMaterial = theme.physics;

            var arms = new GameObject("Arms");
            arms.SetActive(false);
            arms.transform.SetParent(spinner, false);
            arms.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            arms.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            arms.AddComponent<Rigidbody>();
            arms.AddComponent<MillWheel>().speed = spec.speed * 1.6f;
            arms.AddComponent<DamageSource>().multiplier = 1.4f;
            float reach = spec.radius * 1.15f;
            var box = arms.AddComponent<BoxCollider>();
            box.size = new Vector3(reach * 2f, 0.5f, 0.6f);
            box.sharedMaterial = theme.physics;
            GameObject visual = CreateSculpt("ArmVisual", arms.transform, theme.spinnerMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * (reach / SpinnerMeshRadius));
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            arms.SetActive(true);
        }

        static Renderer[] BuildBambooGate(Transform gate, float width, CourseTheme theme, string label)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                CreateSculpt("Pole", gate, theme.gatePoleMesh, theme.sculptMaterial, new Vector3(s * (width * 0.5f + 0.25f), -0.6f, 0f), Vector3.one);
            }
            GameObject banner = CreateVisual(PrimitiveType.Cube, "Banner", gate, theme.checkpoint);
            banner.transform.localPosition = new Vector3(0f, 3.9f, 0f);
            banner.transform.localScale = new Vector3(width + 0.9f, 1.05f, 0.18f);
            if (!string.IsNullOrEmpty(label))
            {
                var text = new GameObject("Label");
                text.transform.SetParent(gate, false);
                text.transform.localPosition = new Vector3(0f, 3.9f, -0.11f);
                var mesh = text.AddComponent<TextMesh>();
                mesh.font = UiKit.Font;
                mesh.fontSize = 72;
                mesh.characterSize = 0.06f;
                mesh.fontStyle = FontStyle.Bold;
                mesh.anchor = TextAnchor.MiddleCenter;
                mesh.alignment = TextAlignment.Center;
                mesh.color = new Color(0.45f, 0.22f, 0.08f);
                mesh.text = label;
                var renderer = text.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = UiKit.Font.material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            return new[] { banner.GetComponent<Renderer>() };
        }
    }
}
