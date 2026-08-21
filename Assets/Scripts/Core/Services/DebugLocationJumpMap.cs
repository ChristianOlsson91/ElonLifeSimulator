using System.Collections.Generic;
using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// F1–F5 debug jump intent: location id, era, and whether that place exists
    /// on the travel registry. Execution is Unlock + TravelTo only (no Act1, no
    /// <see cref="GameSession.TravelTo"/>).
    /// </summary>
    public static class DebugLocationJumpMap
    {
        public static DebugJumpTarget ForKey(int functionKey)
        {
            return ForKey(functionKey, PrototypeContent.CreateLocations());
        }

        public static DebugJumpTarget ForKey(int functionKey, IEnumerable<GameLocation> registry)
        {
            var list = registry ?? new GameLocation[0];
            string locationId = null;
            string era = null;

            switch (functionKey)
            {
                case 1:
                    locationId = PrototypeContent.LocationPretoria;
                    era = ElonEraResolver.EraFolderForLocation(locationId);
                    break;
                case 2:
                    locationId = PrototypeContent.LocationToronto;
                    era = ElonEraResolver.EraFolderForLocation(locationId);
                    break;
                case 3:
                    locationId = PrototypeContent.LocationPaloAlto;
                    era = ElonEraResolver.EraFolderForLocation(locationId);
                    break;
                case 4:
                    era = ElonEraResolver.EraModern;
                    locationId = FindLocationIdForEra(list, era);
                    break;
                case 5:
                    era = ElonEraResolver.EraMars;
                    locationId = FindLocationIdForEra(list, era);
                    break;
                default:
                    return new DebugJumpTarget
                    {
                        FunctionKey = functionKey,
                        PlaceExists = false
                    };
            }

            return new DebugJumpTarget
            {
                FunctionKey = functionKey,
                LocationId = locationId,
                EraFolder = era,
                PlaceExists = locationId != null && ContainsId(list, locationId)
            };
        }

        /// <summary>
        /// Unlock + TravelTo the F-key target. Does not touch Act1 or GameSession story APIs.
        /// Registers PrototypeContent locations if the travel service is empty.
        /// </summary>
        public static DebugJumpResult TryJump(TravelService travel, int functionKey)
        {
            if (travel == null)
                throw new System.ArgumentNullException(nameof(travel));

            if (travel.GetAllLocations().Count == 0)
                travel.RegisterLocations(PrototypeContent.CreateLocations());

            var target = ForKey(functionKey, travel.GetAllLocations());
            var from = travel.CurrentLocationId ?? "(none)";
            if (!target.PlaceExists)
            {
                var missingTo = string.IsNullOrEmpty(target.LocationId) ? "(none)" : target.LocationId;
                return new DebugJumpResult
                {
                    FunctionKey = functionKey,
                    FromLocationId = from,
                    ToLocationId = target.LocationId,
                    EraFolder = target.EraFolder,
                    PlaceMissing = true,
                    Moved = false,
                    Log = FormatLog(from, missingTo, target.EraFolder ?? "", missingPlace: true)
                };
            }

            travel.Unlock(target.LocationId);
            bool moved = travel.TravelTo(target.LocationId);
            var to = travel.CurrentLocationId;
            var era = ElonEraResolver.EraFolderForLocation(to);
            return new DebugJumpResult
            {
                FunctionKey = functionKey,
                FromLocationId = from,
                ToLocationId = to,
                EraFolder = era,
                PlaceMissing = false,
                Moved = moved,
                Log = FormatLog(from, to, era, missingPlace: false)
            };
        }

        public static string FormatLog(string from, string to, string era, bool missingPlace)
        {
            var line = "[DebugJump] " + (from ?? "(none)") + " → " + (to ?? "(none)") + " | era=" + (era ?? "");
            if (missingPlace)
                line += " missing as place";
            return line;
        }

        private static string FindLocationIdForEra(IEnumerable<GameLocation> registry, string eraFolder)
        {
            foreach (var loc in registry)
            {
                if (loc == null) continue;
                if (ElonEraResolver.EraFolderForLocation(loc.Id) == eraFolder)
                    return loc.Id;
            }
            return null;
        }

        private static bool ContainsId(IEnumerable<GameLocation> registry, string locationId)
        {
            foreach (var loc in registry)
            {
                if (loc != null && loc.Id == locationId)
                    return true;
            }
            return false;
        }
    }

    public sealed class DebugJumpTarget
    {
        public int FunctionKey { get; set; }
        public string LocationId { get; set; }
        public string EraFolder { get; set; }
        public bool PlaceExists { get; set; }
    }

    public sealed class DebugJumpResult
    {
        public int FunctionKey { get; set; }
        public string FromLocationId { get; set; }
        public string ToLocationId { get; set; }
        public string EraFolder { get; set; }
        public bool PlaceMissing { get; set; }
        public bool Moved { get; set; }
        public string Log { get; set; }
    }
}
