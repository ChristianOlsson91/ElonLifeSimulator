namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Which large HUD surface is open. At most one of these is visible.
    /// Dialogue is not a large panel; it clears Menu, Inbox, Map, Companies, Story, and Resolve.
    /// </summary>
    public enum HudLargePanel
    {
        None = 0,
        Inbox = 1,
        Map = 2,
        Companies = 3,
        Resolve = 4,
        Menu = 5,
        Story = 6
    }

    /// <summary>
    /// Exclusive large-panel rules. Unity applies the result with SetActive.
    /// Toggle same panel closes it; opening another replaces it; Close clears;
    /// dialogue/story-line closes every large sheet so they do not cover the line.
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
        /// Dialogue is about to show: no large sheet (including Menu and Story) may sit on top.
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
        public static bool IsMenu(HudLargePanel current) => current == HudLargePanel.Menu;
        public static bool IsStory(HudLargePanel current) => current == HudLargePanel.Story;
    }
}
