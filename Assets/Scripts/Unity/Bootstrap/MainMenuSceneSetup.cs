using ElonLifeSim.Unity.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.Bootstrap
{
    /// <summary>
    /// Builds a playable Main Menu at runtime if the scene is empty/minimal.
    /// PLACEHOLDER UI. Safe to call multiple times (idempotent).
    /// </summary>
    public sealed class MainMenuSceneSetup : MonoBehaviour
    {
        private static bool s_builtThisSession;

        private void Awake()
        {
            EnsureBuilt();
        }

        private void Start()
        {
            // Second chance if Awake order was wrong.
            EnsureBuilt();
        }

        public void EnsureBuilt()
        {
            EnsureCamera(new Color(0.08f, 0.1f, 0.16f, 1f));

            if (FindFirstObjectByType<MainMenuController>() != null &&
                FindFirstObjectByType<Canvas>() != null)
            {
                s_builtThisSession = true;
                return;
            }

            if (s_builtThisSession && FindFirstObjectByType<Canvas>() != null)
                return;

            BuildMenu();
            s_builtThisSession = true;
        }

        private void BuildMenu()
        {
            EnsureCamera(new Color(0.08f, 0.1f, 0.16f, 1f));

            var canvasGo = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem));
                // Support both old and new input backends.
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Full-screen panel so Game view is never empty solid blue.
            var bg = new GameObject("MenuBackground", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(canvasGo.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.18f, 1f);
            bg.GetComponent<Image>().raycastTarget = false;

            CreateText(canvasGo.transform, "Title", "Elon: The Life Simulator", 36,
                new Vector2(0, 120), new Vector2(900, 60));
            CreateText(canvasGo.transform, "Subtitle",
                "2D pixel narrative life-sim · Prototype\nRespectful · Inspiring · Slightly humorous",
                16, new Vector2(0, 50), new Vector2(900, 60));

            CreateButton(canvasGo.transform, "NewGameButton", "New Game", new Vector2(0, -20), new Vector2(220, 48));
            CreateButton(canvasGo.transform, "QuitButton", "Quit", new Vector2(0, -90), new Vector2(220, 48));

            var note = CreateText(canvasGo.transform, "PlaceholderNote",
                "[PLACEHOLDER UI — replace with pixel art main menu]",
                12, new Vector2(0, -200), new Vector2(700, 30));
            note.color = new Color(1f, 0.85f, 0.4f, 0.9f);

            canvasGo.AddComponent<MainMenuController>();
            Debug.Log("[ElonLifeSim] Main menu PLACEHOLDER UI ready.");
        }

        private static void EnsureCamera(Color background)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                if (FindFirstObjectByType<AudioListener>() == null)
                    camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.enabled = true;
        }

        private static Font GetUiFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Helvetica" }, 16);
            return font;
        }

        private static Text CreateText(Transform parent, string name, string content, int size,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.font = GetUiFont();
            t.text = content;
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.85f, 1f);

            var text = CreateText(go.transform, "Label", label, 18, Vector2.zero, size);
            var trt = text.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            return go.GetComponent<Button>();
        }
    }
}
