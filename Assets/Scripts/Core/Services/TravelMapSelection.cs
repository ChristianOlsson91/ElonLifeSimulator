using System.Collections.Generic;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Pure world-map target selection among unlocked locations (used by TravelMapUI + tests).
    /// </summary>
    public static class TravelMapSelection
    {
        /// <summary>
        /// Default target when opening the map: prefer preferredId if unlocked and not current;
        /// else first unlocked location that is not current; else null.
        /// </summary>
        public static string EnsureTarget(
            IReadOnlyList<GameLocation> unlocked,
            string currentLocationId,
            string preferredOrExistingTarget)
        {
            if (unlocked == null || unlocked.Count == 0)
                return null;

            if (!string.IsNullOrEmpty(preferredOrExistingTarget))
            {
                for (int i = 0; i < unlocked.Count; i++)
                {
                    if (unlocked[i].Id == preferredOrExistingTarget &&
                        unlocked[i].Id != currentLocationId)
                        return preferredOrExistingTarget;
                }
            }

            for (int i = 0; i < unlocked.Count; i++)
            {
                if (unlocked[i].Id != currentLocationId)
                    return unlocked[i].Id;
            }

            return null;
        }

        public static string SelectNext(
            IReadOnlyList<GameLocation> unlocked,
            string currentLocationId,
            string currentTargetId)
        {
            if (unlocked == null || unlocked.Count == 0)
                return null;

            // Build list of travel candidates (unlocked, not "must skip only if single = current")
            var candidates = new List<string>();
            for (int i = 0; i < unlocked.Count; i++)
                candidates.Add(unlocked[i].Id);

            if (candidates.Count == 0)
                return null;

            int idx = 0;
            if (!string.IsNullOrEmpty(currentTargetId))
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i] == currentTargetId)
                    {
                        idx = i;
                        break;
                    }
                }
            }

            // Advance until we find a different location or wrap fully
            for (int step = 1; step <= candidates.Count; step++)
            {
                int next = (idx + step) % candidates.Count;
                if (candidates[next] != currentLocationId || candidates.Count == 1)
                    return candidates[next];
            }

            return candidates[0];
        }

        public static string SelectById(IReadOnlyList<GameLocation> unlocked, string locationId)
        {
            if (unlocked == null || string.IsNullOrEmpty(locationId))
                return null;
            for (int i = 0; i < unlocked.Count; i++)
            {
                if (unlocked[i].Id == locationId)
                    return locationId;
            }
            return null;
        }
    }
}
