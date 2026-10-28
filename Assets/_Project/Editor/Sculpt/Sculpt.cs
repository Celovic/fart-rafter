using System.Collections.Generic;
using UnityEngine;

namespace FartRafters.EditorTools.Sculpt
{
    public class Sculpt
    {
        readonly List<SdfShape> shapes = new List<SdfShape>();

        public int Count => shapes.Count;

        public SdfShape Ellipsoid(Vector3 center, Vector3 radii, Color color, float blend = 0.05f, Vector3 euler = default)
        {
            return Add(new SdfShape { kind = SdfKind.Ellipsoid, center = center, size = radii, color = color, blend = blend, rotation = Quaternion.Euler(euler) });
        }

        public SdfShape Sphere(Vector3 center, float radius, Color color, float blend = 0.05f)
        {
            return Ellipsoid(center, Vector3.one * radius, color, blend);
        }

        public SdfShape Limb(Vector3 a, Vector3 b, float radiusA, float radiusB, Color color, float blend = 0.05f)
        {
            return Add(new SdfShape { kind = SdfKind.RoundCone, center = a, end = b, radiusA = radiusA, radiusB = radiusB, color = color, blend = blend });
        }

        public SdfShape Box(Vector3 center, Vector3 size, float rounding, Color color, float blend = 0.03f, Vector3 euler = default)
        {
            return Add(new SdfShape { kind = SdfKind.RoundBox, center = center, size = size, rounding = rounding, color = color, blend = blend, rotation = Quaternion.Euler(euler) });
        }

        public SdfShape Cylinder(Vector3 center, float radius, float halfLength, float rounding, Color color, float blend = 0.02f, Vector3 euler = default)
        {
            return Add(new SdfShape { kind = SdfKind.Cylinder, center = center, size = new Vector3(radius, radius, halfLength), rounding = rounding, color = color, blend = blend, rotation = Quaternion.Euler(euler) });
        }

        public SdfShape Torus(Vector3 center, float major, float minor, Color color, float blend = 0.03f, Vector3 euler = default)
        {
            return Add(new SdfShape { kind = SdfKind.Torus, center = center, radiusA = major, radiusB = minor, color = color, blend = blend, rotation = Quaternion.Euler(euler) });
        }

        public SdfShape Shell(Sculpt source, float inflate, Color color, float blend = 0.02f)
        {
            return Add(new SdfShape { kind = SdfKind.Shell, source = source, inflate = inflate, color = color, blend = blend });
        }

        public void Append(Sculpt other)
        {
            shapes.AddRange(other.shapes);
        }

        public void Mirror(System.Func<float, SdfShape> make)
        {
            make(1f);
            make(-1f);
        }

        public SdfShape Add(SdfShape shape)
        {
            shapes.Add(shape);
            return shape;
        }

        public void Prepare()
        {
            foreach (SdfShape shape in shapes)
            {
                shape.Prepare();
            }
        }

        public Bounds Bounds
        {
            get
            {
                bool any = false;
                var bounds = new Bounds();
                foreach (SdfShape shape in shapes)
                {
                    if (shape.op == SdfOp.Intersect && any)
                    {
                        Vector3 lo = Vector3.Max(bounds.min, shape.Min);
                        Vector3 hi = Vector3.Max(lo, Vector3.Min(bounds.max, shape.Max));
                        bounds = new Bounds((lo + hi) * 0.5f, hi - lo);
                        continue;
                    }
                    if (shape.op != SdfOp.Add)
                    {
                        continue;
                    }
                    if (!any)
                    {
                        bounds = new Bounds((shape.Min + shape.Max) * 0.5f, shape.Max - shape.Min);
                        any = true;
                    }
                    else
                    {
                        bounds.Encapsulate(shape.Min);
                        bounds.Encapsulate(shape.Max);
                    }
                }
                return bounds;
            }
        }

        public float Distance(Vector3 p)
        {
            float d = 1e9f;
            bool first = true;
            foreach (SdfShape shape in shapes)
            {
                if (shape.op == SdfOp.Paint)
                {
                    continue;
                }
                float s = ShapeDistance(shape, p);
                if (shape.op == SdfOp.Add)
                {
                    d = first ? s : SmoothMin(d, s, shape.blend, out _);
                    first = false;
                }
                else if (shape.op == SdfOp.Intersect)
                {
                    d = -SmoothMin(-d, -s, shape.blend, out _);
                }
                else
                {
                    d = SmoothSubtract(d, s, shape.blend, out _);
                }
            }
            return d;
        }

        public Color Color(Vector3 p)
        {
            float d = 1e9f;
            bool first = true;
            Color color = UnityEngine.Color.white;
            foreach (SdfShape shape in shapes)
            {
                float s = ShapeDistance(shape, p);
                if (shape.op == SdfOp.Paint)
                {
                    float t = Smooth01((shape.blend - s) / (2f * shape.blend));
                    color = UnityEngine.Color.Lerp(color, shape.color, t * shape.color.a);
                    continue;
                }
                float h;
                if (shape.op == SdfOp.Add)
                {
                    if (first)
                    {
                        d = s;
                        color = shape.color;
                        first = false;
                        continue;
                    }
                    d = SmoothMin(d, s, shape.blend, out h);
                    color = UnityEngine.Color.Lerp(color, shape.color, h);
                }
                else if (shape.op == SdfOp.Intersect)
                {
                    d = -SmoothMin(-d, -s, shape.blend, out _);
                }
                else
                {
                    d = SmoothSubtract(d, s, shape.blend, out h);
                    color = UnityEngine.Color.Lerp(color, shape.color, h * shape.color.a);
                }
            }
            color.a = 1f;
            return color;
        }

        static float ShapeDistance(SdfShape shape, Vector3 p)
        {
            float d = shape.Distance(p);
            if (shape.noise > 0f)
            {
                d += (Noise.Value3(p * 9f) - 0.5f) * shape.noise + (Noise.Value3(p * 23f + Vector3.one * 7f) - 0.5f) * shape.noise * 0.35f;
            }
            return d;
        }

        static float SmoothMin(float a, float b, float k, out float h)
        {
            if (k <= 0f)
            {
                h = b < a ? 1f : 0f;
                return Mathf.Min(a, b);
            }
            h = Mathf.Clamp01(0.5f + 0.5f * (a - b) / k);
            return Mathf.Lerp(a, b, h) - k * h * (1f - h);
        }

        static float SmoothSubtract(float a, float b, float k, out float h)
        {
            if (k <= 0f)
            {
                h = -b > a ? 1f : 0f;
                return Mathf.Max(a, -b);
            }
            h = Mathf.Clamp01(0.5f - 0.5f * (a + b) / k);
            return Mathf.Lerp(a, -b, h) + k * h * (1f - h);
        }

        static float Smooth01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
    }

    public static class Noise
    {
        public static float Value3(Vector3 p)
        {
            int xi = Mathf.FloorToInt(p.x);
            int yi = Mathf.FloorToInt(p.y);
            int zi = Mathf.FloorToInt(p.z);
            float fx = Fade(p.x - xi);
            float fy = Fade(p.y - yi);
            float fz = Fade(p.z - zi);
            float x00 = Mathf.Lerp(Hash(xi, yi, zi), Hash(xi + 1, yi, zi), fx);
            float x10 = Mathf.Lerp(Hash(xi, yi + 1, zi), Hash(xi + 1, yi + 1, zi), fx);
            float x01 = Mathf.Lerp(Hash(xi, yi, zi + 1), Hash(xi + 1, yi, zi + 1), fx);
            float x11 = Mathf.Lerp(Hash(xi, yi + 1, zi + 1), Hash(xi + 1, yi + 1, zi + 1), fx);
            return Mathf.Lerp(Mathf.Lerp(x00, x10, fy), Mathf.Lerp(x01, x11, fy), fz);
        }

        static float Fade(float t)
        {
            return t * t * (3f - 2f * t);
        }

        static float Hash(int x, int y, int z)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + z * 1274126177);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777215f;
            }
        }
    }
}
