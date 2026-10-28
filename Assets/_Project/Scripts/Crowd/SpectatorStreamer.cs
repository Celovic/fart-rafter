using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public class SpectatorStreamer : MonoBehaviour
    {
        struct Spot
        {
            public Vector3 position;
            public Vector3 along;
            public Vector3 facing;
            public int seed;
        }

        public LevelDefinition level;
        public CourseTheme theme;
        [Min(10f)] public float buildRange = 70f;
        [Min(10f)] public float keepRange = 110f;
        [Min(0.05f)] public float interval = 0.3f;

        readonly List<Spot> spots = new List<Spot>();
        SpectatorGroup[] groups;
        float nextCheck;

        void Start()
        {
            if (level == null || theme == null || theme.spectators == null || theme.spectators.Length == 0 || level.NodeCount < 4)
            {
                enabled = false;
                return;
            }
            var random = new System.Random(CourseGeometry.StableHash(level.id) * 17 + 3);
            int last = Mathf.Max(level.startNode + 3, level.FinishNode - 1);
            for (int node = level.startNode + 2; node < last; node += 2 + random.Next(3))
            {
                float side = random.NextDouble() < 0.5 ? -1f : 1f;
                if (theme.openSide != 0)
                {
                    side = -theme.openSide;
                    if (level.IsThorn(Mathf.Min(node, level.NodeCount - 2), side))
                    {
                        continue;
                    }
                }
                else if (level.IsThorn(Mathf.Min(node, level.NodeCount - 2), side))
                {
                    side = -side;
                    if (level.IsThorn(Mathf.Min(node, level.NodeCount - 2), side))
                    {
                        continue;
                    }
                }
                Vector3 direction = level.Direction(node);
                Vector3 outward = Vector3.Cross(Vector3.up, direction).normalized * side;
                Vector3 position = level.Edge(node, side) + outward * (1.9f + (float)random.NextDouble() * 0.8f);
                if (CourseGeometry.WallDistance(level, position) > -1.3f)
                {
                    continue;
                }
                position.y = LandscapeBuilder.GroundHeight(position);
                spots.Add(new Spot { position = position, along = direction, facing = -outward, seed = random.Next() });
            }
            groups = new SpectatorGroup[spots.Count];
        }

        void Update()
        {
            if (Time.time < nextCheck)
            {
                return;
            }
            nextCheck = Time.time + interval;
            Vector3 focus = Raft.Current != null ? Raft.Current.transform.position : Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            bool built = false;
            for (int i = 0; i < spots.Count; i++)
            {
                Vector3 offset = spots[i].position - focus;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (groups[i] == null && distance < buildRange && !built)
                {
                    groups[i] = BuildGroup(spots[i]);
                    built = true;
                }
                else if (groups[i] != null && distance > keepRange)
                {
                    Destroy(groups[i].gameObject);
                    groups[i] = null;
                }
            }
        }

        SpectatorGroup BuildGroup(Spot spot)
        {
            var random = new System.Random(spot.seed);
            var root = new GameObject("Crowd").transform;
            root.SetParent(transform, false);
            root.position = spot.position;
            var group = root.gameObject.AddComponent<SpectatorGroup>();
            group.theme = theme;
            double roll = random.NextDouble();
            group.mood = roll < 0.4 ? CrowdMood.Fans : roll < 0.65 ? CrowdMood.Hecklers : roll < 0.85 ? CrowdMood.Foodies : CrowdMood.Mixed;
            Populate(group, random, spot.along, spot.facing, 2 + random.Next(4));
            return group;
        }

        public static void Populate(SpectatorGroup group, System.Random random, Vector3 along, Vector3 facing, int count)
        {
            CourseTheme theme = group.theme;
            count = Mathf.Min(count, GraphicsQuality.CrowdPerGroup);
            for (int i = 0; i < count; i++)
            {
                SpectatorLook look = PickLook(theme, random);
                SpectatorAct act = PickAct(group.mood, random);
                bool jeer = group.mood == CrowdMood.Hecklers || (group.mood == CrowdMood.Mixed && random.NextDouble() < 0.4);
                Spectator spectator = SpectatorFactory.Build(group.transform, look, theme, act, SpectatorFactory.RandomSign(jeer, random, theme), random.Next(10));
                float spread = (i - (count - 1) * 0.5f) * 1.05f;
                Vector3 local = along * (spread + (float)(random.NextDouble() - 0.5) * 0.3f) - facing * ((float)random.NextDouble() * 0.7f);
                spectator.transform.position = group.transform.position + local;
                spectator.transform.rotation = Quaternion.LookRotation(facing, Vector3.up) * Quaternion.Euler(0f, (float)(random.NextDouble() - 0.5) * 30f, 0f);
                spectator.transform.localScale = Vector3.one * (look.animal ? 1f : 1.05f + (float)random.NextDouble() * 0.15f);
                group.members.Add(spectator);
            }
        }

        static SpectatorLook PickLook(CourseTheme theme, System.Random random)
        {
            bool animal = random.NextDouble() < theme.animalShare;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                SpectatorLook look = theme.spectators[random.Next(theme.spectators.Length)];
                if (look.animal == animal)
                {
                    return look;
                }
            }
            return theme.spectators[random.Next(theme.spectators.Length)];
        }

        static SpectatorAct PickAct(CrowdMood mood, System.Random random)
        {
            double roll = random.NextDouble();
            switch (mood)
            {
                case CrowdMood.Fans:
                    return roll < 0.35 ? SpectatorAct.Cheer : roll < 0.65 ? SpectatorAct.Clap : roll < 0.9 ? SpectatorAct.Sign : SpectatorAct.Eat;
                case CrowdMood.Hecklers:
                    return roll < 0.65 ? SpectatorAct.Boo : roll < 0.9 ? SpectatorAct.Sign : SpectatorAct.Eat;
                case CrowdMood.Foodies:
                    return roll < 0.75 ? SpectatorAct.Eat : SpectatorAct.Clap;
                default:
                    return (SpectatorAct)random.Next(5);
            }
        }
    }
}
