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
            return Step(unlocked, currentLocationId, currentTargetId, +1);
        }

        public static string SelectPrevious(
            IReadOnlyList<GameLocation> unlocked,
            string currentLocationId,
            string currentTargetId)
        {
            return Step(unlocked, currentLocationId, currentTargetId, -1);
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

        private static string Step(
            IReadOnlyList<GameLocation> unlocked,
            string currentLocationId,
            string currentTargetId,
            int direction)
        {
            if (unlocked == null || unlocked.Count == 0)
                return null;

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

            int count = candidates.Count;
            for (int step = 1; step <= count; step++)
            {
                int next = ((idx + direction * step) % count + count) % count;
                if (candidates[next] != currentLocationId || count == 1)
                    return candidates[next];
            }

            return candidates[0];
        }
    }
}
