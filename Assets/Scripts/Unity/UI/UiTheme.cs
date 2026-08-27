using ElonLifeSim.Core.Content;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Unity helpers that consume <see cref="UiStyleTokens"/> so main menu and HUD match.
    /// </summary>
    public static class UiTheme
    {
        public static Color ScreenBackground =>
            new Color(UiStyleTokens.ScreenBgR, UiStyleTokens.ScreenBgG, UiStyleTokens.ScreenBgB, 1f);

        public static Color PanelFill =>
            new Color(UiStyleTokens.PanelR, UiStyleTokens.PanelG, UiStyleTokens.PanelB, UiStyleTokens.PanelA);

        public static Color TopBarFill =>
            new Color(UiStyleTokens.TopBarR, UiStyleTokens.TopBarG, UiStyleTokens.TopBarB, UiStyleTokens.TopBarA);

        public static Color OverlayFill =>
            new Color(UiStyleTokens.OverlayR, UiStyleTokens.OverlayG, UiStyleTokens.OverlayB, UiStyleTokens.OverlayA);

        public static Color Primary =>
            new Color(UiStyleTokens.PrimaryR, UiStyleTokens.PrimaryG, UiStyleTokens.PrimaryB, 1f);

        public static Color Secondary =>
            new Color(UiStyleTokens.SecondaryR, UiStyleTokens.SecondaryG, UiStyleTokens.SecondaryB, 1f);

        public static Color Title =>
            new Color(UiStyleTokens.TitleR, UiStyleTokens.TitleG, UiStyleTokens.TitleB, 1f);

        public static Color Muted =>
            new Color(UiStyleTokens.MutedR, UiStyleTokens.MutedG, UiStyleTokens.MutedB, 1f);

        public static Color Border => new Color(0.28f, 0.38f, 0.46f, 0.85f);

        public static Font UiFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Helvetica" }, 16);
            return font;
        }

        public static Color ActiveNav =>
            new Color(UiStyleTokens.ActiveNavR, UiStyleTokens.ActiveNavG, UiStyleTokens.ActiveNavB, 1f);

        public static ColorBlock ColorBlockFor(bool primary)
        {
            var normal = primary ? Primary : Secondary;
            var hover = primary
                ? new Color(0.20f, 0.58f, 0.62f, 1f)
                : new Color(0.26f, 0.28f, 0.36f, 1f);
            var pressed = primary
                ? new Color(0.10f, 0.34f, 0.38f, 1f)
                : new Color(0.12f, 0.14f, 0.18f, 1f);
            var block = ColorBlock.defaultColorBlock;
            block.normalColor = normal;
            block.highlightedColor = hover;
            block.selectedColor = hover;
            block.pressedColor = pressed;
            block.disabledColor = new Color(normal.r, normal.g, normal.b, 0.35f);
            block.colorMultiplier = 1f;
            block.fadeDuration = 0.08f;
            return block;
        }

        public static ColorBlock ColorBlockForNav(bool active)
        {
            if (!active)
                return ColorBlockFor(false);
            var block = ColorBlock.defaultColorBlock;
            block.normalColor = ActiveNav;
            block.highlightedColor = new Color(0.22f, 0.58f, 0.54f, 1f);
            block.selectedColor = ActiveNav;
            block.pressedColor = new Color(0.10f, 0.34f, 0.32f, 1f);
            block.disabledColor = new Color(ActiveNav.r, ActiveNav.g, ActiveNav.b, 0.35f);
            block.colorMultiplier = 1f;
            block.fadeDuration = 0.08f;
            return block;
        }

        public static void ApplyNavVisual(Button btn, bool active)
        {
            if (btn == null)
                return;
            btn.colors = ColorBlockForNav(active);
            var outline = btn.GetComponent<Outline>();
            if (active)
            {
                if (outline == null)
                    outline = btn.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.55f, 0.88f, 0.82f, 0.95f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);
                outline.enabled = true;
            }
            else if (outline != null)
            {
                outline.enabled = false;
            }
        }

        public static GameObject CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UiStyleTokens.ReferenceWidth, UiStyleTokens.ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;
            return go;
        }

        public static Image CreateFullBleed(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static GameObject CreateDimOverlay(Transform parent, string name)
        {
            var img = CreateFullBleed(parent, name, OverlayFill);
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            return img.gameObject;
        }

        public static Image CreateHairline(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static GameObject CreateCenteredSheet(Transform parent, string name)
        {
            return CreatePanel(
                parent,
                name,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-360f, -240f),
                new Vector2(360f, 240f),
                PanelFill);
        }

        public static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var img = go.GetComponent<Image>();
            img.color = color;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Border;
            outline.effectDistance = new Vector2(1f, -1f);
            return go;
        }

        public static Button AddSheetHeader(GameObject panel, string title, string closeName, out Text titleText)
        {
            var accent = new GameObject("HeaderAccent", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(panel.transform, false);
            var aRt = accent.GetComponent<RectTransform>();
            aRt.anchorMin = new Vector2(0, 0);
            aRt.anchorMax = new Vector2(0, 1);
            aRt.pivot = new Vector2(0, 0.5f);
            aRt.sizeDelta = new Vector2(3f, 0);
            aRt.anchoredPosition = Vector2.zero;
            accent.GetComponent<Image>().color = Primary;
            accent.GetComponent<Image>().raycastTarget = false;

            titleText = CreateText(panel.transform, "Title", title, UiStyleTokens.PanelTitleFontSize,
                new Vector2(UiStyleTokens.PanelPadding + 4, -10), new Vector2(420, 24), TextAnchor.MiddleLeft);
            titleText.color = Title;
            titleText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            titleText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            titleText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

            var rule = new GameObject("HeaderRule", typeof(RectTransform), typeof(Image));
            rule.transform.SetParent(panel.transform, false);
            var rRt = rule.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0, 1);
            rRt.anchorMax = new Vector2(1, 1);
            rRt.pivot = new Vector2(0.5f, 1);
            rRt.anchoredPosition = new Vector2(0, -UiStyleTokens.HeaderHeight + 2);
            rRt.sizeDelta = new Vector2(-32, 1);
            rule.GetComponent<Image>().color = new Color(Primary.r, Primary.g, Primary.b, 0.45f);
            rule.GetComponent<Image>().raycastTarget = false;

            var close = CreateButton(
                panel.transform,
                closeName,
                "Close",
                new Vector2(-UiStyleTokens.PanelPadding, -6),
                new Vector2(UiStyleTokens.CloseButtonWidth, UiStyleTokens.CloseButtonHeight),
                primary: false,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f));
            return close;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Vector2 pos,
            Vector2 size,
            bool primary,
            Vector2? anchor = null,
            Vector2? pivot = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            var a = anchor ?? Vector2.zero;
            var p = pivot ?? Vector2.zero;
            rt.anchorMin = a;
            rt.anchorMax = a;
            rt.pivot = p;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            btn.colors = ColorBlockFor(primary);

            var fontSize = primary ? UiStyleTokens.PrimaryButtonFontSize : UiStyleTokens.SecondaryButtonFontSize;
            if (size.y <= UiStyleTokens.TopBarButtonHeight + 1)
                fontSize = UiStyleTokens.TopBarLabelFontSize;

            var text = CreateText(go.transform, "Label", label, fontSize, Vector2.zero, size, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return btn;
        }

        public static Text CreateText(Transform parent, string name, string content, int size,
            Vector2 pos, Vector2 dim, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = dim;
            var t = go.GetComponent<Text>();
            t.font = UiFont();
            t.text = content;
            t.fontSize = size;
            t.color = Title;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.raycastTarget = false;
            return t;
        }

        public static Text CreateCenteredText(Transform parent, string name, string content, int size,
            Vector2 pos, Vector2 dim)
        {
            var t = CreateText(parent, name, content, size, pos, dim, TextAnchor.MiddleCenter);
            var rt = t.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = dim;
            return t;
        }

        public static void Stretch(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = oMin;
            rt.offsetMax = oMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        public static void StyleChoiceButton(Image img, Button btn, Text label)
        {
            img.color = Color.white;
            btn.targetGraphic = img;
            btn.colors = ColorBlockFor(false);
            if (label != null)
            {
                label.font = UiFont();
                label.color = Title;
                label.fontSize = UiStyleTokens.BodyFontSize;
            }
        }
    }
}
