namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Location → Elon era folder and Resources keys (idle / walk / portrait).
    /// Location mapping is the current source of truth. <c>actId</c> is a reserved
    /// hook for a later act override and is ignored today.
    /// </summary>
    public static class ElonEraResolver
    {
        public const string ResourcesRoot = "Characters/Elon";

        public const string EraYoungSa = "01_young_sa";
        public const string EraYoungAdult90s = "02_young_adult_90s";
        public const string EraEarly2000s = "03_early_2000s";
        public const string EraModern = "04_modern";
        public const string EraMars = "05_mars";

        /// <summary>
        /// Prefix used in file names under the era folder (elon_young_sa_idle, …).
        /// </summary>
        public static string PrefixForEra(string eraFolder)
        {
            switch (eraFolder)
            {
                case EraYoungSa: return "elon_young_sa";
                case EraYoungAdult90s: return "elon_young_adult";
                case EraEarly2000s: return "elon_early2000s";
                case EraModern: return "elon_modern";
                case EraMars: return "elon_mars";
                default: return "elon_modern";
            }
        }

        /// <summary>
        /// Era folder for a location. <paramref name="actId"/> is ignored for now
        /// (Pretoria / Toronto / Palo Alto stay on their location eras).
        /// Pass location id <c>mars</c> to select the Mars suit era.
        /// </summary>
        public static string EraFolderForLocation(string locationId, string actId = null)
        {
            _ = actId; // reserved: act progress may override later
            if (IsPretoria(locationId))
                return EraYoungSa;
            if (IsToronto(locationId))
                return EraYoungAdult90s;
            if (IsPaloAlto(locationId))
                return EraEarly2000s;
            if (IsMars(locationId))
                return EraMars;
            // Empty/unknown stays Pretoria — never fall through to modern on Act 1.
            return EraYoungSa;
        }

        public static bool IsPretoria(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
                return true;
            if (locationId == PrototypeContent.LocationPretoria)
                return true;
            return ContainsIgnoreCase(locationId, "pretoria")
                   || ContainsIgnoreCase(locationId, "southafrica")
                   || ContainsIgnoreCase(locationId, "south_africa");
        }

        public static bool IsToronto(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
                return false;
            if (locationId == PrototypeContent.LocationToronto)
                return true;
            return ContainsIgnoreCase(locationId, "toronto")
                   || ContainsIgnoreCase(locationId, "canada");
        }

        public static bool IsPaloAlto(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
                return false;
            if (locationId == PrototypeContent.LocationPaloAlto)
                return true;
            return ContainsIgnoreCase(locationId, "palo")
                   || ContainsIgnoreCase(locationId, "silicon");
        }

        public static bool IsMars(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
                return false;
            return locationId == "mars" || locationId == EraMars
                   || ContainsIgnoreCase(locationId, "mars");
        }

        private static bool ContainsIgnoreCase(string haystack, string needle)
        {
            return haystack.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string IdleResourceKey(string locationId, string actId = null)
        {
            var era = EraFolderForLocation(locationId, actId);
            var prefix = PrefixForEra(era);
            return ResourcesRoot + "/" + era + "/" + prefix + "_idle";
        }

        public static string PortraitResourceKey(string locationId, string actId = null)
        {
            var era = EraFolderForLocation(locationId, actId);
            var prefix = PrefixForEra(era);
            return ResourcesRoot + "/" + era + "/" + prefix + "_portrait";
        }

        /// <summary>Walk frame resource key, e.g. frame 0 → …/walk/{prefix}_walk_00.</summary>
        public static string WalkResourceKey(string locationId, int frameIndex, string actId = null)
        {
            var era = EraFolderForLocation(locationId, actId);
            var prefix = PrefixForEra(era);
            var two = frameIndex < 10 ? "0" + frameIndex : frameIndex.ToString();
            return ResourcesRoot + "/" + era + "/walk/" + prefix + "_walk_" + two;
        }
    }
}
