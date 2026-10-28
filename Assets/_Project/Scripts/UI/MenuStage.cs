using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public class MenuStage : MonoBehaviour
    {
        public CourseTheme theme;
        public CourseTheme[] themes;
        public GameCatalog catalog;
        public Camera viewCamera;
        public Vector3 cameraPosition = new Vector3(0f, 5.5f, -13f);
        public Vector3 lookTarget = new Vector3(0f, 1f, 0f);
        [Min(0f)] public float swayDegrees = 8f;
        [Min(1f)] public float previewDistance = 7f;
        [Min(0.1f)] public float previewScale = 1.25f;
        [Min(0.1f)] public float demoDuration = 0.7f;

        static readonly Vector3[] SeatPositions =
        {
            new Vector3(-0.75f, 0.2f, -0.75f), new Vector3(0.75f, 0.2f, -0.75f),
            new Vector3(-0.75f, 0.2f, 0.75f), new Vector3(0.75f, 0.2f, 0.75f)
        };

        static readonly Vector3[] Islands =
        {
            new Vector3(-17f, 0f, 14f), new Vector3(19f, 0f, 21f), new Vector3(-3f, 0f, 36f)
        };

        static readonly float[] IslandRadii = { 7f, 9f, 6f };

        bool built;
        Transform[] seats;
        ParticleSystem[] seatPuffs;
        AvatarRig[] seatRigs;
        float[] seatDemoUntil;
        Transform[] previews;
        ParticleSystem[] previewPuffs;
        AvatarRig[] previewRigs;
        float[] previewDemoUntil;
        RectTransform[] anchors;
        bool previewsVisible;
        int activeCount = GameSelection.MinPlayers;
        float nextSeatDemo;
        readonly Dictionary<Biome, Transform> worlds = new Dictionary<Biome, Transform>();
        Biome shownBiome;
        Material baseSky;
        Color baseFog;
        float baseFogDensity;
        Color baseSun;
        float baseSunIntensity;
        Quaternion baseSunRotation;
        VolumeProfile baseProfile;
        int[] lineup;

        void Start()
        {
            EnsureBuilt();
        }

        public void SetAvatar(int player, AvatarDefinition avatar, bool demo)
        {
            SetAvatar(player, avatar, demo, GameSelection.GetCostume(player));
        }

        public void SetAvatar(int player, AvatarDefinition avatar, bool demo, int costume)
        {
            EnsureBuilt();
            if (avatar == null || player < 0 || player >= previews.Length)
            {
                return;
            }

            previewRigs[player] = Dress(previews[player], previewPuffs[player], avatar, false, costume);
            if (player < seats.Length && player < activeCount)
            {
                seatRigs[player] = Dress(seats[player], seatPuffs[player], avatar, true, costume);
            }
            if (demo)
            {
                previewDemoUntil[player] = Time.time + demoDuration;
                GameAudio.PlayFart(avatar, 0.8f, true);
            }
        }

        public void ShowPreviews(bool visible, RectTransform[] slotAnchors)
        {
            EnsureBuilt();
            previewsVisible = visible;
            anchors = slotAnchors;
            RefreshPreviewVisibility();
        }

        public bool ShowingLineup => lineup != null;

        public void PlayEmote(int seat, int move)
        {
            if (!built || seat < 0 || seat >= seatRigs.Length || seatRigs[seat] == null)
            {
                return;
            }
            seatRigs[seat].PlayEmote(move);
        }

        public void SetLineup(int[] codes)
        {
            EnsureBuilt();
            if (!built || codes == null || codes.Length == 0)
            {
                return;
            }
            if (lineup != null && System.Linq.Enumerable.SequenceEqual(lineup, codes))
            {
                return;
            }
            lineup = (int[])codes.Clone();
            for (int seat = 0; seat < seats.Length; seat++)
            {
                bool filled = seat < codes.Length;
                seats[seat].gameObject.SetActive(filled);
                seatRigs[seat] = filled ? Dress(seats[seat], seatPuffs[seat], catalog.GetAvatar(codes[seat] % AvatarDefinition.CostumeCode), true, codes[seat] / AvatarDefinition.CostumeCode) : null;
            }
        }

        public void SetCrew(int players, int bots)
        {
            EnsureBuilt();
            if (!built)
            {
                return;
            }
            lineup = null;
            activeCount = Mathf.Clamp(players, GameSelection.MinPlayers, previews.Length);
            int crew = Mathf.Min(seats.Length, activeCount + bots);
            for (int seat = 0; seat < seats.Length; seat++)
            {
                bool filled = seat < crew;
                seats[seat].gameObject.SetActive(filled);
                if (!filled)
                {
                    seatRigs[seat] = null;
                    continue;
                }
                AvatarDefinition avatar = seat < activeCount ? catalog.GetAvatar(GameSelection.GetAvatar(seat)) : catalog.GetAvatar(Random.Range(0, catalog.avatars.Length));
                seatRigs[seat] = Dress(seats[seat], seatPuffs[seat], avatar, true, seat < activeCount ? GameSelection.GetCostume(seat) : 0);
            }
            float scale = previewScale * (activeCount > 2 ? 0.8f : 1f);
            foreach (Transform preview in previews)
            {
                preview.localScale = Vector3.one * scale;
            }
            RefreshPreviewVisibility();
        }

        void RefreshPreviewVisibility()
        {
            for (int i = 0; i < previews.Length; i++)
            {
                previews[i].gameObject.SetActive(previewsVisible && i < activeCount);
            }
        }

        void EnsureBuilt()
        {
            if (built || theme == null || catalog == null)
            {
                return;
            }
            built = true;
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }

            CaptureLook();
            shownBiome = theme.biome;
            worlds[shownBiome] = BuildWorld(theme);
            BuildRaft();

            int players = GameSelection.MaxPlayers;
            previews = new Transform[players];
            previewPuffs = new ParticleSystem[players];
            previewRigs = new AvatarRig[players];
            previewDemoUntil = new float[players];
            for (int i = 0; i < players; i++)
            {
                previews[i] = new GameObject("Preview_" + i).transform;
                previews[i].SetParent(transform, false);
                previewPuffs[i] = EffectFactory.CreatePuff(previews[i], theme.puff);
                previews[i].gameObject.SetActive(false);
                previewRigs[i] = Dress(previews[i], previewPuffs[i], catalog.GetAvatar(GameSelection.GetAvatar(i)), false, GameSelection.GetCostume(i));
            }
            SetCrew(GameSelection.PlayerCount, GameSelection.BotCount);
        }

        public void SetBiome(int choice)
        {
            EnsureBuilt();
            if (!built)
            {
                return;
            }
            Biome biome = choice >= 0 && choice < Biomes.Count ? (Biome)choice : theme.biome;
            if (biome == shownBiome)
            {
                return;
            }
            CourseTheme look = ThemeFor(biome);
            if (!worlds.TryGetValue(biome, out Transform world) || world == null)
            {
                world = BuildWorld(look);
                worlds[biome] = world;
            }
            foreach (KeyValuePair<Biome, Transform> pair in worlds)
            {
                if (pair.Value != null)
                {
                    pair.Value.gameObject.SetActive(pair.Key == biome);
                }
            }
            shownBiome = biome;
            RestoreLook();
            if (look != theme)
            {
                BiomeEnvironment.ApplyLook(look);
            }
        }

        CourseTheme ThemeFor(Biome biome)
        {
            if (themes != null)
            {
                foreach (CourseTheme candidate in themes)
                {
                    if (candidate != null && candidate.biome == biome)
                    {
                        return candidate;
                    }
                }
            }
            return theme;
        }

        void CaptureLook()
        {
            baseSky = RenderSettings.skybox;
            baseFog = RenderSettings.fogColor;
            baseFogDensity = RenderSettings.fogDensity;
            Light sun = BiomeEnvironment.Sun();
            if (sun != null)
            {
                baseSun = sun.color;
                baseSunIntensity = sun.intensity;
                baseSunRotation = sun.transform.rotation;
            }
            Volume volume = BiomeEnvironment.GlobalVolume();
            baseProfile = volume != null ? volume.sharedProfile : null;
        }

        void RestoreLook()
        {
            RenderSettings.skybox = baseSky;
            RenderSettings.fogColor = baseFog;
            RenderSettings.fogDensity = baseFogDensity;
            Light sun = BiomeEnvironment.Sun();
            if (sun != null)
            {
                sun.color = baseSun;
                sun.intensity = baseSunIntensity;
                sun.transform.rotation = baseSunRotation;
            }
            Volume volume = BiomeEnvironment.GlobalVolume();
            if (volume != null && baseProfile != null)
            {
                volume.sharedProfile = baseProfile;
            }
        }

        Transform BuildWorld(CourseTheme theme)
        {
            var root = new GameObject("World_" + theme.biome).transform;
            root.SetParent(transform, false);
            var waterRoot = new GameObject("Water");
            waterRoot.transform.SetParent(root, false);
            WaterSurface surface = waterRoot.AddComponent<WaterSurface>();
            surface.waveAmplitude = 0.12f;
            CourseBuilder.CreateWaterMesh(waterRoot.transform, new Vector3(0f, 0f, 12f), new Vector2(160f, 120f), theme, IslandShore);

            for (int i = 0; i < Islands.Length; i++)
            {
                LandscapeBuilder.BuildIsland(Islands[i], IslandRadii[i], theme, root, new System.Random(42 + i));
            }
            if (Application.isPlaying)
            {
                StartCoroutine(BuildLife(root, theme));
            }
            else
            {
                for (System.Collections.IEnumerator steps = BuildLife(root, theme); steps.MoveNext();)
                {
                }
            }
            return root;
        }

        System.Collections.IEnumerator BuildLife(Transform root, CourseTheme theme)
        {
            yield return null;
            if (root == null)
            {
                yield break;
            }
            var life = new GameObject("Life").transform;
            life.SetParent(root, false);

            int[] dancerIslands = { 0, 0, 1, 1, 2 };
            float[] dancerSides = { -1.3f, 1.3f, -1.5f, 1.5f, 0f };
            for (int i = 0; i < dancerIslands.Length; i++)
            {
                Vector3 island = Islands[dancerIslands[i]];
                Vector3 toCamera = Vector3.ProjectOnPlane(cameraPosition - island, Vector3.up).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, toCamera);
                Vector3 spot = island + toCamera * (IslandRadii[dancerIslands[i]] - 1.7f) + side * dancerSides[i];
                var holder = new GameObject("Dancer_" + i).transform;
                holder.SetParent(life, false);
                holder.position = new Vector3(spot.x, LandscapeBuilder.LandHeight, spot.z);
                holder.localScale = Vector3.one * 1.5f;
                AvatarRig rig = AvatarVisualFactory.Build(holder, catalog.GetAvatar(4 + i), theme);
                rig.Celebrate();
            }

            yield return null;
            for (int i = 0; i < Islands.Length && theme.predatorSpin <= 0f; i++)
            {
                GameObject croc = CourseBuilder.CreateSculpt("Crocodile", life, theme.crocodileMesh, theme.sculptMaterial, Vector3.zero, PredatorScale(theme, new Vector3(1.4f, 0.9f, 3.4f)));
                var swimmer = croc.AddComponent<MenuSwimmer>();
                swimmer.center = Islands[i];
                swimmer.radius = IslandRadii[i] + 3.5f;
                swimmer.speed = (i % 2 == 0 ? 1f : -1f) * 0.09f;
            }

            for (int i = 0; i < 5 && Calm(theme); i++)
            {
                int island = i % Islands.Length;
                float angle = 3.6f + i * 0.9f;
                Vector3 position = Islands[island] + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (IslandRadii[island] + 1.6f);
                GameObject pad = CourseBuilder.CreateSculpt("LilyPad", life, theme.lilyPadMesh, theme.sculptMaterial, Vector3.zero, Vector3.one);
                pad.transform.SetPositionAndRotation(new Vector3(position.x, 0.02f, position.z), Quaternion.Euler(0f, i * 70f, 0f));
                pad.AddComponent<WaterFloater>().heightOffset = 0.02f;
                if (i % 2 == 0 || i == 3)
                {
                    var frogHolder = new GameObject("Frog").transform;
                    frogHolder.SetParent(pad.transform, false);
                    CourseBuilder.CreateSculpt("FrogBody", frogHolder, theme.frogMesh, theme.sculptMaterial, Vector3.zero, Vector3.one);
                    pad.AddComponent<MenuHopper>().body = frogHolder;
                }
            }

            yield return null;
            BuildWildlife(life, theme);
            yield return null;

            for (int i = 0; i < 9; i++)
            {
                var bird = new GameObject("Bird").transform;
                bird.SetParent(life, false);
                CourseBuilder.CreateSculpt("Body", bird, theme.birdBodyMesh, theme.sculptMaterial, Vector3.zero, Vector3.one);
                var flyer = bird.gameObject.AddComponent<MenuFlyer>();
                flyer.leftWing = CourseBuilder.CreateSculpt("WingLeft", bird, theme.birdWingMesh, theme.sculptMaterial, new Vector3(-0.08f, 0.05f, 0f), Vector3.one).transform;
                flyer.rightWing = CourseBuilder.CreateSculpt("WingRight", bird, theme.birdWingMesh, theme.sculptMaterial, new Vector3(0.08f, 0.05f, 0f), Vector3.one).transform;
                bool flock = i >= 4;
                flyer.center = flock ? new Vector3(-6f + (i - 4) * 0.9f, 0f, 30f + (i % 2) * 1.2f) : new Vector3(0f, 0f, 22f);
                flyer.radius = flock ? 20f : 14f + i * 4f;
                flyer.height = flock ? 12f + (i % 3) * 0.6f : 9f + i * 1.5f;
                flyer.speed = flock ? 0.2f : (i % 2 == 0 ? 1f : -1f) * (0.22f + i * 0.03f);
                bird.localScale = Vector3.one * (flock ? 1.2f : 1.6f);
            }
        }

        void BuildWildlife(Transform life, CourseTheme theme)
        {
            ParticleSystem splash = EffectFactory.CreateSplash(life, theme.splash);

            for (int i = 0; i < 2 && theme.predatorSpin <= 0f; i++)
            {
                GameObject croc = CourseBuilder.CreateSculpt("Crocodile", life, theme.crocodileMesh, theme.sculptMaterial, Vector3.zero, PredatorScale(theme, new Vector3(1.3f, 0.9f, 3.1f)));
                var swimmer = croc.AddComponent<MenuSwimmer>();
                swimmer.center = new Vector3(0f, 0f, 6f);
                swimmer.radius = 9f + i * 5f;
                swimmer.speed = (i == 0 ? -1f : 1f) * 0.06f;
                swimmer.depth = -0.32f;
            }

            for (int i = 0; i < 7 && Calm(theme); i++)
            {
                GameObject fish = CourseBuilder.CreateSculpt("Fish", life, theme.fishMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * (1.1f + (i % 3) * 0.25f));
                var jumper = fish.AddComponent<MenuJumper>();
                jumper.center = new Vector3(-12f + i * 4f, 0f, 4f + (i % 3) * 6f);
                jumper.radius = 6f;
                jumper.height = 1.1f + (i % 3) * 0.4f;
                jumper.length = 2.5f + (i % 2);
                jumper.splash = splash;
            }

            for (int family = 0; family < 2 && Calm(theme); family++)
            {
                Vector3 island = Islands[family * 2];
                float radius = IslandRadii[family * 2] + 2.4f;
                for (int i = 0; i < 4; i++)
                {
                    GameObject duck = CourseBuilder.CreateSculpt("Duck", life, theme.duckMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * (i == 0 ? 1.3f : 0.7f));
                    var paddler = duck.AddComponent<MenuPaddler>();
                    paddler.center = island;
                    paddler.radius = radius;
                    paddler.speed = (family == 0 ? 1f : -1f) * 0.07f;
                    paddler.lag = i * 0.09f * (family == 0 ? 1f : -1f);
                }
            }

            for (int i = 0; i < 7 && Calm(theme); i++)
            {
                int island = i % Islands.Length;
                float angle = 0.5f + i * 1.7f;
                Vector3 position = Islands[island] + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (IslandRadii[island] + 2.6f);
                GameObject pad = CourseBuilder.CreateSculpt("LilyPad", life, theme.lilyPadMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * 0.9f);
                pad.transform.SetPositionAndRotation(new Vector3(position.x, 0.02f, position.z), Quaternion.Euler(0f, i * 53f, 0f));
                pad.AddComponent<WaterFloater>().heightOffset = 0.02f;
                var frogHolder = new GameObject("Frog").transform;
                frogHolder.SetParent(pad.transform, false);
                CourseBuilder.CreateSculpt("FrogBody", frogHolder, theme.frogMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * 0.9f);
                pad.AddComponent<MenuHopper>().body = frogHolder;
            }

            for (int i = 0; i < 8; i++)
            {
                int island = i % Islands.Length;
                var butterfly = new GameObject("Butterfly").transform;
                butterfly.SetParent(life, false);
                var flutter = butterfly.gameObject.AddComponent<MenuFlutter>();
                flutter.center = Islands[island] + Vector3.up * LandscapeBuilder.LandHeight;
                flutter.radius = IslandRadii[island] * 0.6f;
                flutter.height = 1.2f + (i % 3) * 0.5f;
                flutter.leftWing = CourseBuilder.CreateSculpt("WingLeft", butterfly, theme.butterflyWingMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * 1.4f).transform;
                flutter.rightWing = CourseBuilder.CreateSculpt("WingRight", butterfly, theme.butterflyWingMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * 1.4f).transform;
            }

            for (int i = 0; i < 3 && Calm(theme); i++)
            {
                Vector3 island = Islands[i];
                Vector3 toCamera = Vector3.ProjectOnPlane(cameraPosition - island, Vector3.up).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, toCamera);
                Vector3 spot = island + toCamera * (IslandRadii[i] + 0.4f) + side * (i - 1) * 2.5f;
                GameObject turtle = CourseBuilder.CreateSculpt("Turtle", life, theme.turtleMesh, theme.sculptMaterial, Vector3.zero, Vector3.one * 1.3f);
                turtle.transform.SetPositionAndRotation(new Vector3(spot.x, 0.05f, spot.z), Quaternion.LookRotation(-side, Vector3.up));
                turtle.AddComponent<WaterFloater>().heightOffset = 0.03f;
            }

            if (theme.spectators != null && theme.spectators.Length > 0)
            {
                var random = new System.Random(11);
                CrowdMood[] moods = { CrowdMood.Fans, CrowdMood.Foodies, CrowdMood.Fans };
                for (int i = 0; i < Islands.Length; i++)
                {
                    Vector3 island = Islands[i];
                    Vector3 toCamera = Vector3.ProjectOnPlane(cameraPosition - island, Vector3.up).normalized;
                    Vector3 side = Vector3.Cross(Vector3.up, toCamera);
                    var crowd = new GameObject("Crowd").transform;
                    crowd.SetParent(life, false);
                    crowd.position = island + toCamera * (IslandRadii[i] - 3.2f) - side * 1.5f + Vector3.up * LandscapeBuilder.LandHeight;
                    var group = crowd.gameObject.AddComponent<SpectatorGroup>();
                    group.theme = theme;
                    group.mood = moods[i];
                    group.reactDistance = 60f;
                    group.volume = 0.2f;
                    SpectatorStreamer.Populate(group, random, side, toCamera, 3);
                    foreach (Spectator member in group.members)
                    {
                        member.transform.localScale *= 1.4f;
                    }
                }
            }
        }

        static bool Calm(CourseTheme theme)
        {
            return theme.biome != Biome.Lava;
        }

        static Vector3 PredatorScale(CourseTheme theme, Vector3 crocodile)
        {
            return theme.predatorLength > 0f ? Vector3.one * 0.6f : crocodile;
        }

        static float IslandShore(Vector3 point)
        {
            float shore = 0f;
            for (int i = 0; i < Islands.Length; i++)
            {
                Vector3 offset = point - Islands[i];
                offset.y = 0f;
                shore = Mathf.Max(shore, 1f - (offset.magnitude - IslandRadii[i]) / 3f);
            }
            return shore;
        }

        Transform menuRaft;

        public void SetRaft(RaftVariant variant)
        {
            EnsureBuilt();
            RaftVisualFactory.ApplyVariant(menuRaft, variant);
        }

        void BuildRaft()
        {
            var raft = new GameObject("MenuRaft").transform;
            menuRaft = raft;
            raft.SetParent(transform, false);
            raft.rotation = Quaternion.Euler(0f, 155f, 0f);
            RaftVisualFactory.Build(raft, theme);
            var floater = raft.gameObject.AddComponent<WaterFloater>();
            floater.heightOffset = 0.1f;
            floater.tiltSampleDistance = 1.3f;

            seats = new Transform[SeatPositions.Length];
            seatPuffs = new ParticleSystem[seats.Length];
            seatRigs = new AvatarRig[seats.Length];
            seatDemoUntil = new float[seats.Length];
            for (int i = 0; i < seats.Length; i++)
            {
                seats[i] = new GameObject("Seat_" + i).transform;
                seats[i].SetParent(raft, false);
                seats[i].localPosition = SeatPositions[i];
                seats[i].localRotation = Quaternion.Euler(0f, Random.Range(-40f, 40f), 0f);
                seatPuffs[i] = EffectFactory.CreatePuff(seats[i], theme.puff);
            }
            nextSeatDemo = Time.time + Random.Range(2f, 4f);
        }

        AvatarRig Dress(Transform holder, ParticleSystem puff, AvatarDefinition avatar, bool castShadows, int costume = 0)
        {
            if (avatar == null)
            {
                return null;
            }
            Transform old = holder.Find(AvatarVisualFactory.VisualName);
            if (old != null)
            {
                Destroy(old.gameObject);
                old.name = "Old";
            }
            AvatarRig rig = AvatarVisualFactory.Build(holder, avatar, theme, costume);
            rig.puff = puff;
            if (puff != null)
            {
                puff.transform.localPosition = AvatarVisualFactory.PuffPosition(avatar);
            }
            if (!castShadows)
            {
                foreach (Renderer renderer in rig.GetComponentsInChildren<Renderer>())
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            return rig;
        }

        void Update()
        {
            if (!built)
            {
                return;
            }

            float now = Time.time;
            if (viewCamera != null)
            {
                float yaw = Mathf.Sin(now * 0.15f) * swayDegrees;
                viewCamera.transform.position = lookTarget + Quaternion.Euler(0f, yaw, 0f) * (cameraPosition - lookTarget);
                viewCamera.transform.LookAt(lookTarget);
            }

            if (now >= nextSeatDemo)
            {
                seatDemoUntil[Random.Range(0, seats.Length)] = now + Random.Range(0.4f, 0.9f);
                nextSeatDemo = now + Random.Range(2f, 5f);
            }
            for (int i = 0; i < seatRigs.Length; i++)
            {
                if (seatRigs[i] != null)
                {
                    seatRigs[i].demoFiring = now < seatDemoUntil[i];
                }
            }

            if (previewsVisible && viewCamera != null && anchors != null)
            {
                PlacePreviews(now);
            }
        }

        void PlacePreviews(float now)
        {
            Transform cameraTransform = viewCamera.transform;
            Vector3 flatForward = Vector3.ProjectOnPlane(-cameraTransform.forward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.0001f)
            {
                flatForward = Vector3.back;
            }

            for (int i = 0; i < activeCount && i < previews.Length && i < anchors.Length; i++)
            {
                RectTransform anchor = anchors[i];
                if (anchor == null)
                {
                    continue;
                }
                Vector3 screen = RectTransformUtility.WorldToScreenPoint(null, anchor.TransformPoint(anchor.rect.center));
                Vector3 world = viewCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, previewDistance));
                previews[i].position = world - cameraTransform.up * (0.75f * previews[i].localScale.y);
                previews[i].rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up) * Quaternion.Euler(0f, Mathf.Sin(now * 0.8f + i * 2f) * 25f, 0f);
                if (previewRigs[i] != null)
                {
                    previewRigs[i].demoFiring = now < previewDemoUntil[i];
                }
            }
        }
    }
}
