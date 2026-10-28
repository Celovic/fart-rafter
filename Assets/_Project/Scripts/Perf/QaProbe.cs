using System;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FartRafters
{
    public class QaProbe : MonoBehaviour
    {
        static bool enabledByArgs;
        GameBootstrap game;
        float nextSample;
        float sceneStart;
        bool visualChecked;
        int frames;
        float frameTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg == "-qalog")
                {
                    enabledByArgs = true;
                }
            }
            if (!enabledByArgs)
            {
                return;
            }
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.logMessageReceived += OnLog;
            var go = new GameObject("QaProbe");
            DontDestroyOnLoad(go);
            go.AddComponent<QaProbe>();
        }

        static void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                string first = string.IsNullOrEmpty(stack) ? "" : stack.Split('\n')[0];
                Debug.Log("QA ERR " + type + " | " + message.Replace('\n', ' ') + " | " + first);
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLoaded;
        }

        void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            game = FindAnyObjectByType<GameBootstrap>();
            sceneStart = Time.unscaledTime;
            visualChecked = false;
            nextSample = Time.unscaledTime + 1f;
            Debug.Log("QA SCENE " + scene.name);
            if (game == null)
            {
                return;
            }
            var text = new StringBuilder("QA START");
            text.Append(" biome=").Append(game.Biome);
            text.Append(" adventure=").Append(game.IsAdventure);
            text.Append(" diff=").Append(game.IsAdventure ? GameSelection.DifficultyIndex : -1);
            text.Append(" players=").Append(game.PlayerCount);
            text.Append(" raft=").Append(GameSelection.RaftIndex);
            RaftVariant variant = game.catalog != null ? game.catalog.GetRaft(GameSelection.RaftIndex) : null;
            text.Append(" raftName=").Append(variant != null ? variant.name : "null");
            foreach (Rider rider in game.seats)
            {
                if (rider == null || !rider.gameObject.activeSelf)
                {
                    continue;
                }
                text.Append(" seat=").Append(rider.avatar != null ? rider.avatar.name : "null").Append('/').Append(rider.Costume).Append(rider.IsBot ? "/bot" : "/player");
            }
            Debug.Log(text.ToString());
        }

        void Update()
        {
            frames++;
            frameTime += Time.unscaledDeltaTime;
            if (game == null || Time.unscaledTime < nextSample)
            {
                return;
            }
            nextSample = Time.unscaledTime + 1f;
            if (!visualChecked && Time.unscaledTime - sceneStart > 2f)
            {
                visualChecked = true;
                CheckVisuals();
            }
            Sample();
        }

        void Sample()
        {
            Rider player = game.PlayerRiders != null && game.PlayerRiders.Length > 0 ? game.PlayerRiders[0] : null;
            Raft raft = game.raft;
            var text = new StringBuilder("QA T");
            text.Append(" t=").Append((Time.unscaledTime - sceneStart).ToString("0", CultureInfo.InvariantCulture));
            text.Append(" fps=").Append(frameTime > 0f ? (frames / frameTime).ToString("0", CultureInfo.InvariantCulture) : "0");
            frames = 0;
            frameTime = 0f;
            text.Append(" tut=").Append(TutorialGuide.Active ? 1 : 0);
            text.Append(" ts=").Append(Time.timeScale.ToString("0.##", CultureInfo.InvariantCulture));
            if (game.course != null)
            {
                text.Append(" run=").Append(game.course.Running ? 1 : 0);
                text.Append(" fin=").Append(game.course.Finished ? 1 : 0);
                text.Append(" cp=").Append(game.course.LastCheckpoint);
                text.Append(" wp=").Append(game.course.WaypointIndex);
                text.Append(" resp=").Append(game.course.RespawnCount);
                text.Append(" nudge=").Append(RaftUnstick.Nudges);
            }
            if (raft != null && raft.Body != null)
            {
                Vector3 p = raft.Body.position;
                Vector3 v = raft.Body.linearVelocity;
                text.Append(" pos=").Append(p.x.ToString("0.0", CultureInfo.InvariantCulture)).Append(":").Append(p.y.ToString("0.0", CultureInfo.InvariantCulture)).Append(":").Append(p.z.ToString("0.0", CultureInfo.InvariantCulture));
                text.Append(" vel=").Append(v.x.ToString("0.0", CultureInfo.InvariantCulture)).Append(":").Append(v.y.ToString("0.0", CultureInfo.InvariantCulture)).Append(":").Append(v.z.ToString("0.0", CultureInfo.InvariantCulture));
                if (float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z))
                {
                    text.Append(" NAN");
                }
                RaftHealth health = raft.GetComponent<RaftHealth>();
                if (health != null)
                {
                    text.Append(" hp=").Append(health.Health.ToString("0", CultureInfo.InvariantCulture));
                }
            }
            if (player != null)
            {
                Vector3 thrust = player.LastThrust;
                text.Append(" pfire=").Append(player.IsFiring ? 1 : 0);
                text.Append(" pthrust=").Append(thrust.x.ToString("0", CultureInfo.InvariantCulture)).Append(":").Append(thrust.z.ToString("0", CultureInfo.InvariantCulture));
                text.Append(" pwater=").Append(player.InWater ? 1 : 0);
                text.Append(" pauto=").Append(player.Autopilot ? 1 : 0);
                text.Append(" pgas=").Append(player.Gas.Normalized.ToString("0.00", CultureInfo.InvariantCulture));
            }
            if (game.ActivePlayerBrains != null && game.ActivePlayerBrains.Length > 0 && game.ActivePlayerBrains[0] != null)
            {
                PlayerBrain brain = game.ActivePlayerBrains[0];
                text.Append(" stick=").Append(brain.JoystickActive ? 1 : 0).Append(" held=").Append(brain.GasHeld ? 1 : 0);
            }
            if (Mathf.RoundToInt(Time.unscaledTime - sceneStart) % 5 == 0)
            {
                int visible = 0;
                long triangles = 0;
                int particles = 0;
                foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    if (!renderer.isVisible)
                    {
                        continue;
                    }
                    if (renderer is ParticleSystemRenderer)
                    {
                        particles++;
                        continue;
                    }
                    visible++;
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null)
                    {
                        for (int m = 0; m < filter.sharedMesh.subMeshCount; m++)
                        {
                            triangles += (long)filter.sharedMesh.GetIndexCount(m) / 3;
                        }
                    }
                }
                text.Append(" vis=").Append(visible).Append(" tris=").Append(triangles / 1000).Append("k ps=").Append(particles);
            }
            Debug.Log(text.ToString());
        }

        void CheckVisuals()
        {
            foreach (Rider rider in game.seats)
            {
                if (rider == null || !rider.gameObject.activeSelf)
                {
                    continue;
                }
                CheckHierarchy("rider " + (rider.avatar != null ? rider.avatar.name : "null") + "/" + rider.Costume, rider.transform, rider.transform.position, 2.5f);
            }
            if (game.raft != null)
            {
                CheckHierarchy("raft", game.raft.transform, game.raft.transform.position, 4f);
            }
            int unsupported = 0;
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null || !material.shader.isSupported)
                    {
                        unsupported++;
                        if (unsupported <= 5)
                        {
                            Debug.Log("QA VIS badmat " + renderer.name + " " + (material == null ? "null" : material.shader == null ? material.name + ":noshader" : material.name + ":" + material.shader.name));
                        }
                    }
                }
            }
            Debug.Log("QA VIS done unsupported=" + unsupported);
        }

        void CheckHierarchy(string label, Transform root, Vector3 center, float reach)
        {
            int renderers = 0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is ParticleSystemRenderer)
                {
                    continue;
                }
                renderers++;
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }
                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                if (renderer is MeshRenderer && (filter == null || filter.sharedMesh == null || filter.sharedMesh.vertexCount == 0))
                {
                    Debug.Log("QA VIS " + label + " emptymesh " + Path(renderer.transform, root));
                    continue;
                }
                Bounds bounds = renderer.bounds;
                float distance = Vector3.Distance(bounds.center, center);
                if (distance > reach || bounds.size.magnitude > reach * 3f || bounds.size.magnitude < 0.005f)
                {
                    Debug.Log("QA VIS " + label + " oddbounds " + Path(renderer.transform, root) + " dist=" + distance.ToString("0.00", CultureInfo.InvariantCulture) + " size=" + bounds.size.ToString("0.00", CultureInfo.InvariantCulture));
                }
                if (renderer.sharedMaterial == null)
                {
                    Debug.Log("QA VIS " + label + " nomat " + Path(renderer.transform, root));
                }
            }
            Debug.Log("QA VIS " + label + " renderers=" + renderers);
        }

        static string Path(Transform node, Transform root)
        {
            string path = node.name;
            while (node.parent != null && node != root)
            {
                node = node.parent;
                path = node.name + "/" + path;
            }
            return path;
        }
    }
}
