using UnityEngine;

namespace FartRafters.EditorTools.Sculpt
{
    public enum SdfKind
    {
        Ellipsoid,
        RoundCone,
        RoundBox,
        Torus,
        Cylinder,
        Shell
    }

    public enum SdfOp
    {
        Add,
        Subtract,
        Paint,
        Intersect
    }

    public class SdfShape
    {
        public SdfKind kind;
        public SdfOp op;
        public Vector3 center;
        public Vector3 size = Vector3.one;
        public Vector3 end;
        public float radiusA;
        public float radiusB;
        public float rounding;
        public Quaternion rotation = Quaternion.identity;
        public float blend = 0.05f;
        public Color color = Color.white;
        public float noise;
        public Sculpt source;
        public float inflate;

        Quaternion inverse;

        public void Prepare()
        {
            inverse = Quaternion.Inverse(rotation);
            if (source != null)
            {
                source.Prepare();
            }
        }

        public float Distance(Vector3 p)
        {
            switch (kind)
            {
                case SdfKind.Shell:
                    return source.Distance(p) - inflate;
                case SdfKind.RoundCone:
                    return RoundCone(p, center, end, radiusA, radiusB);
                case SdfKind.RoundBox:
                    return RoundBox(inverse * (p - center), size * 0.5f, rounding);
                case SdfKind.Cylinder:
                    Vector3 c = inverse * (p - center);
                    float radial = new Vector2(c.x, c.y).magnitude - size.x + rounding;
                    float axial = Mathf.Abs(c.z) - size.z + rounding;
                    return Mathf.Min(Mathf.Max(radial, axial), 0f) + new Vector2(Mathf.Max(radial, 0f), Mathf.Max(axial, 0f)).magnitude - rounding;
                case SdfKind.Torus:
                    Vector3 q = inverse * (p - center);
                    float ring = new Vector2(q.x, q.z).magnitude - radiusA;
                    return new Vector2(ring, q.y).magnitude - radiusB;
                default:
                    return Ellipsoid(inverse * (p - center), size);
            }
        }

        public Vector3 Min
        {
            get
            {
                if (kind == SdfKind.Shell)
                {
                    return source.Bounds.min - Vector3.one * (inflate + blend);
                }
                Vector3 extent = Extent;
                return (kind == SdfKind.RoundCone ? Vector3.Min(center, end) : center) - extent;
            }
        }

        public Vector3 Max
        {
            get
            {
                if (kind == SdfKind.Shell)
                {
                    return source.Bounds.max + Vector3.one * (inflate + blend);
                }
                Vector3 extent = Extent;
                return (kind == SdfKind.RoundCone ? Vector3.Max(center, end) : center) + extent;
            }
        }

        Vector3 Extent
        {
            get
            {
                float pad = blend + noise;
                switch (kind)
                {
                    case SdfKind.RoundCone:
                        return Vector3.one * (Mathf.Max(radiusA, radiusB) + pad);
                    case SdfKind.Torus:
                        return Vector3.one * (radiusA + radiusB + pad);
                    case SdfKind.Cylinder:
                        return Vector3.one * (Mathf.Max(size.x, size.z) + pad);
                    default:
                        float r = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) * (kind == SdfKind.RoundBox ? 0.9f : 1f);
                        return Vector3.one * (r + rounding + pad);
                }
            }
        }

        static float Ellipsoid(Vector3 p, Vector3 r)
        {
            float k0 = new Vector3(p.x / r.x, p.y / r.y, p.z / r.z).magnitude;
            float k1 = new Vector3(p.x / (r.x * r.x), p.y / (r.y * r.y), p.z / (r.z * r.z)).magnitude;
            if (k1 < 1e-6f)
            {
                return -Mathf.Min(r.x, Mathf.Min(r.y, r.z));
            }
            return k0 * (k0 - 1f) / k1;
        }

        static float RoundBox(Vector3 p, Vector3 half, float r)
        {
            Vector3 q = new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z)) - half + Vector3.one * r;
            Vector3 outside = Vector3.Max(q, Vector3.zero);
            return outside.magnitude + Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0f) - r;
        }

        static float RoundCone(Vector3 p, Vector3 a, Vector3 b, float r1, float r2)
        {
            Vector3 ba = b - a;
            float l2 = Vector3.Dot(ba, ba);
            if (l2 < 1e-8f)
            {
                return (p - a).magnitude - Mathf.Max(r1, r2);
            }
            float rr = r1 - r2;
            float a2 = l2 - rr * rr;
            float il2 = 1f / l2;
            Vector3 pa = p - a;
            float y = Vector3.Dot(pa, ba);
            float z = y - l2;
            Vector3 xv = pa * l2 - ba * y;
            float x2 = Vector3.Dot(xv, xv);
            float y2 = y * y * l2;
            float z2 = z * z * l2;
            float k = Mathf.Sign(rr) * rr * rr * x2;
            if (Mathf.Sign(z) * a2 * z2 > k)
            {
                return Mathf.Sqrt(x2 + z2) * il2 - r2;
            }
            if (Mathf.Sign(y) * a2 * y2 < k)
            {
                return Mathf.Sqrt(x2 + y2) * il2 - r1;
            }
            return (Mathf.Sqrt(x2 * a2 * il2) + y * rr) * il2 - r1;
        }
    }
}
