using ElonLifeSim.Unity.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.Bootstrap
{
    /// <summary>
    /// Builds a playable Main Menu at runtime if the scene is empty/minimal.
    /// PLACEHOLDER UI.
    /// </summary>
    public sealed class MainMenuSceneSetup : MonoBehaviour
    {
        private void Awake()
        {
            if (FindFirstObjectByType<MainMenuController>() != null &&
                FindFirstObjectByType<Canvas>() != null)
                return;

            BuildMenu();
        }

        private void BuildMenu()
        {
            // Camera
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5;
                cam.backgroundColor = new Color(0.08f, 0.1f, 0.16f, 1f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }

            var canvasGo = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            var titleGo = CreateText(canvasGo.transform, "Title", "Elon: The Life Simulator", 36,
                new Vector2(0, 120), new Vector2(900, 60));
            var subGo = CreateText(canvasGo.transform, "Subtitle",
                "2D pixel narrative life-sim · Prototype\nRespectful · Inspiring · Slightly humorous",
                16, new Vector2(0, 50), new Vector2(900, 50));

            var newGame = CreateButton(canvasGo.transform, "NewGameButton", "New Game", new Vector2(0, -20), new Vector2(220, 48));
            var quit = CreateButton(canvasGo.transform, "QuitButton", "Quit", new Vector2(0, -90), new Vector2(220, 48));

            var note = CreateText(canvasGo.transform, "PlaceholderNote",
                "[PLACEHOLDER UI — replace with pixel art main menu]",
                12, new Vector2(0, -200), new Vector2(700, 30));
            note.color = new Color(1f, 0.85f, 0.4f, 0.9f);

            var controller = canvasGo.AddComponent<MainMenuController>();
            // MainMenuController finds buttons by name in Start.
            Debug.Log("[ElonLifeSim] Main menu PLACEHOLDER UI ready.");
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
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null)
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = content;
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
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
            go.GetComponent<Image>().color = new Color(0.18f, 0.4f, 0.7f, 1f);
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
