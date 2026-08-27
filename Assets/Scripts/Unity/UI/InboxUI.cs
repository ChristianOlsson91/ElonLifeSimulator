using System.Text;
using ElonLifeSim.Core.Models;
using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Persistent Inbox panel: list, Prev/Next selection, accept, resolve company problems.
    /// Selection uses pure <see cref="InboxSelection"/> so completed tickets do not stick forever.
    /// </summary>
    public sealed class InboxUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text listText;
        [SerializeField] private Text detailText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button resolveButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;

        private GameSession _session;
        private string _selectedTicketId;

        public string SelectedTicketId => _selectedTicketId;

        public void Bind(GameSession session, GameObject panel, Text list, Text detail,
            Button accept, Button close, Button toggle)
        {
            _session = session;
            panelRoot = panel;
            listText = list;
            detailText = detail;
            acceptButton = accept;
            closeButton = close;
            toggleButton = toggle;
            WireButtons();
            if (panelRoot != null)
                panelRoot.SetActive(false);
            Refresh();
        }

        public void BindResolveButton(Button resolve)
        {
            resolveButton = resolve;
            if (resolveButton != null)
            {
                resolveButton.onClick.RemoveAllListeners();
                resolveButton.onClick.AddListener(OnResolve);
            }
        }

        public void BindNavButtons(Button prev, Button next)
        {
            prevButton = prev;
            nextButton = next;
            if (prevButton != null)
            {
                prevButton.onClick.RemoveAllListeners();
                prevButton.onClick.AddListener(SelectPrevious);
            }
            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(SelectNext);
            }
        }

        private void Start()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            WireButtons();
            if (_session != null)
                _session.Inbox.InboxChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_session != null)
                _session.Inbox.InboxChanged -= Refresh;
        }

        private void WireButtons()
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
            if (acceptButton != null)
            {
                acceptButton.onClick.RemoveAllListeners();
                acceptButton.onClick.AddListener(OnAccept);
            }
            if (resolveButton != null)
            {
                resolveButton.onClick.RemoveAllListeners();
                resolveButton.onClick.AddListener(OnResolve);
            }
            if (prevButton != null)
            {
                prevButton.onClick.RemoveAllListeners();
                prevButton.onClick.AddListener(SelectPrevious);
            }
            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(SelectNext);
            }
        }

        public void Toggle()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
            {
                hud.Toggle(HudLargePanel.Inbox);
                if (hud.IsOpen(HudLargePanel.Inbox))
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
                hud.Open(HudLargePanel.Inbox);
            else if (panelRoot != null)
                panelRoot.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
            {
                hud.Close();
                return;
            }

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        /// <summary>Public selection API for UI buttons / tests of the same rules.</summary>
        public void SelectTicket(string ticketId)
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;
            var tickets = _session.Inbox.ListTickets();
            _selectedTicketId = InboxSelection.SelectById(tickets, ticketId);
            Refresh();
        }

        public void SelectNext()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;
            var tickets = _session.Inbox.ListTickets();
            _selectedTicketId = InboxSelection.SelectNext(tickets, _selectedTicketId);
            Refresh();
        }

        public void SelectPrevious()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;
            var tickets = _session.Inbox.ListTickets();
            _selectedTicketId = InboxSelection.SelectPrevious(tickets, _selectedTicketId);
            Refresh();
        }

        public void Refresh()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;

            var tickets = _session.Inbox.ListTickets();
            _selectedTicketId = InboxSelection.EnsureSelected(tickets, _selectedTicketId);

            var sb = new StringBuilder();
            if (tickets.Count == 0)
            {
                sb.AppendLine("Nothing waiting.");
                sb.AppendLine();
                sb.AppendLine("Follow the story, or found a company, and work will show up here.");
            }
            else
            {
                sb.AppendLine(tickets.Count == 1 ? "1 item" : tickets.Count + " items");
                sb.AppendLine();
                foreach (var t in tickets)
                {
                    var mark = t.Id == _selectedTicketId ? "▸ " : "    ";
                    sb.AppendLine($"{mark}{StatusLabel(t.Status)}   {t.Title}");
                    sb.AppendLine($"      {t.CompanyDisplayName}");
                }
            }

            if (listText != null)
                listText.text = sb.ToString();

            UpdateDetail();
        }

        private void UpdateDetail()
        {
            if (detailText == null) return;
            if (_session == null || string.IsNullOrEmpty(_selectedTicketId) ||
                !_session.Inbox.TryGet(_selectedTicketId, out var t))
            {
                detailText.text = ticketsHint();
                if (acceptButton != null) acceptButton.interactable = false;
                if (resolveButton != null) resolveButton.interactable = false;
                return;
            }

            bool isProblem = _session.TryGetProblem(t.Id, out _);
            var hint = isProblem
                ? "Accept, travel if needed, then Resolve."
                : "";
            detailText.text =
                $"<b>{t.Title}</b>\n" +
                $"{t.CompanyDisplayName}  ·  {t.LocationDisplayName}  ·  {t.Difficulty}/5\n" +
                $"Reward: {t.RewardDescription}\n" +
                $"Status: {StatusLabel(t.Status)}\n" +
                (string.IsNullOrEmpty(hint) ? "\n" : hint + "\n\n") +
                t.Description;

            if (acceptButton != null)
                acceptButton.interactable = t.Status == TicketStatus.Pending;
            if (resolveButton != null)
                resolveButton.interactable = isProblem &&
                    (t.Status == TicketStatus.Accepted || t.Status == TicketStatus.Pending);
        }

        private string ticketsHint()
        {
            return _session != null && _session.Inbox.ListTickets().Count > 0
                ? "Use Prev / Next to choose an item."
                : "";
        }

        private static string StatusLabel(TicketStatus status)
        {
            switch (status)
            {
                case TicketStatus.Pending: return "New";
                case TicketStatus.Accepted: return "Accepted";
                case TicketStatus.InProgress: return "In progress";
                case TicketStatus.Completed: return "Done";
                case TicketStatus.Failed: return "Failed";
                default: return status.ToString();
            }
        }

        private void OnAccept()
        {
            if (_session == null || string.IsNullOrEmpty(_selectedTicketId))
                return;

            if (!_session.AcceptTicket(_selectedTicketId))
                return;

            FindFirstObjectByType<TravelMapUI>()?.ShowForActiveTicket();
            Refresh();
        }

        private void OnResolve()
        {
            if (_session == null || string.IsNullOrEmpty(_selectedTicketId))
                return;

            if (_session.Inbox.TryGet(_selectedTicketId, out var t) && t.Status == TicketStatus.Pending)
                _session.AcceptTicket(_selectedTicketId);

            var resolveUi = FindFirstObjectByType<ProblemResolveUI>();
            if (resolveUi != null)
                resolveUi.ShowForTicket(_selectedTicketId);

            Refresh();
        }
    }
}
