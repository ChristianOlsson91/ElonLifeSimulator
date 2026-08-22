using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Builds a functional runtime HUD (Inbox, Travel, Dialogue, Companies, Story, Problem resolve).
    /// PLACEHOLDER UI — replace with designed pixel panels later.
    /// </summary>
    public sealed class GameplayHudBuilder : MonoBehaviour
    {
        private void Awake()
        {
            if (GameObject.Find("HUD_Canvas_PLACEHOLDER") != null &&
                FindFirstObjectByType<InboxUI>() != null)
            {
                return;
            }

            Build();
        }

        private void Build()
        {
            var session = GameBootstrap.RequireSession();

            var canvasGo = new GameObject("HUD_Canvas_PLACEHOLDER", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            EnsureEventSystem();

            // Top bar
            var topBar = CreatePanel(canvasGo.transform, "TopBar", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -52), new Vector2(0, 0), new Color(0.05f, 0.05f, 0.08f, 0.9f));

            var inboxToggle = CreateButton(topBar.transform, "InboxToggle", "Inbox", new Vector2(8, -10), new Vector2(90, 32));
            var mapToggle = CreateButton(topBar.transform, "MapToggle", "Map", new Vector2(104, -10), new Vector2(80, 32));
            var coToggle = CreateButton(topBar.transform, "CompaniesToggle", "Companies", new Vector2(190, -10), new Vector2(110, 32));
            var storyBtn = CreateButton(topBar.transform, "StoryButton", "Story", new Vector2(308, -10), new Vector2(80, 32));
            var locLabel = CreateText(topBar.transform, "LocationLabel", "Location", 14,
                new Vector2(400, -8), new Vector2(360, 28), TextAnchor.MiddleLeft);
            var storyStatus = CreateText(topBar.transform, "StoryStatus", "Act 1", 12,
                new Vector2(400, -30), new Vector2(500, 20), TextAnchor.MiddleLeft);
            storyStatus.color = new Color(1f, 0.9f, 0.5f, 1f);

            // Inbox panel
            var inboxPanel = CreatePanel(canvasGo.transform, "InboxPanel", new Vector2(0, 0), new Vector2(0.42f, 0.82f),
                new Vector2(12, 12), new Vector2(-6, -56), new Color(0.08f, 0.1f, 0.14f, 0.95f));
            var inboxList = CreateText(inboxPanel.transform, "InboxList", "INBOX", 13,
                new Vector2(8, -8), new Vector2(-16, 160), TextAnchor.UpperLeft);
            Stretch(inboxList.rectTransform, new Vector2(0, 0.48f), new Vector2(1, 1), new Vector2(8, 8), new Vector2(-8, -8));
            var inboxDetail = CreateText(inboxPanel.transform, "InboxDetail", "Detail", 12,
                new Vector2(8, -8), new Vector2(-16, 120), TextAnchor.UpperLeft);
            Stretch(inboxDetail.rectTransform, new Vector2(0, 0.18f), new Vector2(1, 0.48f), new Vector2(8, 8), new Vector2(-8, -8));
            var acceptBtn = CreateButton(inboxPanel.transform, "AcceptButton", "Accept", new Vector2(10, 10), new Vector2(90, 34));
            var resolveBtn = CreateButton(inboxPanel.transform, "ResolveButton", "Resolve", new Vector2(106, 10), new Vector2(90, 34));
            var prevTicket = CreateButton(inboxPanel.transform, "PrevTicket", "Prev", new Vector2(202, 10), new Vector2(60, 34));
            var nextTicket = CreateButton(inboxPanel.transform, "NextTicket", "Next", new Vector2(268, 10), new Vector2(60, 34));
            var inboxClose = CreateButton(inboxPanel.transform, "InboxClose", "Close", new Vector2(334, 10), new Vector2(70, 34));

            // Travel panel
            var travelPanel = CreatePanel(canvasGo.transform, "TravelPanel", new Vector2(0.45f, 0), new Vector2(1, 0.82f),
                new Vector2(6, 12), new Vector2(-12, -56), new Color(0.08f, 0.12f, 0.1f, 0.95f));
            var travelList = CreateText(travelPanel.transform, "TravelList", "MAP", 13,
                new Vector2(8, -8), new Vector2(-16, 200), TextAnchor.UpperLeft);
            Stretch(travelList.rectTransform, new Vector2(0, 0.2f), new Vector2(1, 1), new Vector2(8, 8), new Vector2(-8, -8));
            var travelBtn = CreateButton(travelPanel.transform, "TravelButton", "Travel", new Vector2(12, 12), new Vector2(120, 36));
            var nextLoc = CreateButton(travelPanel.transform, "NextLocation", "Next Loc", new Vector2(140, 12), new Vector2(100, 36));
            var travelClose = CreateButton(travelPanel.transform, "TravelClose", "Close", new Vector2(250, 12), new Vector2(90, 36));

            // Companies panel
            var coPanel = CreatePanel(canvasGo.transform, "CompaniesPanel", new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f),
                new Vector2(0, 0), new Vector2(0, -56), new Color(0.1f, 0.1f, 0.16f, 0.96f));
            var coBody = CreateText(coPanel.transform, "CompaniesBody", "Companies", 13,
                new Vector2(12, -12), new Vector2(-24, 280), TextAnchor.UpperLeft);
            Stretch(coBody.rectTransform, new Vector2(0, 0.22f), new Vector2(1, 1), new Vector2(12, 12), new Vector2(-12, -12));
            var foundZip2 = CreateButton(coPanel.transform, "FoundZip2", "Found Zip2", new Vector2(12, 12), new Vector2(140, 36));
            var foundX = CreateButton(coPanel.transform, "FoundXCom", "Found X.com", new Vector2(160, 12), new Vector2(140, 36));
            var coClose = CreateButton(coPanel.transform, "CompaniesClose", "Close", new Vector2(310, 12), new Vector2(100, 36));

            // Problem resolve panel
            var resolvePanel = CreatePanel(canvasGo.transform, "ProblemResolvePanel", new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.88f),
                new Vector2(0, 0), new Vector2(0, -56), new Color(0.09f, 0.12f, 0.1f, 0.97f));
            var resHeader = CreateText(resolvePanel.transform, "ResolveHeader", "Problem", 18,
                new Vector2(16, -12), new Vector2(500, 32), TextAnchor.UpperLeft);
            var resBody = CreateText(resolvePanel.transform, "ResolveBody", "", 13,
                new Vector2(16, -48), new Vector2(-32, 120), TextAnchor.UpperLeft);
            Stretch(resBody.rectTransform, new Vector2(0, 0.45f), new Vector2(1, 0.9f), new Vector2(16, 8), new Vector2(-16, -8));
            var resChoices = new GameObject("ResolveChoices", typeof(RectTransform), typeof(VerticalLayoutGroup));
            resChoices.transform.SetParent(resolvePanel.transform, false);
            Stretch(resChoices.GetComponent<RectTransform>(), new Vector2(0, 0.12f), new Vector2(1, 0.45f),
                new Vector2(16, 8), new Vector2(-16, -8));
            var rVlg = resChoices.GetComponent<VerticalLayoutGroup>();
            rVlg.spacing = 6;
            rVlg.childControlWidth = true;
            rVlg.childControlHeight = false;
            var resClose = CreateButton(resolvePanel.transform, "ResolveClose", "Close", new Vector2(16, 12), new Vector2(100, 34));

            // Dialogue panel (bottom)
            var dialoguePanel = CreatePanel(canvasGo.transform, "DialoguePanel", new Vector2(0.08f, 0), new Vector2(0.92f, 0.34f),
                new Vector2(0, 10), new Vector2(0, 10), new Color(0.05f, 0.05f, 0.1f, 0.96f));

            // Portrait (left)
            var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGo.transform.SetParent(dialoguePanel.transform, false);
            var portraitRt = portraitGo.GetComponent<RectTransform>();
            portraitRt.anchorMin = new Vector2(0, 0);
            portraitRt.anchorMax = new Vector2(0, 1);
            portraitRt.pivot = new Vector2(0, 0.5f);
            portraitRt.anchoredPosition = new Vector2(12, 0);
            portraitRt.sizeDelta = new Vector2(96, -24);
            var portraitImg = portraitGo.GetComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.preserveAspect = true;

            var speaker = CreateText(dialoguePanel.transform, "Speaker", "Speaker", 16,
                new Vector2(120, -12), new Vector2(400, 28), TextAnchor.UpperLeft);
            var body = CreateText(dialoguePanel.transform, "Body", "…", 14,
                new Vector2(120, -44), new Vector2(-32, 80), TextAnchor.UpperLeft);
            Stretch(body.rectTransform, new Vector2(0, 0.35f), new Vector2(1, 0.85f), new Vector2(120, 8), new Vector2(-16, -8));
            var contBtn = CreateButton(dialoguePanel.transform, "ContinueButton", "Continue", new Vector2(-140, 12), new Vector2(120, 32));
            var contRt = contBtn.GetComponent<RectTransform>();
            contRt.anchorMin = new Vector2(1, 0);
            contRt.anchorMax = new Vector2(1, 0);
            contRt.pivot = new Vector2(1, 0);
            contRt.anchoredPosition = new Vector2(-16, 12);

            var choicesRoot = new GameObject("ChoicesRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            choicesRoot.transform.SetParent(dialoguePanel.transform, false);
            Stretch(choicesRoot.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0.35f),
                new Vector2(120, 48), new Vector2(-16, 8));
            var vlg = choicesRoot.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            // Wire
            var inboxUi = canvasGo.AddComponent<InboxUI>();
            inboxUi.Bind(session, inboxPanel, inboxList, inboxDetail, acceptBtn, inboxClose, inboxToggle);
            inboxUi.BindResolveButton(resolveBtn);
            inboxUi.BindNavButtons(prevTicket, nextTicket);

            var travelUi = canvasGo.AddComponent<TravelMapUI>();
            travelUi.Bind(session, travelPanel, travelList, travelBtn, travelClose, mapToggle);
            travelUi.BindLocationNav(null, nextLoc);

            var dialogueUi = canvasGo.AddComponent<DialogueUI>();
            dialogueUi.Bind(dialoguePanel, speaker, body, contBtn, choicesRoot.transform, portraitImg);

            var coUi = canvasGo.AddComponent<CompanyDashboardUI>();
            coUi.Bind(session, coPanel, coBody, foundZip2, foundX, coClose, coToggle);

            var resolveUi = canvasGo.AddComponent<ProblemResolveUI>();
            resolveUi.Bind(resolvePanel, resHeader, resBody, resChoices.transform, resClose);

            var storyUi = canvasGo.AddComponent<Act1StoryUI>();
            storyUi.Bind(session, storyBtn, storyStatus);

            canvasGo.AddComponent<HudLocationLabel>().Init(locLabel);

            var hud = canvasGo.AddComponent<HudPanelController>();
            hud.Bind(topBar, inboxPanel, travelPanel, coPanel, resolvePanel, dialoguePanel);

            inboxPanel.SetActive(false);
            travelPanel.SetActive(false);
            coPanel.SetActive(false);
            resolvePanel.SetActive(false);
            dialoguePanel.SetActive(false);
            topBar.SetActive(true);
            topBar.transform.SetAsLastSibling();

            DontDestroyOnLoad(canvasGo);

            Debug.Log("[ElonLifeSim] PLACEHOLDER HUD built (Inbox/Map/Companies/Story/Resolve).");
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                return;
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            DontDestroyOnLoad(es);
        }

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.2f, 0.35f, 0.55f, 1f);

            var text = CreateText(go.transform, "Label", label, 13, Vector2.zero, size, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return go.GetComponent<Button>();
        }

        private static Text CreateText(Transform parent, string name, string content, int size,
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
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null)
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = content;
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            return t;
        }

        private static void Stretch(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = oMin;
            rt.offsetMax = oMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    public sealed class HudLocationLabel : MonoBehaviour
    {
        private Text _label;

        public void Init(Text label)
        {
            _label = label;
            Refresh();
        }

        private void OnEnable() => Refresh();

        private void Refresh()
        {
            if (_label == null) return;
            var session = GameBootstrap.RequireSession();
            if (session?.Travel.CurrentLocation != null)
                _label.text = session.Travel.CurrentLocation.DisplayName;
        }
    }
}
