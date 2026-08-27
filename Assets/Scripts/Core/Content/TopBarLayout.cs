namespace ElonLifeSim.Core.Content
{
    /// <summary>Pixel rect in top-bar local space, origin top-left, Y down.</summary>
    public readonly struct HudRect
    {
        public readonly float X;
        public readonly float Y;
        public readonly float W;
        public readonly float H;

        public HudRect(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public float Right => X + W;
        public float Bottom => Y + H;

        public bool FullyInside(float width, float height, float pad)
        {
            return X >= pad - 0.01f
                   && Y >= -0.01f
                   && Right <= width - pad + 0.01f
                   && Bottom <= height + 0.01f;
        }

        public bool Overlaps(HudRect other)
        {
            return X < other.Right && Right > other.X && Y < other.Bottom && Bottom > other.Y;
        }
    }

    /// <summary>
    /// Single-row gameplay top bar: Inbox, Map, Companies, Story on the left;
    /// location + act status on the right. Padding keeps labels off the screen edge.
    /// </summary>
    public static class TopBarLayout
    {
        public const int NavCount = 4;

        public static readonly string[] NavLabels =
        {
            "Inbox", "Map", "Companies", "Story"
        };

        public static int ScreenPad => UiStyleTokens.TopBarScreenPadding;
        public static int Gap => UiStyleTokens.ButtonGap;
        public static int ButtonWidth => UiStyleTokens.TopBarButtonWidth;
        public static int ButtonHeight => UiStyleTokens.TopBarButtonHeight;
        public static int BarHeight => UiStyleTokens.TopBarHeight;
        public static int CanvasWidth => UiStyleTokens.ReferenceWidth;
        public static int CanvasHeight => UiStyleTokens.ReferenceHeight;

        public static HudRect NavButton(int index, int canvasWidth = 0)
        {
            _ = canvasWidth;
            if (index < 0 || index >= NavCount)
                return new HudRect(0, 0, 0, 0);
            float y = (BarHeight - ButtonHeight) * 0.5f;
            float x = ScreenPad + index * (ButtonWidth + Gap);
            return new HudRect(x, y, ButtonWidth, ButtonHeight);
        }

        public static HudRect StatusCluster(int canvasWidth = 0)
        {
            int width = canvasWidth > 0 ? canvasWidth : CanvasWidth;
            float y = (BarHeight - ButtonHeight) * 0.5f;
            float w = UiStyleTokens.TopBarStatusWidth;
            float x = width - ScreenPad - w;
            return new HudRect(x, y, w, ButtonHeight);
        }

        public static float NavClusterRight()
        {
            return NavButton(NavCount - 1).Right;
        }

        public static bool StatusClearsNav(int canvasWidth = 0)
        {
            return NavClusterRight() + Gap <= StatusCluster(canvasWidth).X;
        }

        public static bool AllNavButtonsFullyVisible(int canvasWidth = 0)
        {
            int width = canvasWidth > 0 ? canvasWidth : CanvasWidth;
            for (int i = 0; i < NavCount; i++)
            {
                if (!NavButton(i, width).FullyInside(width, BarHeight, ScreenPad))
                    return false;
            }

            return StatusCluster(width).FullyInside(width, BarHeight, ScreenPad)
                   && StatusClearsNav(width);
        }

        public static float UnityYFromBottom(HudRect rect)
        {
            return BarHeight - rect.Y - rect.H;
        }
    }

    /// <summary>Readable top-bar copy. Not a debug dump.</summary>
    public static class HudStatusCopy
    {
        public static string LocationLine(string displayName)
        {
            return string.IsNullOrEmpty(displayName) ? "" : displayName;
        }

        public static string ActLine(int actNumber, string place)
        {
            if (string.IsNullOrEmpty(place))
                return "Act " + actNumber;
            return "Act " + actNumber + ": " + place;
        }

        public static string ActLineForLocation(string locationId)
        {
            if (locationId == PrototypeContent.LocationToronto)
                return ActLine(2, "Toronto");
            if (locationId == PrototypeContent.LocationPaloAlto)
                return ActLine(3, "Palo Alto");
            return ActLine(1, "Pretoria");
        }
    }

    /// <summary>Dialogue lives in a bottom strip and must stay below the top bar.</summary>
    public static class DialogueStripLayout
    {
        public static float AnchorMinY => 0f;
        public static float AnchorMaxY => UiStyleTokens.DialogueStripAnchorMaxY;
        public static float BottomPad => UiStyleTokens.DialogueStripBottomPad;

        public static float TopBarBottomNormalized()
        {
            return 1f - (UiStyleTokens.TopBarHeight / (float)UiStyleTokens.ReferenceHeight);
        }

        public static bool OverlapsTopBar()
        {
            return AnchorMaxY >= TopBarBottomNormalized();
        }
    }
}
