namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Which large HUD surface is open. At most one of these is visible.
    /// Dialogue/story is not a large panel; it clears Inbox/Map/Companies (and Resolve).
    /// </summary>
    public enum HudLargePanel
    {
        None = 0,
        Inbox = 1,
        Map = 2,
        Companies = 3,
        Resolve = 4
    }

    /// <summary>
    /// Exclusive large-panel rules. Unity applies the result with SetActive.
    /// Toggle same panel closes it; opening another replaces it; Close clears;
    /// dialogue/story closes Inbox, Map, and Companies (and Resolve so they do not cover the line).
    /// </summary>
    public static class HudPanelExclusivity
    {
        public static HudLargePanel Toggle(HudLargePanel current, HudLargePanel requested)
        {
            if (requested == HudLargePanel.None)
                return HudLargePanel.None;
            if (current == requested)
                return HudLargePanel.None;
            return requested;
        }

        public static HudLargePanel Open(HudLargePanel current, HudLargePanel requested)
        {
            _ = current;
            if (requested == HudLargePanel.None)
                return HudLargePanel.None;
            return requested;
        }

        public static HudLargePanel Close()
        {
            return HudLargePanel.None;
        }

        /// <summary>
        /// Dialogue or Story is about to show: Inbox, Map, and Companies must not sit on top.
        /// Resolve is also closed so a large sheet cannot cover the line.
        /// </summary>
        public static HudLargePanel OnDialogueOrStory(HudLargePanel current)
        {
            _ = current;
            return HudLargePanel.None;
        }

        public static bool IsOpen(HudLargePanel current, HudLargePanel panel)
        {
            return panel != HudLargePanel.None && current == panel;
        }

        public static bool IsInbox(HudLargePanel current) => current == HudLargePanel.Inbox;
        public static bool IsMap(HudLargePanel current) => current == HudLargePanel.Map;
        public static bool IsCompanies(HudLargePanel current) => current == HudLargePanel.Companies;
        public static bool IsResolve(HudLargePanel current) => current == HudLargePanel.Resolve;
    }
}
