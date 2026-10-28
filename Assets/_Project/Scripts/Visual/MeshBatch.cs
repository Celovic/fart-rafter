using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public class MeshBatch
    {
        readonly struct Key
        {
            public readonly Material material;
            public readonly int x;
            public readonly int z;

            public Key(Material material, int x, int z)
            {
                this.material = material;
                this.x = x;
                this.z = z;
            }
        }

        class KeyComparer : IEqualityComparer<Key>
        {
            public bool Equals(Key a, Key b)
            {
                return a.material == b.material && a.x == b.x && a.z == b.z;
            }

            public int GetHashCode(Key key)
            {
                unchecked
                {
                    return (key.material.GetInstanceID() * 397 ^ key.x) * 397 ^ key.z;
                }
            }
        }

        readonly Dictionary<Key, List<CombineInstance>> parts = new Dictionary<Key, List<CombineInstance>>(new KeyComparer());
        readonly Dictionary<Key, int> vertexCounts = new Dictionary<Key, int>(new KeyComparer());
        readonly float chunkSize;

        public MeshBatch(float chunkSize = 0f)
        {
            this.chunkSize = chunkSize;
        }

        public void Add(Mesh mesh, Material material, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Add(mesh, 0, material, Matrix4x4.TRS(position, rotation, scale));
        }

        public void AddModel(GameObject model, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (model == null)
            {
                return;
            }
            Matrix4x4 placement = Matrix4x4.TRS(position, rotation, scale);
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                Mesh mesh = filter.sharedMesh;
                if (renderer == null || mesh == null)
                {
                    continue;
                }
                Material[] materials = renderer.sharedMaterials;
                Matrix4x4 matrix = placement * filter.transform.localToWorldMatrix;
                for (int i = 0; i < mesh.subMeshCount && i < materials.Length; i++)
                {
                    Add(mesh, i, materials[i], matrix);
                }
            }
        }

        void Add(Mesh mesh, int subMesh, Material material, Matrix4x4 matrix)
        {
            if (mesh == null || material == null)
            {
                return;
            }
            Vector3 position = matrix.GetColumn(3);
            var key = chunkSize > 0f
                ? new Key(material, Mathf.FloorToInt(position.x / chunkSize), Mathf.FloorToInt(position.z / chunkSize))
                : new Key(material, 0, 0);
            if (!parts.TryGetValue(key, out List<CombineInstance> list))
            {
                list = new List<CombineInstance>();
                parts[key] = list;
                vertexCounts[key] = 0;
            }
            list.Add(new CombineInstance { mesh = mesh, subMeshIndex = subMesh, transform = matrix });
            vertexCounts[key] += mesh.GetSubMesh(subMesh).vertexCount;
        }

        public void Build(Transform parent, string name, bool castShadows)
        {
            foreach (KeyValuePair<Key, List<CombineInstance>> pair in parts)
            {
                Material material = pair.Key.material;
                string partName = name + "_" + material.name + "_" + pair.Key.x + "_" + pair.Key.z;
                var mesh = new Mesh { name = partName };
                mesh.indexFormat = vertexCounts[pair.Key] > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();

                var go = new GameObject(partName, typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(parent, true);
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
            parts.Clear();
            vertexCounts.Clear();
        }
    }
}
