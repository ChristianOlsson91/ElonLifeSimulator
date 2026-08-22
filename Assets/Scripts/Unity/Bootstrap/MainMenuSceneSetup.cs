using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Controllers;
using ElonLifeSim.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.Bootstrap
{
    /// <summary>
    /// Builds the main menu at runtime if the scene is empty/minimal. Idempotent.
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
            EnsureBuilt();
        }

        public void EnsureBuilt()
        {
            EnsureCamera(UiTheme.ScreenBackground);

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
            EnsureCamera(UiTheme.ScreenBackground);

            var canvasGo = UiTheme.CreateCanvas("MainMenuCanvas", 100);

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem));
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            UiTheme.CreateFullBleed(canvasGo.transform, "MenuBackground", UiTheme.ScreenBackground);

            var accent = new GameObject("MenuAccent", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(canvasGo.transform, false);
            var accentRt = accent.GetComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0f, 0f);
            accentRt.anchorMax = new Vector2(0f, 1f);
            accentRt.pivot = new Vector2(0f, 0.5f);
            accentRt.sizeDelta = new Vector2(4f, 0f);
            accentRt.anchoredPosition = Vector2.zero;
            accent.GetComponent<Image>().color = UiTheme.Primary;
            accent.GetComponent<Image>().raycastTarget = false;

            var topLine = new GameObject("MenuTopLine", typeof(RectTransform), typeof(Image));
            topLine.transform.SetParent(canvasGo.transform, false);
            var topRt = topLine.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.sizeDelta = new Vector2(0f, 2f);
            topRt.anchoredPosition = Vector2.zero;
            topLine.GetComponent<Image>().color = UiTheme.Primary;
            topLine.GetComponent<Image>().raycastTarget = false;

            var title = UiTheme.CreateCenteredText(canvasGo.transform, "Title", UiStyleTokens.GameTitle,
                UiStyleTokens.TitleFontSize, new Vector2(0f, 110f), new Vector2(980f, 64f));
            title.color = UiTheme.Title;

            var subtitle = UiTheme.CreateCenteredText(canvasGo.transform, "Subtitle", UiStyleTokens.GameSubtitle,
                UiStyleTokens.SubtitleFontSize, new Vector2(0f, 52f), new Vector2(720f, 32f));
            subtitle.color = UiTheme.Muted;

            var primaryW = 280f;
            UiTheme.CreateButton(
                canvasGo.transform,
                "NewGameButton",
                "New Game",
                new Vector2(0f, -16f),
                new Vector2(primaryW, UiStyleTokens.PrimaryButtonHeight),
                primary: true,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));

            UiTheme.CreateButton(
                canvasGo.transform,
                "QuitButton",
                "Quit",
                new Vector2(0f, -16f - UiStyleTokens.PrimaryButtonHeight - UiStyleTokens.ButtonGap - 4),
                new Vector2(primaryW, UiStyleTokens.SecondaryButtonHeight),
                primary: false,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));

            var footer = UiTheme.CreateCenteredText(canvasGo.transform, "Footer", UiStyleTokens.FooterLabel,
                UiStyleTokens.CaptionFontSize, Vector2.zero, new Vector2(480f, 24f));
            footer.color = new Color(UiStyleTokens.MutedR, UiStyleTokens.MutedG, UiStyleTokens.MutedB, 0.7f);
            var footerRt = footer.rectTransform;
            footerRt.anchorMin = new Vector2(0.5f, 0f);
            footerRt.anchorMax = new Vector2(0.5f, 0f);
            footerRt.pivot = new Vector2(0.5f, 0f);
            footerRt.anchoredPosition = new Vector2(0f, 22f);

            canvasGo.AddComponent<MainMenuController>();
            Debug.Log("[ElonLifeSim] Main menu ready.");
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
    }
}
