namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Shared visual language for main menu and in-game HUD.
    /// Hierarchy: title &gt; primary button &gt; secondary. Unity builders consume these values.
    /// </summary>
    public static class UiStyleTokens
    {
        public const string GameTitle = "Elon: The Life Simulator";
        public const string GameSubtitle = "From Pretoria to Mars";
        public const string FooterLabel = "Community build";
        public const string DisclaimerLabel = "Fan-made. Not affiliated with Elon Musk or his companies.";

        public const int TitleFontSize = 44;
        public const int SubtitleFontSize = 16;
        public const int PrimaryButtonFontSize = 18;
        public const int SecondaryButtonFontSize = 16;
        public const int PanelTitleFontSize = 18;
        public const int BodyFontSize = 14;
        public const int CaptionFontSize = 12;
        public const int TopBarLabelFontSize = 13;

        public const int PrimaryButtonHeight = 48;
        public const int SecondaryButtonHeight = 40;
        public const int TopBarButtonHeight = 32;
        public const int TopBarButtonWidth = 116;
        public const int TopBarHeight = 48;
        public const int TopBarScreenPadding = 16;
        public const int TopBarStatusWidth = 340;
        public const int ButtonGap = 10;
        public const int PanelPadding = 16;
        public const int HeaderHeight = 40;
        public const int CloseButtonWidth = 72;
        public const int CloseButtonHeight = 28;
        public const int ReferenceWidth = 1280;
        public const int ReferenceHeight = 720;
        public const int SheetActionHeight = 32;
        public const float DialogueStripAnchorMaxY = 0.24f;
        public const int DialogueStripBottomPad = 12;

        public const float ActiveNavR = 0.16f;
        public const float ActiveNavG = 0.50f;
        public const float ActiveNavB = 0.46f;

        public const float ScreenBgR = 0.055f;
        public const float ScreenBgG = 0.065f;
        public const float ScreenBgB = 0.090f;

        public const float PanelR = 0.090f;
        public const float PanelG = 0.100f;
        public const float PanelB = 0.135f;
        public const float PanelA = 0.96f;

        public const float TopBarR = 0.045f;
        public const float TopBarG = 0.050f;
        public const float TopBarB = 0.070f;
        public const float TopBarA = 0.94f;

        public const float OverlayR = 0.02f;
        public const float OverlayG = 0.025f;
        public const float OverlayB = 0.04f;
        public const float OverlayA = 0.55f;

        public const float PrimaryR = 0.14f;
        public const float PrimaryG = 0.46f;
        public const float PrimaryB = 0.50f;

        public const float SecondaryR = 0.17f;
        public const float SecondaryG = 0.19f;
        public const float SecondaryB = 0.25f;

        public const float TitleR = 0.93f;
        public const float TitleG = 0.95f;
        public const float TitleB = 0.97f;

        public const float MutedR = 0.62f;
        public const float MutedG = 0.68f;
        public const float MutedB = 0.74f;
    }
}
