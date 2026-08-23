using ElonLifeSim.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Applies <see cref="HudPanelExclusivity"/> to the HUD panels. Top bar stays
    /// visible. When a sheet is open, overlay and sheet sit above the top bar.
    /// Esc opens the centered Menu from gameplay, closes Menu back to the game,
    /// and returns content sheets to the Menu. Dialogue is left alone.
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

        private HudLargePanel _open = HudLargePanel.None;

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
            WireOverlay();
            Apply();
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
            Set(inboxPanel, _open == HudLargePanel.Inbox);
            Set(mapPanel, _open == HudLargePanel.Map);
            Set(companiesPanel, _open == HudLargePanel.Companies);
            Set(resolvePanel, _open == HudLargePanel.Resolve);
            Set(menuPanel, _open == HudLargePanel.Menu);
            Set(storyPanel, _open == HudLargePanel.Story);
            Set(dimOverlay, _open != HudLargePanel.None);

            if (topBar != null)
                topBar.SetActive(true);

            if (_open != HudLargePanel.None)
            {
                if (dimOverlay != null)
                    dimOverlay.transform.SetAsLastSibling();
                var sheet = SheetFor(_open);
                if (sheet != null)
                    sheet.transform.SetAsLastSibling();
            }
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
