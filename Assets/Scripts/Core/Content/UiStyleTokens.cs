namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Single visual language for title screen and gameplay HUD.
    /// Dark chrome: near-black panels, 1px highlight edge, brass accent — not Unity-blue.
    /// Type scale: 12 / 14 / 16 / 22 / 36. Motion 150 ms. Resolve dim ~76%.
    /// </summary>
    public static class UiStyleTokens
    {
        public const string GameTitle = "Elon: The Life Simulator";
        public const string GameSubtitle = "From Pretoria to Mars";
        public const string PrimaryCta = "New Game";
        public const string SecondaryCta = "Quit";
        public const string FooterLabel = "Community build";
        public const string DisclaimerLabel = "Fan-made. Not affiliated with Elon Musk or his companies.";
        public const string DisplayFontFamily = "Georgia";
        public const string UiFontFamily = "Segoe UI";
        public const string SpriteFilterName = "Point";

        public const int Type12 = 12;
        public const int Type14 = 14;
        public const int Type16 = 16;
        public const int Type22 = 22;
        public const int Type36 = 36;

        public const int CaptionFontSize = Type12;
        public const int BodyFontSize = Type14;
        public const int UiFontSize = Type16;
        public const int SubtitleFontSize = Type16;
        public const int PrimaryButtonFontSize = Type16;
        public const int SecondaryButtonFontSize = Type16;
        public const int PanelTitleFontSize = Type22;
        public const int TitleFontSize = Type36;
        public const int TopBarLabelFontSize = Type14;

        public const int PrimaryButtonHeight = 48;
        public const int SecondaryButtonHeight = 40;
        public const int TopBarButtonHeight = 32;
        public const int TopBarButtonWidth = 116;
        public const int TopBarHeight = 48;
        public const int TopBarScreenPadding = 20;
        public const int TopBarStatusWidth = 340;
        public const int ButtonGap = 12;
        public const int PanelPadding = 20;
        public const int HeaderHeight = 44;
        public const int CloseButtonWidth = 76;
        public const int CloseButtonHeight = 28;
        public const int ReferenceWidth = 1280;
        public const int ReferenceHeight = 720;
        public const int SheetActionHeight = 32;
        public const float DialogueStripAnchorMaxY = 0.24f;
        public const int DialogueStripBottomPad = 16;

        public const float PanelMotionSeconds = 0.15f;
        public const float PanelSlidePixels = 16f;
        public const float HoverScale = 1.035f;

        /// <summary>Unity FilterMode.Point. Pixel-art must never use bilinear (1).</summary>
        public const int SpriteFilterModePoint = 0;
        public const int SpriteFilterModeBilinear = 1;

        public const float ActiveNavR = 0.28f;
        public const float ActiveNavG = 0.22f;
        public const float ActiveNavB = 0.12f;

        public const float ScreenBgR = 0.035f;
        public const float ScreenBgG = 0.040f;
        public const float ScreenBgB = 0.058f;

        public const float PanelR = 0.040f;
        public const float PanelG = 0.042f;
        public const float PanelB = 0.052f;
        public const float PanelA = 0.97f;

        public const float PanelBorderR = 0.55f;
        public const float PanelBorderG = 0.48f;
        public const float PanelBorderB = 0.34f;
        public const float PanelBorderA = 0.90f;

        public const float TopBarR = 0.020f;
        public const float TopBarG = 0.022f;
        public const float TopBarB = 0.030f;
        public const float TopBarA = 0.92f;

        public const float OverlayR = 0.00f;
        public const float OverlayG = 0.00f;
        public const float OverlayB = 0.00f;
        public const float OverlayA = 0.76f;

        public const float AccentR = 0.82f;
        public const float AccentG = 0.64f;
        public const float AccentB = 0.32f;

        public const float PrimaryR = 0.145f;
        public const float PrimaryG = 0.125f;
        public const float PrimaryB = 0.090f;

        public const float SecondaryR = 0.090f;
        public const float SecondaryG = 0.092f;
        public const float SecondaryB = 0.110f;

        public const float TitleR = 0.94f;
        public const float TitleG = 0.91f;
        public const float TitleB = 0.84f;

        public const float MutedR = 0.62f;
        public const float MutedG = 0.58f;
        public const float MutedB = 0.50f;

        public const float DangerR = 0.72f;
        public const float DangerG = 0.28f;
        public const float DangerB = 0.22f;

        public const float DisabledR = 0.22f;
        public const float DisabledG = 0.22f;
        public const float DisabledB = 0.24f;
        public const float DisabledA = 0.45f;

        public static bool HasTypeScale()
        {
            return CaptionFontSize == 12
                   && BodyFontSize == 14
                   && UiFontSize == 16
                   && PanelTitleFontSize == 22
                   && TitleFontSize == 36;
        }

        public static bool PaddingInRange()
        {
            return PanelPadding >= 16 && PanelPadding <= 24
                   && TopBarScreenPadding >= 16 && TopBarScreenPadding <= 24;
        }

        public static bool PanelMotionInRange()
        {
            return PanelMotionSeconds >= 0.12f && PanelMotionSeconds <= 0.18f;
        }

        public static bool OverlayDimInRange()
        {
            return OverlayA >= 0.70f && OverlayA <= 0.80f;
        }

        public static bool PanelIsNearBlack()
        {
            float luma = 0.2126f * PanelR + 0.7152f * PanelG + 0.0722f * PanelB;
            return luma < 0.12f;
        }

        public static bool IsUnityDefaultButtonBlue(float r, float g, float b)
        {
            return Abs(r - 0.26f) < 0.08f && Abs(g - 0.52f) < 0.10f && Abs(b - 0.96f) < 0.10f;
        }

        public static bool PrimaryIsNotUnityBlue()
        {
            return !IsUnityDefaultButtonBlue(PrimaryR, PrimaryG, PrimaryB)
                   && !IsUnityDefaultButtonBlue(AccentR, AccentG, AccentB);
        }

        public static bool UsesPointFilter()
        {
            return SpriteFilterName == "Point" && SpriteFilterModePoint == 0 && SpriteFilterModeBilinear == 1;
        }

        private static float Abs(float v) => v < 0f ? -v : v;
    }
}
