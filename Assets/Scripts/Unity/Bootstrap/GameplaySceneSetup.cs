using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Characters;
using ElonLifeSim.Unity.Controllers;
using ElonLifeSim.Unity.UI;
using UnityEngine;

namespace ElonLifeSim.Unity.Bootstrap
{
    /// <summary>
    /// Runtime setup for location scenes: camera, ground, era Elon sprite, controllers.
    /// </summary>
    public sealed class GameplaySceneSetup : MonoBehaviour
    {
        [SerializeField] private string locationId = PrototypeContent.LocationPretoria;
        [SerializeField] private Color backgroundColor = new Color(0.078f, 0.125f, 0.255f, 1f);
        [SerializeField] private Color groundColor = new Color(0.275f, 0.215f, 0.135f, 1f);
        [SerializeField] private Color playerColor = new Color(0.85f, 0.7f, 0.45f, 1f);

        private bool _built;

        /// <summary>Called by RuntimePlayBootstrap when scene objects lack a setup component.</summary>
        public void Configure(string locId, Color bg, Color ground)
        {
            if (!string.IsNullOrEmpty(locId))
                locationId = locId;
            backgroundColor = bg;
            groundColor = ground;
            if (!_built)
                Build();
            else
                SetupPlayer();
        }

        /// <summary>Re-apply era sprites for an existing or new Player_Elon.</summary>
        public void RefreshPlayer(string locId)
        {
            if (!string.IsNullOrEmpty(locId))
                locationId = locId;
            SetupPlayer();
        }

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            if (_built) return;
            _built = true;
            SetupCamera();
            SetupWorld();
            SetupPlayer();
            SetupControllers();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }

            var palette = WorldBackdropTokens.ForLocation(locationId);
            cam.orthographic = true;
            cam.orthographicSize = PixelOrthoSize();
            cam.backgroundColor = new Color(palette.SkyR, palette.SkyG, palette.SkyB, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.allowMSAA = false;
            cam.transform.position = new Vector3(0, 0, -10);
        }

        /// <summary>Integer screen-pixel zoom nearest the old 5-unit view.</summary>
        internal static float PixelOrthoSize()
        {
            float ppu = ElonSpriteCatalog.PixelsPerUnit;
            float height = Screen.height > 0 ? Screen.height : 720f;
            float best = 5f;
            float bestDelta = float.MaxValue;
            for (int scale = 1; scale <= 8; scale++)
            {
                float ortho = height / (2f * ppu * scale);
                float d = Mathf.Abs(ortho - 5f);
                if (d < bestDelta)
                {
                    bestDelta = d;
                    best = ortho;
                }
            }

            return best;
        }

        private void SetupWorld()
        {
            var leftover = GameObject.Find("PLACEHOLDER_ART_MARKER");
            if (leftover != null)
                Destroy(leftover);

            if (GameObject.Find(WorldBackdropTokens.BackdropRootName) != null)
                return;

            DestroyIfPresent("Ground");
            DestroyIfPresent("PlaceholderGround");

            var palette = WorldBackdropTokens.ForLocation(locationId);
            backgroundColor = new Color(palette.SkyR, palette.SkyG, palette.SkyB, 1f);
            groundColor = new Color(palette.GroundR, palette.GroundG, palette.GroundB, 1f);

            var root = new GameObject(WorldBackdropTokens.BackdropRootName);
            CreateBand(root.transform, WorldBackdropTokens.SoftSkyName,
                new Color(palette.SkyR + 0.04f, palette.SkyG + 0.03f, palette.SkyB + 0.02f, 1f),
                1.15f, 28f, 5.2f, -25);
            CreateBand(root.transform, WorldBackdropTokens.HorizonName,
                new Color(palette.HorizonR, palette.HorizonG, palette.HorizonB, 1f),
                palette.HorizonY, 28f, palette.HorizonHeight, -15);
            CreateBand(root.transform, WorldBackdropTokens.HorizonLineName,
                new Color(0.62f, 0.42f, 0.28f, 1f),
                palette.GroundTop + WorldBackdropTokens.HorizonLineHeight * 0.5f,
                28f, WorldBackdropTokens.HorizonLineHeight, -14);
            CreateBand(root.transform, WorldBackdropTokens.GroundName,
                new Color(palette.GroundR, palette.GroundG, palette.GroundB, 1f),
                palette.GroundY, 28f, palette.GroundHeight, -20);
            CreateVignette(root.transform);
        }

        private static void CreateVignette(Transform parent)
        {
            float a = WorldBackdropTokens.VignetteAlpha;
            var dark = new Color(0.01f, 0.015f, 0.03f, 1f);
            CreateBand(parent, WorldBackdropTokens.VignetteName + "Top", dark, 4.4f, 28f, 2.2f, 8);
            CreateBand(parent, WorldBackdropTokens.VignetteName + "Left", dark, 0.4f, 6f, 10f, 8);
            CreateBand(parent, WorldBackdropTokens.VignetteName + "Right", dark, 0.4f, 6f, 10f, 8);
            var left = parent.Find(WorldBackdropTokens.VignetteName + "Left");
            if (left != null)
                left.position = new Vector3(-7.4f, 0.4f, 0f);
            var right = parent.Find(WorldBackdropTokens.VignetteName + "Right");
            if (right != null)
                right.position = new Vector3(7.4f, 0.4f, 0f);
            var top = parent.Find(WorldBackdropTokens.VignetteName + "Top");
            if (top != null)
            {
                var sr = top.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = new Color(dark.r, dark.g, dark.b, a);
            }
        }

        private static void DestroyIfPresent(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
                Destroy(go);
        }

        private static void CreateBand(Transform parent, string name, Color color, float y, float width, float height, int sort)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSolidSprite(color, 64, 64);
            sr.sortingOrder = sort;
            UiTheme.ApplyPointFilter(sr);
            const float spriteWorld = 64f / 16f;
            go.transform.localScale = new Vector3(width / spriteWorld, height / spriteWorld, 1f);
        }

        private void SetupPlayer()
        {
            ElonAppearanceApplier.Apply(locationId);
        }

        private void SetupControllers()
        {
            if (FindFirstObjectByType<LocationSceneController>() == null)
            {
                var go = new GameObject("LocationSceneController");
                go.AddComponent<LocationSceneController>();
                go.AddComponent<LocationIdOverride>().locationId = locationId;
            }

            if (FindFirstObjectByType<SceneFlowController>() == null)
            {
                var go = new GameObject("SceneFlowController");
                go.AddComponent<SceneFlowController>();
            }

            if (FindFirstObjectByType<GameplayHudBuilder>() == null)
            {
                var go = new GameObject("GameplayHudBuilder");
                go.AddComponent<GameplayHudBuilder>();
            }

            ElonAppearanceController.Ensure();
            DebugLocationJump.Ensure();
        }

        private static Sprite CreateSolidSprite(Color color, int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        }
    }

    /// <summary>Applies location id before LocationSceneController.Start.</summary>
    public sealed class LocationIdOverride : MonoBehaviour
    {
        public string locationId;

        private void Awake()
        {
            var loc = GetComponent<LocationSceneController>();
            if (loc == null || string.IsNullOrEmpty(locationId)) return;
            loc.SetLocationId(locationId);
        }
    }
}
