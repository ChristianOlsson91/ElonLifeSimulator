using System.Linq;
using System.Text;
using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// World-map / hub UI: cycle unlocked locations, travel free or for active ticket.
    /// Target selection uses pure <see cref="TravelMapSelection"/>.
    /// </summary>
    public sealed class TravelMapUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text listText;
        [SerializeField] private Button travelButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button openMapButton;
        [SerializeField] private Button nextLocationButton;
        [SerializeField] private Button prevLocationButton;

        private GameSession _session;
        private string _targetLocationId;

        public string TargetLocationId => _targetLocationId;

        public void Bind(GameSession session, GameObject panel, Text list,
            Button travel, Button close, Button openMap)
        {
            _session = session;
            panelRoot = panel;
            listText = list;
            travelButton = travel;
            closeButton = close;
            openMapButton = openMap;
            Wire();
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void BindLocationNav(Button prev, Button next)
        {
            prevLocationButton = prev;
            nextLocationButton = next;
            if (prevLocationButton != null)
            {
                prevLocationButton.onClick.RemoveAllListeners();
                prevLocationButton.onClick.AddListener(SelectPreviousLocation);
            }
            if (nextLocationButton != null)
            {
                nextLocationButton.onClick.RemoveAllListeners();
                nextLocationButton.onClick.AddListener(SelectNextLocation);
            }
        }

        private void Start()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            Wire();
        }

        private void Wire()
        {
            if (openMapButton != null)
            {
                openMapButton.onClick.RemoveAllListeners();
                openMapButton.onClick.AddListener(Show);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
            if (travelButton != null)
            {
                travelButton.onClick.RemoveAllListeners();
                travelButton.onClick.AddListener(OnTravel);
            }
            if (prevLocationButton != null)
            {
                prevLocationButton.onClick.RemoveAllListeners();
                prevLocationButton.onClick.AddListener(SelectPreviousLocation);
            }
            if (nextLocationButton != null)
            {
                nextLocationButton.onClick.RemoveAllListeners();
                nextLocationButton.onClick.AddListener(SelectNextLocation);
            }
        }

        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);
            EnsureDefaultTarget();
            Refresh();
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        /// <summary>Opens map focused on the active ticket's location.</summary>
        public void ShowForActiveTicket()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session != null && !string.IsNullOrEmpty(_session.ActiveTicketId) &&
                _session.Inbox.TryGet(_session.ActiveTicketId, out var ticket))
            {
                _targetLocationId = ticket.LocationId;
            }
            Show();
        }

        /// <summary>Select an unlocked location as travel target (public for tests / buttons).</summary>
        public bool SelectLocation(string locationId)
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return false;

            var unlocked = _session.Travel.GetUnlockedLocations().ToList();
            var id = TravelMapSelection.SelectById(unlocked, locationId);
            if (id == null)
                return false;
            _targetLocationId = id;
            Refresh();
            return true;
        }

        public void SelectNextLocation()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;
            var unlocked = _session.Travel.GetUnlockedLocations().ToList();
            _targetLocationId = TravelMapSelection.SelectNext(
                unlocked, _session.Travel.CurrentLocationId, _targetLocationId);
            Refresh();
        }

        public void SelectPreviousLocation()
        {
            // Reuse SelectNext cycling by walking once per unlocked count-1 — simple: reverse via next
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;
            var unlocked = _session.Travel.GetUnlockedLocations().ToList();
            if (unlocked.Count == 0) return;
            // N-1 next steps = previous
            for (int i = 0; i < unlocked.Count - 1; i++)
            {
                _targetLocationId = TravelMapSelection.SelectNext(
                    unlocked, _session.Travel.CurrentLocationId, _targetLocationId);
            }
            Refresh();
        }

        private void EnsureDefaultTarget()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;
            var unlocked = _session.Travel.GetUnlockedLocations().ToList();
            _targetLocationId = TravelMapSelection.EnsureTarget(
                unlocked, _session.Travel.CurrentLocationId, _targetLocationId);
        }

        private void Refresh()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null || listText == null) return;

            EnsureDefaultTarget();

            var sb = new StringBuilder();
            sb.AppendLine("WORLD MAP  [Next Loc / Travel]");
            sb.AppendLine($"Current: {_session.Travel.CurrentLocation?.DisplayName ?? "Unknown"}");
            sb.AppendLine("----------------");
            foreach (var loc in _session.Travel.GetAllLocations())
            {
                var unlocked = _session.Travel.IsUnlocked(loc.Id);
                var cur = loc.Id == _session.Travel.CurrentLocationId ? " [YOU]" : "";
                var tgt = loc.Id == _targetLocationId ? " << SELECTED" : "";
                var lockMark = unlocked ? "" : " [LOCKED]";
                sb.AppendLine($"• {loc.DisplayName}{cur}{tgt}{lockMark}");
                sb.AppendLine($"  {loc.Description}");
            }

            if (!string.IsNullOrEmpty(_targetLocationId))
                sb.AppendLine($"\nReady to travel to: {_targetLocationId}");
            else
                sb.AppendLine("\nNo unlocked destination (or already at only location).");

            listText.text = sb.ToString();
            if (travelButton != null)
            {
                bool can = !string.IsNullOrEmpty(_targetLocationId) &&
                           _targetLocationId != _session.Travel.CurrentLocationId &&
                           _session.Travel.IsUnlocked(_targetLocationId);
                // Ticket path may unlock on TravelToActiveTicketLocation.
                if (!can && !string.IsNullOrEmpty(_targetLocationId) &&
                    !string.IsNullOrEmpty(_session.ActiveTicketId) &&
                    _session.Inbox.TryGet(_session.ActiveTicketId, out var t) &&
                    t.LocationId == _targetLocationId)
                {
                    can = _targetLocationId != _session.Travel.CurrentLocationId;
                }
                travelButton.interactable = can;
            }
        }

        private void OnTravel()
        {
            if (_session == null) return;
            EnsureDefaultTarget();
            if (string.IsNullOrEmpty(_targetLocationId))
            {
                Debug.LogWarning("[Travel] No target selected.");
                Refresh();
                return;
            }

            bool ok;
            if (!string.IsNullOrEmpty(_session.ActiveTicketId) &&
                _session.Inbox.TryGet(_session.ActiveTicketId, out var ticket) &&
                ticket.LocationId == _targetLocationId)
            {
                ok = _session.TravelToActiveTicketLocation();
            }
            else
            {
                if (!_session.Travel.IsUnlocked(_targetLocationId))
                    _session.Travel.Unlock(_targetLocationId);
                ok = _session.TravelTo(_targetLocationId);
            }

            if (ok)
            {
                Hide();
            }
            else
            {
                Debug.LogWarning("[Travel] Travel failed.");
                Refresh();
            }
        }
    }
}
