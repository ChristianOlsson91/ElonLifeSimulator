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
                openMapButton.onClick.AddListener(Toggle);
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

        public void Toggle()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
            {
                hud.Toggle(HudLargePanel.Map);
                if (hud.IsOpen(HudLargePanel.Map))
                {
                    EnsureDefaultTarget();
                    Refresh();
                }
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
                hud.Open(HudLargePanel.Map);
            else if (panelRoot != null)
                panelRoot.SetActive(true);
            EnsureDefaultTarget();
            Refresh();
        }

        public void Hide()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
            {
                hud.CloseIf(HudLargePanel.Map);
                return;
            }

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
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;
            var unlocked = _session.Travel.GetUnlockedLocations().ToList();
            _targetLocationId = TravelMapSelection.SelectPrevious(
                unlocked, _session.Travel.CurrentLocationId, _targetLocationId);
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

            var here = _session.Travel.CurrentLocation?.DisplayName ?? "Unknown";
            var sb = new StringBuilder();
            sb.AppendLine("You are in " + here);
            sb.AppendLine();
            foreach (var loc in _session.Travel.GetAllLocations())
            {
                var unlocked = _session.Travel.IsUnlocked(loc.Id);
                var mark = loc.Id == _targetLocationId ? "▸ " : "    ";
                string tag;
                if (loc.Id == _session.Travel.CurrentLocationId)
                    tag = "You are here";
                else if (!unlocked)
                    tag = "Locked";
                else if (loc.Id == _targetLocationId)
                    tag = "Selected";
                else
                    tag = "";

                sb.AppendLine($"{mark}{loc.DisplayName}" + (tag.Length > 0 ? "   ·  " + tag : ""));
                sb.AppendLine($"      {loc.Description}");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(_targetLocationId) &&
                _targetLocationId != _session.Travel.CurrentLocationId)
            {
                var dest = _session.Travel.GetAllLocations()
                    .FirstOrDefault(l => l.Id == _targetLocationId);
                sb.AppendLine("Travel to " + (dest != null ? dest.DisplayName : _targetLocationId) + ".");
            }
            else
            {
                sb.AppendLine("Choose a destination with Prev / Next.");
            }

            listText.text = sb.ToString();
            if (travelButton != null)
            {
                bool can = !string.IsNullOrEmpty(_targetLocationId) &&
                           _targetLocationId != _session.Travel.CurrentLocationId &&
                           _session.Travel.IsUnlocked(_targetLocationId);
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
                Hide();
            else
                Refresh();
        }
    }
}
