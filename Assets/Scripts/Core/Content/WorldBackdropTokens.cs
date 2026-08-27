namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Simple color-block world behind Elon. Pretoria is a Highveld dusk:
    /// dark blue sky, warm horizon, dry earth floor — not editor gray.
    /// </summary>
    public readonly struct WorldBackdropPalette
    {
        public readonly string LocationId;
        public readonly float SkyR, SkyG, SkyB;
        public readonly float HorizonR, HorizonG, HorizonB;
        public readonly float GroundR, GroundG, GroundB;
        public readonly float GroundY;
        public readonly float GroundHeight;
        public readonly float HorizonY;
        public readonly float HorizonHeight;

        public WorldBackdropPalette(
            string locationId,
            float skyR, float skyG, float skyB,
            float horizonR, float horizonG, float horizonB,
            float groundR, float groundG, float groundB,
            float groundY, float groundHeight,
            float horizonY, float horizonHeight)
        {
            LocationId = locationId;
            SkyR = skyR;
            SkyG = skyG;
            SkyB = skyB;
            HorizonR = horizonR;
            HorizonG = horizonG;
            HorizonB = horizonB;
            GroundR = groundR;
            GroundG = groundG;
            GroundB = groundB;
            GroundY = groundY;
            GroundHeight = groundHeight;
            HorizonY = horizonY;
            HorizonHeight = horizonHeight;
        }

        public float SkyLuma => Luma(SkyR, SkyG, SkyB);
        public float GroundLuma => Luma(GroundR, GroundG, GroundB);
        public float GroundTop => GroundY + GroundHeight * 0.5f;
        public float GroundBottom => GroundY - GroundHeight * 0.5f;

        public static float Luma(float r, float g, float b)
        {
            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }
    }

    public static class WorldBackdropTokens
    {
        public const string BackdropRootName = "WorldBackdrop";
        public const string GroundName = "Ground";
        public const string HorizonName = "Horizon";
        public const string HorizonLineName = "HorizonLine";
        public const string VignetteName = "Vignette";
        public const string SoftSkyName = "SoftSky";
        public const float HorizonLineHeight = 0.07f;
        public const float VignetteAlpha = 0.42f;

        public static WorldBackdropPalette ForLocation(string locationId)
        {
            if (locationId == PrototypeContent.LocationToronto)
                return Toronto();
            if (locationId == PrototypeContent.LocationPaloAlto)
                return PaloAlto();
            return Pretoria();
        }

        /// <summary>Highveld evening: indigo sky, jacaranda-dust horizon, ochre floor.</summary>
        public static WorldBackdropPalette Pretoria()
        {
            return new WorldBackdropPalette(
                PrototypeContent.LocationPretoria,
                0.078f, 0.125f, 0.255f,
                0.430f, 0.250f, 0.310f,
                0.275f, 0.215f, 0.135f,
                -2.75f, 3.70f,
                -0.72f, 0.44f);
        }

        public static WorldBackdropPalette Toronto()
        {
            return new WorldBackdropPalette(
                PrototypeContent.LocationToronto,
                0.070f, 0.110f, 0.200f,
                0.220f, 0.260f, 0.340f,
                0.160f, 0.175f, 0.205f,
                -2.75f, 3.70f,
                -0.72f, 0.44f);
        }

        public static WorldBackdropPalette PaloAlto()
        {
            return new WorldBackdropPalette(
                PrototypeContent.LocationPaloAlto,
                0.090f, 0.160f, 0.280f,
                0.360f, 0.280f, 0.180f,
                0.240f, 0.210f, 0.150f,
                -2.75f, 3.70f,
                -0.72f, 0.44f);
        }

        /// <summary>Unity scene-view / default-clear grays that read as an empty editor.</summary>
        public static bool LooksLikeEditorGray(float r, float g, float b)
        {
            float spread = Abs(r - g) + Abs(g - b) + Abs(r - b);
            float avg = (r + g + b) / 3f;
            return spread < 0.08f && avg > 0.25f && avg < 0.65f;
        }

        public static bool IsDesignedBackdrop(WorldBackdropPalette p)
        {
            if (p.GroundHeight <= 0f || p.HorizonHeight <= 0f)
                return false;
            if (LooksLikeEditorGray(p.SkyR, p.SkyG, p.SkyB))
                return false;
            if (LooksLikeEditorGray(p.GroundR, p.GroundG, p.GroundB))
                return false;
            if (Abs(p.SkyR - p.GroundR) + Abs(p.SkyG - p.GroundG) + Abs(p.SkyB - p.GroundB) < 0.12f)
                return false;
            if (p.SkyLuma >= 0.28f)
                return false;
            // Floor stays under the actor so the torso reads against the sky.
            if (p.GroundTop >= 0.05f)
                return false;
            return true;
        }

        public static bool HasHorizonLine()
        {
            return HorizonLineName == "HorizonLine" && HorizonLineHeight > 0f && HorizonLineHeight < 0.2f;
        }

        private static float Abs(float v) => v < 0f ? -v : v;
    }
}
