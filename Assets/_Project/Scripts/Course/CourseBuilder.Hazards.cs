using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public static partial class CourseBuilder
    {
        const float RampLength = 7f;
        const float RampRise = 1.35f;

        static void BuildGeysers(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.geysers == null || level.geysers.Length == 0)
            {
                return;
            }
            var root = new GameObject("Geysers").transform;
            root.SetParent(parent, false);
            foreach (GeyserSpec spec in level.geysers)
            {
                var go = new GameObject("Geyser");
                go.transform.SetParent(root, false);
                go.transform.position = Lift(new Vector3(spec.position.x, 0f, spec.position.z));
                var geyser = go.AddComponent<Geyser>();
                geyser.period = spec.period;
                geyser.phase = spec.phase;
                geyser.radius = spec.radius;
                geyser.bubbles = CreateBubbles(go.transform, new Vector3(0f, -0.1f, 0f), new Vector3(spec.radius * 1.6f, 0.1f, spec.radius * 1.6f), new Vector3(0f, 1.6f, 0f), theme.bubbles);
                geyser.burst = EffectFactory.CreateSplash(go.transform, theme.splash);
                var foam = new Material(theme.finish) { name = "GeyserFoam" };
                foam.SetColor("_BaseColor", new Color(0.85f, 0.95f, 1f));
                GameObject dome = CreateVisual(PrimitiveType.Sphere, "Dome", go.transform, foam);
                dome.transform.localPosition = new Vector3(0f, -0.05f, 0f);
                dome.transform.localScale = new Vector3(spec.radius * 0.4f, 0.25f, spec.radius * 0.4f);
                dome.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                geyser.dome = dome.transform;
                var hazard = go.AddComponent<SoftHazard>();
                hazard.radius = spec.radius + 0.8f;
                hazard.weight = 0.8f;
            }
        }

        static void BuildBarrels(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.barrels == null || level.barrels.Length == 0 || theme.barrelMesh == null)
            {
                return;
            }
            var root = new GameObject("Barrels").transform;
            root.SetParent(parent, false);
            for (int i = 0; i < level.barrels.Length; i++)
            {
                BarrelSpec spec = level.barrels[i];
                var go = new GameObject("Barrel");
                go.transform.SetParent(root, false);
                go.transform.position = Lift(new Vector3(spec.position.x, 0f, spec.position.z));
                var collider = go.AddComponent<SphereCollider>();
                collider.radius = 0.9f;
                collider.center = new Vector3(0f, 0.3f, 0f);
                collider.sharedMaterial = theme.physics;
                go.AddComponent<DamageSource>().multiplier = 0f;
                var floater = go.AddComponent<WaterFloater>();
                floater.heightOffset = -0.15f;
                floater.tiltSampleDistance = 0.6f;
                var barrel = go.AddComponent<Barrel>();
                barrel.index = i;
                GameObject visual = CreateSculpt("Visual", go.transform, theme.barrelMesh, theme.sculptMaterial, Vector3.zero, Vector3.one);
                visual.transform.localRotation = Quaternion.Euler(0f, i * 57f, 0f);
                barrel.visual = visual.transform;
                barrel.burst = EffectFactory.CreateSplash(go.transform, theme.splash);
                var hazard = go.AddComponent<SoftHazard>();
                hazard.radius = 2.2f;
                hazard.weight = 1f;
            }
        }

        static void BuildRamps(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.ramps == null || level.ramps.Length == 0)
            {
                return;
            }
            var root = new GameObject("Ramps").transform;
            root.SetParent(parent, false);
            foreach (RampSpec spec in level.ramps)
            {
                var go = new GameObject("Ramp");
                go.transform.SetParent(root, false);
                go.transform.SetPositionAndRotation(Lift(new Vector3(spec.position.x, 0f, spec.position.z)), Quaternion.Euler(0f, spec.yaw, 0f));
                var launch = go.AddComponent<LaunchRamp>();

                float angle = Mathf.Atan2(RampRise, RampLength) * Mathf.Rad2Deg;
                float slopeLength = Mathf.Sqrt(RampLength * RampLength + RampRise * RampRise);
                var deckMaterial = new Material(theme.finish) { name = "RampDeck" };
                deckMaterial.SetColor("_BaseColor", theme.rampColor);
                deckMaterial.SetTexture("_BaseMap", ProceduralTextures.Checker);
                deckMaterial.SetTextureScale("_BaseMap", new Vector2(2f, Mathf.Round(slopeLength / 1.2f)));
                var sideMaterial = new Material(theme.finish) { name = "RampSide" };
                sideMaterial.SetColor("_BaseColor", theme.rampStripe);
                GameObject slope = CreateVisual(PrimitiveType.Cube, "Slope", go.transform, deckMaterial);
                slope.transform.localPosition = new Vector3(0f, -0.35f + RampRise * 0.5f - 0.2f, 0f);
                slope.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                slope.transform.localScale = new Vector3(spec.width, 0.4f, slopeLength);
                GameObject back = CreateVisual(PrimitiveType.Cube, "Back", go.transform, sideMaterial);
                back.transform.localPosition = new Vector3(0f, (RampRise - 0.35f - 1.2f) * 0.5f - 0.2f, RampLength * 0.5f - 0.3f);
                back.transform.localScale = new Vector3(spec.width, RampRise - 0.35f + 1.2f, 0.6f);
                for (int s = -1; s <= 1; s += 2)
                {
                    GameObject rail = CreateVisual(PrimitiveType.Cube, "Rail", go.transform, sideMaterial);
                    rail.transform.localPosition = new Vector3(s * (spec.width * 0.5f + 0.1f), -0.35f + RampRise * 0.5f + 0.15f, 0f);
                    rail.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                    rail.transform.localScale = new Vector3(0.22f, 0.7f, slopeLength);
                }
                var deck = new GameObject("Deck");
                deck.transform.SetParent(go.transform, false);
                deck.transform.localPosition = new Vector3(0f, -0.35f + RampRise * 0.5f, 0f);
                deck.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                var box = deck.AddComponent<BoxCollider>();
                box.size = new Vector3(spec.width, 0.5f, Mathf.Sqrt(RampLength * RampLength + RampRise * RampRise));
                box.center = new Vector3(0f, -0.25f, 0f);
                box.sharedMaterial = theme.physics;

                var crest = new GameObject("Crest");
                crest.transform.SetParent(go.transform, false);
                crest.transform.localPosition = new Vector3(0f, RampRise, RampLength * 0.5f - 0.6f);
                var trigger = crest.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(spec.width, 2.5f, 1.2f);
                launch.boost = 3.5f;
                launch.lift = 3f;
            }
        }

        static void BuildIcicles(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.icicles == null || level.icicles.Length == 0 || theme.icicleMesh == null)
            {
                return;
            }
            var root = new GameObject("Icicles").transform;
            root.SetParent(parent, false);
            foreach (IcicleSpec spec in level.icicles)
            {
                var go = new GameObject("IcicleDrop");
                go.transform.SetParent(root, false);
                go.transform.position = Lift(new Vector3(spec.position.x, 0f, spec.position.z));
                var drop = go.AddComponent<IcicleDrop>();
                drop.period = spec.period;
                drop.phase = spec.phase;
                GameObject icicle = CreateSculpt("Icicle", go.transform, theme.icicleMesh, theme.iceMaterial != null ? theme.iceMaterial : theme.sculptMaterial, new Vector3(0f, drop.height, 0f), Vector3.one);
                drop.icicle = icicle.transform;
                GameObject shadow = CreateVisual(PrimitiveType.Cylinder, "Shadow", go.transform, theme.accent);
                shadow.transform.localPosition = new Vector3(0f, 0.04f, 0f);
                shadow.transform.localScale = new Vector3(drop.radius * 0.6f, 0.02f, drop.radius * 0.6f);
                shadow.SetActive(false);
                shadow.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                drop.shadow = shadow.transform;
                drop.splash = EffectFactory.CreateSplash(go.transform, theme.splash);
                var hazard = go.AddComponent<SoftHazard>();
                hazard.radius = drop.radius + 0.6f;
                hazard.weight = 0.7f;
            }
        }
    }
}
