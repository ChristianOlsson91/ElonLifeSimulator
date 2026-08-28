using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Characters;
using ElonLifeSim.Unity.Controllers;
using ElonLifeSim.Unity.UI;
using UnityEngine;

namespace ElonLifeSim.Unity.Bootstrap
{
    /// <summary>
    /// Runtime setup for location scenes: camera, placeholder floor, young Elon sprite, controllers.
    /// Marked PLACEHOLDER throughout.
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
            cam.orthographicSize = 5f;
            cam.backgroundColor = backgroundColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0, 0, -10);
        }

        private void SetupWorld()
        {
            // PLACEHOLDER ground quad (sprite-like) via SpriteRenderer from a solid texture.
            if (GameObject.Find("PlaceholderGround") != null) return;

            var ground = new GameObject("PlaceholderGround");
            var sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSolidSprite(groundColor, 64, 64);
            sr.sortingOrder = -10;
            ground.transform.localScale = new Vector3(16, 10, 1);

            // Border label in world space (optional visual)
            var marker = new GameObject("PLACEHOLDER_ART_MARKER");
            var markerSr = marker.AddComponent<SpriteRenderer>();
            markerSr.sprite = CreateSolidSprite(new Color(1f, 0.8f, 0.2f, 0.35f), 32, 8);
            marker.transform.position = new Vector3(0, 4.2f, 0);
            marker.transform.localScale = new Vector3(6, 0.4f, 1);
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
                var loc = go.AddComponent<LocationSceneController>();
                // locationId set via public method if needed — use reflection-free approach:
                // LocationSceneController reads serialized field; for runtime we add a helper.
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
        }

        private static Sprite CreateSolidSprite(Color color, int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            // Simple face detail for player-sized sprites
            if (w == 16 && h == 24)
            {
                for (int x = 5; x <= 6; x++)
                {
                    pixels[x + 16 * 16] = Color.black; // eyes row
                    pixels[x + 9 + 16 * 16] = Color.black;
                }
            }
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
            // Use SendMessage-free serialized set via public API on controller.
            loc.SetLocationId(locationId);
        }
    }
}
