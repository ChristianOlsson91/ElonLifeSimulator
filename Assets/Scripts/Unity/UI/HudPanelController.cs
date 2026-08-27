using System.Collections;
using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Applies <see cref="HudPanelExclusivity"/> to the HUD panels. Top bar stays
    /// visible and above the dimmer so nav stays clickable. Esc opens the centered
    /// Menu from gameplay. Sheet Close returns to the world. Dialogue is a bottom
    /// strip and is left alone.
    /// </summary>
    public sealed class HudPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject topBar;
        [SerializeField] private GameObject inboxPanel;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject companiesPanel;
        [SerializeField] private GameObject resolvePanel;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject storyPanel;
        [SerializeField] private GameObject dimOverlay;
        [SerializeField] private Button navInbox;
        [SerializeField] private Button navMap;
        [SerializeField] private Button navCompanies;
        [SerializeField] private Button navStory;

        private HudLargePanel _open = HudLargePanel.None;
        private HudLargePanel _shown = HudLargePanel.None;
        private Coroutine _motion;

        public HudLargePanel OpenPanel => _open;

        public bool HasOpenPanel => _open != HudLargePanel.None;

        public static HudPanelController Find()
        {
            return FindFirstObjectByType<HudPanelController>();
        }

        public void Bind(
            GameObject top,
            GameObject inbox,
            GameObject map,
            GameObject companies,
            GameObject resolve,
            GameObject dialogue,
            GameObject menu,
            GameObject story,
            GameObject overlay = null)
        {
            topBar = top;
            inboxPanel = inbox;
            mapPanel = map;
            companiesPanel = companies;
            resolvePanel = resolve;
            dialoguePanel = dialogue;
            menuPanel = menu;
            storyPanel = story;
            dimOverlay = overlay;
            _open = HudLargePanel.None;
            _shown = HudLargePanel.None;
            WireOverlay();
            ApplyImmediate();
        }

        public void BindNav(Button inbox, Button map, Button companies, Button story)
        {
            navInbox = inbox;
            navMap = map;
            navCompanies = companies;
            navStory = story;
            ApplyNavHighlight();
        }

        public void Toggle(HudLargePanel panel)
        {
            _open = HudPanelExclusivity.Toggle(_open, panel);
            Apply();
        }

        public void Open(HudLargePanel panel)
        {
            _open = HudPanelExclusivity.Open(_open, panel);
            Apply();
        }

        public void Close()
        {
            _open = HudPanelExclusivity.Close();
            Apply();
        }

        public void CloseIf(HudLargePanel panel)
        {
            if (_open == panel)
                Close();
        }

        public void PrepareForDialogueOrStory()
        {
            _open = HudPanelExclusivity.OnDialogueOrStory(_open);
            Apply();
        }

        public bool IsOpen(HudLargePanel panel)
        {
            return HudPanelExclusivity.IsOpen(_open, panel);
        }

        /// <summary>
        /// Esc / overlay: gameplay opens Menu; Menu closes; a content sheet
        /// returns to Menu. Dialogue playing is left as-is.
        /// </summary>
        public void HandleBack()
        {
            if (IsDialoguePlaying())
                return;

            if (_open == HudLargePanel.None)
                Open(HudLargePanel.Menu);
            else if (_open == HudLargePanel.Menu)
                Close();
            else
                Open(HudLargePanel.Menu);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                HandleBack();
        }

        public void Apply()
        {
            ApplyNavHighlight();
            if (topBar != null)
                topBar.SetActive(true);

            if (!isActiveAndEnabled)
            {
                ApplyImmediate();
                return;
            }

            if (_motion != null)
                StopCoroutine(_motion);
            _motion = StartCoroutine(Transition());
        }

        private void ApplyImmediate()
        {
            Set(inboxPanel, _open == HudLargePanel.Inbox);
            Set(mapPanel, _open == HudLargePanel.Map);
            Set(companiesPanel, _open == HudLargePanel.Companies);
            Set(resolvePanel, _open == HudLargePanel.Resolve);
            Set(menuPanel, _open == HudLargePanel.Menu);
            Set(storyPanel, _open == HudLargePanel.Story);
            Set(dimOverlay, _open != HudLargePanel.None);
            SnapAlpha(inboxPanel, _open == HudLargePanel.Inbox ? 1f : 0f);
            SnapAlpha(mapPanel, _open == HudLargePanel.Map ? 1f : 0f);
            SnapAlpha(companiesPanel, _open == HudLargePanel.Companies ? 1f : 0f);
            SnapAlpha(resolvePanel, _open == HudLargePanel.Resolve ? 1f : 0f);
            SnapAlpha(menuPanel, _open == HudLargePanel.Menu ? 1f : 0f);
            SnapAlpha(storyPanel, _open == HudLargePanel.Story ? 1f : 0f);
            SnapAlpha(dimOverlay, _open != HudLargePanel.None ? 1f : 0f);
            RaiseChrome();
            _shown = _open;
            ApplyNavHighlight();
        }

        private IEnumerator Transition()
        {
            var next = _open;
            var prev = _shown;
            float d = UiStyleTokens.PanelMotionSeconds;

            if (prev != HudLargePanel.None && prev != next)
            {
                var leaving = SheetFor(prev);
                yield return Fade(leaving, 1f, 0f, d, slide: true);
                Set(leaving, false);
            }

            bool dim = next != HudLargePanel.None;
            if (dimOverlay != null)
            {
                if (dim && !dimOverlay.activeSelf)
                {
                    Set(dimOverlay, true);
                    yield return Fade(dimOverlay, 0f, 1f, d, slide: false);
                }
                else if (!dim && dimOverlay.activeSelf)
                {
                    yield return Fade(dimOverlay, 1f, 0f, d, slide: false);
                    Set(dimOverlay, false);
                }
            }

            if (next != HudLargePanel.None && next != prev)
            {
                var sheet = SheetFor(next);
                Set(sheet, true);
                RaiseChrome();
                yield return Fade(sheet, 0f, 1f, d, slide: true);
            }

            RaiseChrome();
            _shown = next;
            _motion = null;
        }

        private IEnumerator Fade(GameObject go, float from, float to, float duration, bool slide)
        {
            if (go == null)
                yield break;
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = go.AddComponent<CanvasGroup>();
            var rt = go.GetComponent<RectTransform>();
            Vector2 rest = rt != null ? rt.anchoredPosition : Vector2.zero;
            float t = 0f;
            if (duration < 0.01f)
                duration = 0.12f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                k = k * k * (3f - 2f * k);
                cg.alpha = Mathf.Lerp(from, to, k);
                if (slide && rt != null)
                {
                    float slideK = to > from ? 1f - k : k;
                    rt.anchoredPosition = rest + new Vector2(0f, UiStyleTokens.PanelSlidePixels * slideK);
                }
                yield return null;
            }

            cg.alpha = to;
            if (rt != null)
                rt.anchoredPosition = rest;
        }

        private static void SnapAlpha(GameObject go, float alpha)
        {
            if (go == null)
                return;
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = go.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
        }

        private void RaiseChrome()
        {
            if (_open != HudLargePanel.None)
            {
                if (dimOverlay != null)
                    dimOverlay.transform.SetAsLastSibling();
                var sheet = SheetFor(_open);
                if (sheet != null)
                    sheet.transform.SetAsLastSibling();
            }

            if (topBar != null)
                topBar.transform.SetAsLastSibling();
        }

        private void ApplyNavHighlight()
        {
            UiTheme.ApplyNavVisual(navInbox, HudNavHighlight.IsActive(_open, HudLargePanel.Inbox));
            UiTheme.ApplyNavVisual(navMap, HudNavHighlight.IsActive(_open, HudLargePanel.Map));
            UiTheme.ApplyNavVisual(navCompanies, HudNavHighlight.IsActive(_open, HudLargePanel.Companies));
            UiTheme.ApplyNavVisual(navStory, HudNavHighlight.IsActive(_open, HudLargePanel.Story));
        }

        private GameObject SheetFor(HudLargePanel panel)
        {
            switch (panel)
            {
                case HudLargePanel.Inbox: return inboxPanel;
                case HudLargePanel.Map: return mapPanel;
                case HudLargePanel.Companies: return companiesPanel;
                case HudLargePanel.Resolve: return resolvePanel;
                case HudLargePanel.Menu: return menuPanel;
                case HudLargePanel.Story: return storyPanel;
                default: return null;
            }
        }

        private bool IsDialoguePlaying()
        {
            return dialoguePanel != null && dialoguePanel.activeSelf;
        }

        private void WireOverlay()
        {
            if (dimOverlay == null)
                return;
            var btn = dimOverlay.GetComponent<Button>();
            if (btn == null)
                return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(HandleBack);
        }

        private static void Set(GameObject go, bool on)
        {
            if (go != null && go.activeSelf != on)
                go.SetActive(on);
        }
    }
}
