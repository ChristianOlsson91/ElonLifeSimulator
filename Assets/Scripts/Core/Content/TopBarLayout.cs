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

        /// <summary>Right-side location word, same row as nav (not a second debug line).</summary>
        public static HudRect LocationStatus(int canvasWidth = 0)
        {
            var cluster = StatusCluster(canvasWidth);
            return new HudRect(cluster.X, cluster.Y, 140f, cluster.H);
        }

        /// <summary>Right-side act word, same row as nav.</summary>
        public static HudRect ActStatus(int canvasWidth = 0)
        {
            var cluster = StatusCluster(canvasWidth);
            return new HudRect(cluster.Right - 190f, cluster.Y, 190f, cluster.H);
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

    /// <summary>Readable top-bar copy. HUD, not a debug dump.</summary>
    public static class HudStatusCopy
    {
        public static string LocationLine(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return "Pretoria";
            if (displayName.IndexOf("Toronto", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return "Toronto";
            if (displayName.IndexOf("Palo", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return "Palo Alto";
            if (displayName.IndexOf("Pretoria", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return "Pretoria";
            int comma = displayName.IndexOf(',');
            return comma > 0 ? displayName.Substring(0, comma).Trim() : displayName;
        }

        public static string ActLine(int actNumber, string place)
        {
            if (string.IsNullOrEmpty(place))
                return "Act " + actNumber;
            return "Act " + actNumber + " · " + place;
        }

        public static string ActLineForLocation(string locationId)
        {
            if (locationId == PrototypeContent.LocationToronto)
                return ActLine(2, "Toronto");
            if (locationId == PrototypeContent.LocationPaloAlto)
                return ActLine(3, "Palo Alto");
            return ActLine(1, "Home");
        }

        public static bool LooksLikeDebugStatus(string copy)
        {
            if (string.IsNullOrEmpty(copy))
                return false;
            return copy.IndexOf("(Story)", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || copy.IndexOf("Home - Pretoria", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    /// <summary>Dialogue lives in a cinematic bottom strip, not a half-frame box.</summary>
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

        public static bool IsBottomBand()
        {
            return AnchorMinY == 0f && AnchorMaxY > 0.12f && AnchorMaxY <= 0.28f;
        }

        public const int ChoiceLeft = 118;
        public const int ChoiceRightPad = 16;
        public const int ChoiceRowHeight = 26;
        public const int ChoiceRowGap = 4;
        public const int MinChoiceRows = 2;
        public const int MaxChoiceRows = 3;
        public const int ContinueWidth = 110;
        public const int ContinueHeight = 24;
        public const int ContinueBottomPad = 8;

        public static float StripInnerHeight()
        {
            return AnchorMaxY * UiStyleTokens.ReferenceHeight - BottomPad;
        }

        public static float ChoiceStackHeight(int count)
        {
            if (count < 1)
                return 0f;
            return count * ChoiceRowHeight + (count - 1) * ChoiceRowGap;
        }

        /// <summary>Bottom padding of the choice stack; occupies the Continue band when Continue is hidden.</summary>
        public static float ChoiceStackBottom => ContinueBottomPad;

        public static HudRect ContinueBand()
        {
            float innerH = StripInnerHeight();
            float y = innerH - ContinueBottomPad - ContinueHeight;
            float x = UiStyleTokens.ReferenceWidth - ChoiceRightPad - ContinueWidth;
            return new HudRect(x, y, ContinueWidth, ContinueHeight);
        }

        /// <summary>Choice row i of count, Y-down in strip inner space. Packed from the Continue band upward.</summary>
        public static HudRect ChoiceRow(int index, int count)
        {
            if (count < MinChoiceRows)
                count = MinChoiceRows;
            if (count > MaxChoiceRows)
                count = MaxChoiceRows;
            if (index < 0 || index >= count)
                return new HudRect(0, 0, 0, 0);
            float innerH = StripInnerHeight();
            float stackH = ChoiceStackHeight(count);
            float topOfStack = innerH - ChoiceStackBottom - stackH;
            float y = topOfStack + index * (ChoiceRowHeight + ChoiceRowGap);
            float w = UiStyleTokens.ReferenceWidth - ChoiceLeft - ChoiceRightPad;
            return new HudRect(ChoiceLeft, y, w, ChoiceRowHeight);
        }

        public static bool ChoiceRowsFitInsideStrip(int count)
        {
            if (count < MinChoiceRows || count > MaxChoiceRows)
                return false;
            float innerH = StripInnerHeight();
            float width = UiStyleTokens.ReferenceWidth;
            var cont = ContinueBand();
            if (!cont.FullyInside(width, innerH, 0f))
                return false;
            for (int i = 0; i < count; i++)
            {
                var row = ChoiceRow(i, count);
                if (row.H != ChoiceRowHeight)
                    return false;
                if (!row.FullyInside(width, innerH, 0f))
                    return false;
            }

            var last = ChoiceRow(count - 1, count);
            return last.Bottom <= innerH + 0.01f && last.Bottom + 0.01f >= cont.Y;
        }
    }

    /// <summary>Dialogue strip portrait: era Elon from Resources, keyed by speaker + location.</summary>
    public static class DialoguePortrait
    {
        public static string ResourceKey(string speaker, string locationId)
        {
            _ = speaker;
            if (string.IsNullOrEmpty(locationId))
                locationId = PrototypeContent.LocationPretoria;
            return ElonEraResolver.PortraitResourceKey(locationId);
        }

        public static bool UsesShippedEraPortrait(string speaker, string locationId)
        {
            return ResourceKey(speaker, locationId) == ElonEraResolver.PortraitResourceKey(
                string.IsNullOrEmpty(locationId) ? PrototypeContent.LocationPretoria : locationId);
        }
    }

    /// <summary>Title-screen copy and CTA roles. No PLACEHOLDER.</summary>
    public static class TitleScreenCopy
    {
        public static string Title => UiStyleTokens.GameTitle;
        public static string Tagline => UiStyleTokens.GameSubtitle;
        public static string PrimaryCta => UiStyleTokens.PrimaryCta;
        public static string SecondaryCta => UiStyleTokens.SecondaryCta;

        public static bool IsPlaceholder(string copy)
        {
            if (string.IsNullOrEmpty(copy))
                return false;
            return copy.IndexOf("PLACEHOLDER", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsValidTitleScreen()
        {
            return Tagline == "From Pretoria to Mars"
                   && PrimaryCta == "New Game"
                   && SecondaryCta == "Quit"
                   && !IsPlaceholder(Title)
                   && !IsPlaceholder(Tagline);
        }
    }
}
