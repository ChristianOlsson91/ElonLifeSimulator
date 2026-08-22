using ElonLifeSim.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Applies <see cref="HudPanelExclusivity"/> to the HUD panels. Top bar stays
    /// visible and is drawn last so large sheets cannot cover it.
    /// </summary>
    public sealed class HudPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject topBar;
        [SerializeField] private GameObject inboxPanel;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject companiesPanel;
        [SerializeField] private GameObject resolvePanel;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject dimOverlay;

        private HudLargePanel _open = HudLargePanel.None;

        public HudLargePanel OpenPanel => _open;

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
            GameObject overlay = null)
        {
            topBar = top;
            inboxPanel = inbox;
            mapPanel = map;
            companiesPanel = companies;
            resolvePanel = resolve;
            dialoguePanel = dialogue;
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

        public void Apply()
        {
            Set(inboxPanel, _open == HudLargePanel.Inbox);
            Set(mapPanel, _open == HudLargePanel.Map);
            Set(companiesPanel, _open == HudLargePanel.Companies);
            Set(resolvePanel, _open == HudLargePanel.Resolve);
            Set(dimOverlay, _open != HudLargePanel.None);

            if (topBar != null)
            {
                topBar.SetActive(true);
                topBar.transform.SetAsLastSibling();
            }
        }

        private void WireOverlay()
        {
            if (dimOverlay == null)
                return;
            var btn = dimOverlay.GetComponent<Button>();
            if (btn == null)
                return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(Close);
        }

        private static void Set(GameObject go, bool on)
        {
            if (go != null && go.activeSelf != on)
                go.SetActive(on);
        }
    }
}
