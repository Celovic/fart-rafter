using UnityEngine;

namespace FartRafters
{
    public static class RaftVisualFactory
    {
        public const string VisualName = "RaftVisual";

        static readonly Vector3[] PropPositions = { new Vector3(0f, 0.2f, 1.2f), new Vector3(0.05f, 0.2f, -1.25f) };
        static readonly float[] PropScales = { 0.42f, 0.55f };

        public static void ApplyVariant(Transform root, RaftVariant variant)
        {
            if (root == null || variant == null)
            {
                return;
            }
            Transform visual = root.Find(RaftVisualFactory.VisualName);
            if (visual == null)
            {
                return;
            }
            foreach (Transform child in visual)
            {
                if (child.name == "Logs")
                {
                    if (variant.mesh != null)
                    {
                        child.GetComponent<MeshFilter>().sharedMesh = variant.mesh;
                    }
                    if (variant.material != null)
                    {
                        child.GetComponent<MeshRenderer>().sharedMaterial = variant.material;
                    }
                }
                else if (child.name != "Pole" && child.name != "FlagPivot")
                {
                    child.gameObject.SetActive(variant.props);
                }
            }
        }

        public static Transform Build(Transform parent, CourseTheme theme)
        {
            var visual = new GameObject(VisualName).transform;
            visual.SetParent(parent, false);

            GameObject logs = CourseBuilder.CreateSculpt("Logs", visual, theme.raftMesh, theme.raftMaterial != null ? theme.raftMaterial : theme.sculptMaterial, Vector3.zero, Vector3.one);
            logs.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            if (theme.raftProps != null)
            {
                for (int i = 0; i < theme.raftProps.Length && i < PropPositions.Length; i++)
                {
                    GameObject model = theme.raftProps[i];
                    if (model == null)
                    {
                        continue;
                    }
                    GameObject prop = Object.Instantiate(model, visual, false);
                    prop.name = model.name;
                    prop.transform.localPosition = PropPositions[i];
                    prop.transform.localRotation = Quaternion.Euler(0f, i * 40f, 0f) * model.transform.localRotation;
                    prop.transform.localScale = model.transform.localScale * PropScales[i];
                }
            }

            GameObject pole = CourseBuilder.CreateVisual(PrimitiveType.Cylinder, "Pole", visual, theme.raftLog);
            pole.transform.localPosition = new Vector3(0f, 1.05f, -0.1f);
            pole.transform.localScale = new Vector3(0.08f, 0.9f, 0.08f);

            var flagPivot = new GameObject("FlagPivot").transform;
            flagPivot.SetParent(visual, false);
            flagPivot.localPosition = new Vector3(0f, 1.75f, -0.1f);
            GameObject flag = CourseBuilder.CreateVisual(PrimitiveType.Cube, "Flag", flagPivot, theme.flag);
            flag.transform.localPosition = new Vector3(0f, 0f, -0.34f);
            flag.transform.localScale = new Vector3(0.025f, 0.38f, 0.64f);
            flagPivot.gameObject.AddComponent<FlagWave>();
            return visual;
        }
    }
}
