using System;
using System.Collections.Generic;
using System.Linq;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Pure travel / location state. Tracks current location, known destinations, and unlocks.
    ///
    /// Unity layer (SceneFlowController) listens to <see cref="LocationChanged"/> and
    /// loads the scene named by <see cref="GameLocation.SceneName"/>.
    ///
    /// Extension: register locations via <see cref="RegisterLocation"/>; unlock via progression.
    /// </summary>
    public sealed class TravelService
    {
        private readonly Dictionary<string, GameLocation> _locations =
            new Dictionary<string, GameLocation>(StringComparer.Ordinal);
        private readonly HashSet<string> _unlocked =
            new HashSet<string>(StringComparer.Ordinal);

        public string CurrentLocationId { get; private set; }
        public GameLocation CurrentLocation =>
            CurrentLocationId != null && _locations.TryGetValue(CurrentLocationId, out var loc)
                ? loc
                : null;

        /// <summary>Args: previousLocationId, newLocationId.</summary>
        public event Action<string, string> LocationChanged;

        public void RegisterLocation(GameLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));
            _locations[location.Id] = location;
        }

        public void RegisterLocations(IEnumerable<GameLocation> locations)
        {
            if (locations == null) return;
            foreach (var loc in locations)
                RegisterLocation(loc);
        }

        public bool TryGetLocation(string locationId, out GameLocation location)
        {
            if (locationId != null && _locations.TryGetValue(locationId, out location))
                return true;
            location = null;
            return false;
        }

        public IReadOnlyCollection<GameLocation> GetAllLocations()
        {
            return _locations.Values;
        }

        public IReadOnlyList<GameLocation> GetUnlockedLocations()
        {
            return _locations.Values.Where(l => _unlocked.Contains(l.Id)).ToList();
        }

        public bool IsUnlocked(string locationId)
        {
            return locationId != null && _unlocked.Contains(locationId);
        }

        /// <summary>Unlocks a registered location for travel (idempotent).</summary>
        public bool Unlock(string locationId)
        {
            if (string.IsNullOrEmpty(locationId) || !_locations.ContainsKey(locationId))
                return false;
            return _unlocked.Add(locationId);
        }

        /// <summary>
        /// Sets starting location and auto-unlocks it. Fires LocationChanged if id changes.
        /// </summary>
        public bool SetStartingLocation(string locationId)
        {
            if (string.IsNullOrEmpty(locationId) || !_locations.ContainsKey(locationId))
                return false;

            _unlocked.Add(locationId);
            var previous = CurrentLocationId;
            CurrentLocationId = locationId;
            if (!string.Equals(previous, locationId, StringComparison.Ordinal))
                LocationChanged?.Invoke(previous, locationId);
            return true;
        }

        /// <summary>
        /// Travel to an unlocked destination. Returns false if unknown, locked, or already there.
        /// </summary>
        public bool TravelTo(string locationId)
        {
            if (string.IsNullOrEmpty(locationId) || !_locations.ContainsKey(locationId))
                return false;
            if (!_unlocked.Contains(locationId))
                return false;
            if (string.Equals(CurrentLocationId, locationId, StringComparison.Ordinal))
                return false;

            var previous = CurrentLocationId;
            CurrentLocationId = locationId;
            LocationChanged?.Invoke(previous, locationId);
            return true;
        }

        /// <summary>
        /// Travel suggested by an accepted ticket (destination must be unlocked).
        /// </summary>
        public bool TravelForTicket(InboxTicket ticket)
        {
            if (ticket == null)
                return false;
            return TravelTo(ticket.LocationId);
        }

        public void ClearUnlocks()
        {
            _unlocked.Clear();
            if (CurrentLocationId != null)
                _unlocked.Add(CurrentLocationId);
        }
    }
}
