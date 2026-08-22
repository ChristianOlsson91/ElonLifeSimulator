using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Characters;
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

            UiTheme.CreateHairline(canvasGo.transform, "MenuAccent",
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                new Vector2(4f, 0f), UiTheme.Primary);

            UiTheme.CreateHairline(canvasGo.transform, "MenuTopLine",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 2f), UiTheme.Primary);

            var title = UiTheme.CreateCenteredText(canvasGo.transform, "Title", UiStyleTokens.GameTitle,
                UiStyleTokens.TitleFontSize, new Vector2(-80f, 118f), new Vector2(820f, 64f));
            title.color = UiTheme.Title;

            var subtitle = UiTheme.CreateCenteredText(canvasGo.transform, "Subtitle", UiStyleTokens.GameSubtitle,
                UiStyleTokens.SubtitleFontSize, new Vector2(-80f, 62f), new Vector2(720f, 32f));
            subtitle.color = UiTheme.Muted;

            var primaryW = 280f;
            UiTheme.CreateButton(
                canvasGo.transform,
                "NewGameButton",
                "New Game",
                new Vector2(-80f, -16f),
                new Vector2(primaryW, UiStyleTokens.PrimaryButtonHeight),
                primary: true,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));

            UiTheme.CreateButton(
                canvasGo.transform,
                "QuitButton",
                "Quit",
                new Vector2(-80f, -16f - UiStyleTokens.PrimaryButtonHeight - UiStyleTokens.ButtonGap - 4),
                new Vector2(primaryW, UiStyleTokens.SecondaryButtonHeight),
                primary: false,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));

            AddPortrait(canvasGo.transform);

            var disclaimer = UiTheme.CreateCenteredText(canvasGo.transform, "Disclaimer",
                UiStyleTokens.DisclaimerLabel, UiStyleTokens.CaptionFontSize,
                Vector2.zero, new Vector2(720f, 22f));
            disclaimer.color = new Color(UiStyleTokens.MutedR, UiStyleTokens.MutedG, UiStyleTokens.MutedB, 0.75f);
            var dRt = disclaimer.rectTransform;
            dRt.anchorMin = new Vector2(0.5f, 0f);
            dRt.anchorMax = new Vector2(0.5f, 0f);
            dRt.pivot = new Vector2(0.5f, 0f);
            dRt.anchoredPosition = new Vector2(0f, 40f);

            var footer = UiTheme.CreateCenteredText(canvasGo.transform, "Footer", UiStyleTokens.FooterLabel,
                UiStyleTokens.CaptionFontSize, Vector2.zero, new Vector2(480f, 24f));
            footer.color = new Color(UiStyleTokens.MutedR, UiStyleTokens.MutedG, UiStyleTokens.MutedB, 0.55f);
            var footerRt = footer.rectTransform;
            footerRt.anchorMin = new Vector2(0.5f, 0f);
            footerRt.anchorMax = new Vector2(0.5f, 0f);
            footerRt.pivot = new Vector2(0.5f, 0f);
            footerRt.anchoredPosition = new Vector2(0f, 18f);

            canvasGo.AddComponent<MainMenuController>();
        }

        private static void AddPortrait(Transform parent)
        {
            var sprite = ElonSpriteCatalog.LoadIdle(PrototypeContent.LocationPretoria);
            if (sprite == null)
                return;

            var go = new GameObject("MenuPortrait", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(340f, 10f);
            rt.sizeDelta = new Vector2(220f, 320f);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = Color.white;
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
