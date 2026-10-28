using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FartRafters.EditorTools
{
    public static class AssetInspector
    {
        public static void DumpThirdParty()
        {
            AssetDatabase.Refresh();
            var report = new StringBuilder();
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/_Project/ThirdParty" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null)
                {
                    continue;
                }
                report.AppendLine(path + "  scale " + model.transform.localScale + " rot " + model.transform.localEulerAngles);
                foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
                {
                    Mesh mesh = filter.sharedMesh;
                    var renderer = filter.GetComponent<MeshRenderer>();
                    Transform t = filter.transform;
                    report.Append("  " + t.name + " pos " + t.localPosition + " rot " + t.localEulerAngles + " scale " + t.localScale);
                    if (mesh != null)
                    {
                        report.Append(" verts " + mesh.vertexCount + " subs " + mesh.subMeshCount + " bounds " + mesh.bounds.center + " " + mesh.bounds.size);
                    }
                    report.AppendLine();
                    if (renderer != null)
                    {
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            Texture texture = material != null && material.HasProperty("_BaseMap") ? material.GetTexture("_BaseMap") : null;
                            report.AppendLine("    mat " + (material != null ? material.name + " shader " + material.shader.name + " color " + (material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor").ToString() : "-") : "null") + " tex " + (texture != null ? texture.name : "-"));
                        }
                    }
                }
            }
            File.WriteAllText("Logs/thirdparty.txt", report.ToString());
            Debug.Log("[FartRafters] ThirdParty dump written");
        }
    }
}
