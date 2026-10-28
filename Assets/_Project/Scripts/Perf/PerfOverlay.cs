#if FR_PERF
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FartRafters
{
    public class PerfOverlay : MonoBehaviour
    {
        static readonly float[] Scales = { 1f, 0.85f, 0.7f, 0.55f };
        static readonly int[] Samples = { 4, 2, 1 };

        bool open;
        bool water = true;
        bool shadows = true;
        bool effects = true;
        bool crowd = true;
        bool decor = true;
        bool ground = true;
        bool avatars = true;
        int scale;
        int msaa;
        float shadowDistance;
        float elapsed;
        int frames;
        float worst;
        string label = "";
        GUIStyle text;
        GUIStyle button;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var go = new GameObject("PerfOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<PerfOverlay>();
        }

        static UniversalRenderPipelineAsset Asset => GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        void Start()
        {
            UniversalRenderPipelineAsset asset = Asset;
            if (asset != null)
            {
                shadowDistance = asset.shadowDistance;
                scale = System.Array.IndexOf(Scales, asset.renderScale);
                scale = scale < 0 ? 0 : scale;
                msaa = System.Array.IndexOf(Samples, asset.msaaSampleCount);
                msaa = msaa < 0 ? 0 : msaa;
            }
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => Reapply();
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            frames++;
            worst = Mathf.Max(worst, dt);
            if ((!water || !decor || !ground || !avatars) && Time.frameCount % 60 == 0)
            {
                ApplyRenderers();
            }
            if (elapsed >= 0.5f)
            {
                label = $"{frames / elapsed:F0} FPS · {elapsed / frames * 1000f:F0} ms · en kötü {worst * 1000f:F0} ms · {GraphicsQuality.LevelName} %{GraphicsQuality.Scale * 100f:F0} · {Screen.width}x{Screen.height}";
                elapsed = 0f;
                frames = 0;
                worst = 0f;
            }
        }

        void Reapply()
        {
            ApplyRenderers();
            ApplyEffects();
            ApplyCrowd();
        }

        void OnGUI()
        {
            float unit = Mathf.Max(24f, Screen.height / 26f);
            if (text == null)
            {
                text = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(unit * 0.42f), fontStyle = FontStyle.Bold, alignment = TextAnchor.LowerCenter };
                text.normal.textColor = Color.yellow;
                button = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(unit * 0.5f) };
            }
            GUI.Label(new Rect(0f, Screen.height - unit * 0.9f, Screen.width, unit * 0.8f), label, text);
            if (GUI.Button(new Rect(Screen.width - unit * 3.4f, Screen.height * 0.45f, unit * 3f, unit), open ? "Kapat" : "Test", button))
            {
                open = !open;
            }
            if (!open)
            {
                return;
            }

            float x = Screen.width * 0.5f - unit * 5.5f;
            float y = unit * 2.4f;
            float w = unit * 5.2f;
            Toggle(ref water, "Su", x, ref y, w, unit, ApplyRenderers);
            Toggle(ref shadows, "Gölge", x, ref y, w, unit, () => { if (Asset != null) { Asset.shadowDistance = shadows ? shadowDistance : 0f; } });
            Toggle(ref effects, "Efektler", x, ref y, w, unit, ApplyEffects);
            Toggle(ref crowd, "Seyirci", x, ref y, w, unit, ApplyCrowd);
            x += w + unit * 0.6f;
            y = unit * 2.4f;
            Toggle(ref decor, "Süsler", x, ref y, w, unit, ApplyRenderers);
            Toggle(ref ground, "Zemin", x, ref y, w, unit, ApplyRenderers);
            Toggle(ref avatars, "Avatarlar", x, ref y, w, unit, ApplyRenderers);
            if (GUI.Button(new Rect(x, y, w, unit), $"Çözünürlük %{Scales[scale] * 100f:F0}", button))
            {
                scale = (scale + 1) % Scales.Length;
                GraphicsQuality.Frozen = true;
                if (Asset != null)
                {
                    Asset.renderScale = Scales[scale];
                }
            }
            y += unit * 1.1f;
            if (GUI.Button(new Rect(x, y, w, unit), $"Kenar yumuşatma {Samples[msaa]}x", button))
            {
                msaa = (msaa + 1) % Samples.Length;
                GraphicsQuality.Frozen = true;
                if (Asset != null)
                {
                    Asset.msaaSampleCount = Samples[msaa];
                }
            }
        }

        void Toggle(ref bool value, string name, float x, ref float y, float w, float unit, System.Action apply)
        {
            if (GUI.Button(new Rect(x, y, w, unit), name + (value ? ": AÇIK" : ": kapalı"), button))
            {
                value = !value;
                apply();
            }
            y += unit * 1.1f;
        }

        void ApplyRenderers()
        {
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                Material material = renderer.sharedMaterial;
                string shader = material != null && material.shader != null ? material.shader.name : "";
                if (shader == "FartRafters/RiverWater")
                {
                    renderer.enabled = water;
                }
                else if (shader == "FartRafters/Ground")
                {
                    renderer.enabled = ground;
                }
                else if (renderer.GetComponentInParent<AvatarRig>() != null)
                {
                    renderer.enabled = avatars;
                }
                else if (renderer.name.StartsWith("Decor") || renderer.name.StartsWith("Island"))
                {
                    renderer.enabled = decor;
                }
            }
        }

        void ApplyEffects()
        {
            foreach (Volume volume in FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                volume.enabled = effects;
            }
            foreach (Camera camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                var data = camera.GetComponent<UniversalAdditionalCameraData>();
                if (data != null)
                {
                    data.renderPostProcessing = effects;
                }
            }
        }

        void ApplyCrowd()
        {
            foreach (SpectatorGroup group in FindObjectsByType<SpectatorGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                group.gameObject.SetActive(crowd);
            }
            foreach (SpectatorStreamer streamer in FindObjectsByType<SpectatorStreamer>(FindObjectsSortMode.None))
            {
                streamer.enabled = crowd;
            }
        }
    }
}
#endif
