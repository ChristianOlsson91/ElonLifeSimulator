using System.Text;
using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Models;
using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Shows company stats and Found Zip2 / Found X.com actions.
    /// PLACEHOLDER panel built by GameplayHudBuilder.
    /// </summary>
    public sealed class CompanyDashboardUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button foundZip2Button;
        [SerializeField] private Button foundXComButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button toggleButton;

        private GameSession _session;

        public void Bind(GameSession session, GameObject panel, Text body,
            Button foundZip2, Button foundXCom, Button close, Button toggle)
        {
            _session = session;
            panelRoot = panel;
            bodyText = body;
            foundZip2Button = foundZip2;
            foundXComButton = foundXCom;
            closeButton = close;
            toggleButton = toggle;
            Wire();
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void Start()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            Wire();
            if (_session != null)
                _session.Companies.CompaniesChanged += Refresh;
        }

        private void OnDestroy()
        {
            if (_session != null)
                _session.Companies.CompaniesChanged -= Refresh;
        }

        private void Wire()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveAllListeners();
                toggleButton.onClick.AddListener(Toggle);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
            if (foundZip2Button != null)
            {
                foundZip2Button.onClick.RemoveAllListeners();
                foundZip2Button.onClick.AddListener(OnFoundZip2);
            }
            if (foundXComButton != null)
            {
                foundXComButton.onClick.RemoveAllListeners();
                foundXComButton.onClick.AddListener(OnFoundXCom);
            }
        }

        public void Toggle()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
            {
                hud.Toggle(HudLargePanel.Companies);
                if (hud.IsOpen(HudLargePanel.Companies))
                    Refresh();
                return;
            }

            if (panelRoot == null) return;
            if (panelRoot.activeSelf) Hide();
            else Show();
        }

        public void Show()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
                hud.Open(HudLargePanel.Companies);
            else if (panelRoot != null)
                panelRoot.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
            {
                hud.CloseIf(HudLargePanel.Companies);
                return;
            }

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void Refresh()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null || bodyText == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("COMPANIES");
            sb.AppendLine("----------------");
            foreach (var c in _session.Companies.ListCompanies())
            {
                sb.AppendLine($"{c.DisplayName}  [{c.Status}]");
                sb.AppendLine($"  Money: {c.Money}  Progress: {c.Progress}");
                sb.AppendLine($"  Engineering: {c.EngineeringLevel}  Opinion: {c.PublicOpinion}");
                sb.AppendLine($"  {c.Summary}");
                sb.AppendLine();
            }

            if (_session.CanFoundZip2())
                sb.AppendLine("Action: Found Zip2 is available.");
            if (_session.CanFoundXCom())
                sb.AppendLine("Action: Found X.com is available (after Zip2 sale).");

            bodyText.text = sb.ToString();

            if (foundZip2Button != null)
                foundZip2Button.interactable = _session.CanFoundZip2();
            if (foundXComButton != null)
                foundXComButton.interactable = _session.CanFoundXCom();
        }

        private void OnFoundZip2()
        {
            if (_session == null) return;
            if (_session.FoundZip2())
            {
                Debug.Log("[ElonLifeSim] Zip2 founded — Zip2 problems delivered to Inbox.");
                FindFirstObjectByType<InboxUI>()?.Show();
                Refresh();
            }
        }

        private void OnFoundXCom()
        {
            if (_session == null) return;
            if (_session.FoundXCom())
            {
                Debug.Log("[ElonLifeSim] X.com founded — X.com problems delivered to Inbox.");
                FindFirstObjectByType<InboxUI>()?.Show();
                Refresh();
            }
        }
    }
}
