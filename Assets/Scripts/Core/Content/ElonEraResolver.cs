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
            if (locationId == PrototypeContent.LocationPretoria)
                return EraYoungSa;
            if (locationId == PrototypeContent.LocationToronto)
                return EraYoungAdult90s;
            if (locationId == PrototypeContent.LocationPaloAlto)
                return EraEarly2000s;
            if (locationId == "mars" || locationId == EraMars)
                return EraMars;
            return EraModern;
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
