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
        [SerializeField] private Color backgroundColor = new Color(0.35f, 0.4f, 0.28f, 1f);
        [SerializeField] private Color groundColor = new Color(0.45f, 0.5f, 0.32f, 1f);
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

            cam.orthographic = true;
            cam.orthographicSize = PixelOrthoSize();
            cam.backgroundColor = backgroundColor;
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

            if (GameObject.Find("Ground") != null || GameObject.Find("PlaceholderGround") != null)
                return;

            var ground = new GameObject("Ground");
            var sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSolidSprite(groundColor, 64, 64);
            sr.sortingOrder = -10;
            ground.transform.localScale = new Vector3(16, 10, 1);
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
