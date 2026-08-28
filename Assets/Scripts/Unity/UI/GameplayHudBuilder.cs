using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Builds the in-game HUD using <see cref="UiTheme"/> so it matches the main menu.
    /// Top bar: Inbox / Map / Companies / Story plus location and act. One centered sheet at a time.
    /// </summary>
    public sealed class GameplayHudBuilder : MonoBehaviour
    {
        public const string CanvasName = "HUD_Canvas";

        private void Awake()
        {
            if ((GameObject.Find(CanvasName) != null || GameObject.Find("HUD_Canvas_PLACEHOLDER") != null) &&
                FindFirstObjectByType<InboxUI>() != null)
            {
                return;
            }

            Build();
        }

        private void Build()
        {
            var session = GameBootstrap.RequireSession();

            var canvasGo = UiTheme.CreateCanvas(CanvasName, 50);
            EnsureEventSystem();

            var overlay = UiTheme.CreateDimOverlay(canvasGo.transform, "DimOverlay");
            overlay.SetActive(false);

            float barH = TopBarLayout.BarHeight;
            var topBar = UiTheme.CreatePanel(canvasGo.transform, "TopBar",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -barH), new Vector2(0, 0), UiTheme.TopBarFill);

            var navInbox = CreateTopBarNav(topBar.transform, "NavInbox", 0);
            var navMap = CreateTopBarNav(topBar.transform, "NavMap", 1);
            var navCompanies = CreateTopBarNav(topBar.transform, "NavCompanies", 2);
            var navStory = CreateTopBarNav(topBar.transform, "NavStory", 3);

            var locRect = TopBarLayout.LocationStatus();
            var locLabel = UiTheme.CreateText(topBar.transform, "LocationLabel",
                HudStatusCopy.LocationLine("Pretoria"),
                UiStyleTokens.TopBarLabelFontSize, Vector2.zero, new Vector2(locRect.W, locRect.H), TextAnchor.MiddleRight);
            locLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            PlaceStatusRect(locLabel.rectTransform, locRect);

            var actRect = TopBarLayout.ActStatus();
            var storyStatus = UiTheme.CreateText(topBar.transform, "StoryStatus",
                HudStatusCopy.ActLineForLocation(PrototypeContent.LocationPretoria),
                UiStyleTokens.CaptionFontSize, Vector2.zero, new Vector2(actRect.W, actRect.H), TextAnchor.MiddleRight);
            storyStatus.color = UiTheme.Muted;
            storyStatus.horizontalOverflow = HorizontalWrapMode.Overflow;
            PlaceStatusRect(storyStatus.rectTransform, actRect);

            var inboxPanel = UiTheme.CreateCenteredSheet(canvasGo.transform, "InboxPanel");
            UiTheme.AddSheetHeader(inboxPanel, "Inbox", "InboxClose", out _);
            var inboxList = UiTheme.CreateText(inboxPanel.transform, "InboxList", "",
                UiStyleTokens.BodyFontSize, new Vector2(16, -48), new Vector2(-32, 160), TextAnchor.UpperLeft);
            UiTheme.Stretch(inboxList.rectTransform, new Vector2(0, 0.50f), new Vector2(1, 1),
                new Vector2(20, 8), new Vector2(-20, -48));
            var inboxDetail = UiTheme.CreateText(inboxPanel.transform, "InboxDetail", "",
                UiStyleTokens.CaptionFontSize, new Vector2(16, -8), new Vector2(-32, 120), TextAnchor.UpperLeft);
            inboxDetail.color = UiTheme.Muted;
            UiTheme.Stretch(inboxDetail.rectTransform, new Vector2(0, 0.16f), new Vector2(1, 0.50f),
                new Vector2(20, 8), new Vector2(-20, -8));
            var acceptBtn = UiTheme.CreateButton(inboxPanel.transform, "AcceptButton", "Accept",
                new Vector2(20, 14), new Vector2(100, 32), true);
            var resolveBtn = UiTheme.CreateButton(inboxPanel.transform, "ResolveButton", "Resolve",
                new Vector2(128, 14), new Vector2(100, 32), true);
            var prevTicket = UiTheme.CreateButton(inboxPanel.transform, "PrevTicket", "Prev",
                new Vector2(236, 14), new Vector2(64, 32), false);
            var nextTicket = UiTheme.CreateButton(inboxPanel.transform, "NextTicket", "Next",
                new Vector2(308, 14), new Vector2(64, 32), false);
            var inboxClose = inboxPanel.transform.Find("InboxClose").GetComponent<Button>();

            var travelPanel = UiTheme.CreateCenteredSheet(canvasGo.transform, "TravelPanel");
            UiTheme.AddSheetHeader(travelPanel, "Map", "TravelClose", out _);
            var travelList = UiTheme.CreateText(travelPanel.transform, "TravelList", "",
                UiStyleTokens.BodyFontSize, new Vector2(16, -48), new Vector2(-32, 200), TextAnchor.UpperLeft);
            UiTheme.Stretch(travelList.rectTransform, new Vector2(0, 0.16f), new Vector2(1, 1),
                new Vector2(20, 8), new Vector2(-20, -48));
            var travelBtn = UiTheme.CreateButton(travelPanel.transform, "TravelButton", "Travel",
                new Vector2(20, 14), new Vector2(120, 32), true);
            var prevLoc = UiTheme.CreateButton(travelPanel.transform, "PrevLocation", "Prev",
                new Vector2(148, 14), new Vector2(80, 32), false);
            var nextLoc = UiTheme.CreateButton(travelPanel.transform, "NextLocation", "Next",
                new Vector2(236, 14), new Vector2(80, 32), false);
            var travelClose = travelPanel.transform.Find("TravelClose").GetComponent<Button>();

            var coPanel = UiTheme.CreateCenteredSheet(canvasGo.transform, "CompaniesPanel");
            UiTheme.AddSheetHeader(coPanel, "Companies", "CompaniesClose", out _);
            var coBody = UiTheme.CreateText(coPanel.transform, "CompaniesBody", "",
                UiStyleTokens.BodyFontSize, new Vector2(16, -48), new Vector2(-32, 280), TextAnchor.UpperLeft);
            UiTheme.Stretch(coBody.rectTransform, new Vector2(0, 0.16f), new Vector2(1, 1),
                new Vector2(20, 8), new Vector2(-20, -48));
            var foundZip2 = UiTheme.CreateButton(coPanel.transform, "FoundZip2", "Found Zip2",
                new Vector2(20, 14), new Vector2(140, 32), true);
            var foundX = UiTheme.CreateButton(coPanel.transform, "FoundXCom", "Found X.com",
                new Vector2(168, 14), new Vector2(140, 32), true);
            var coClose = coPanel.transform.Find("CompaniesClose").GetComponent<Button>();

            var resolvePanel = UiTheme.CreateCenteredSheet(canvasGo.transform, "ProblemResolvePanel");
            UiTheme.AddSheetHeader(resolvePanel, "Problem", "ResolveClose", out var resHeader);
            var resBody = UiTheme.CreateText(resolvePanel.transform, "ResolveBody", "",
                UiStyleTokens.BodyFontSize, new Vector2(16, -48), new Vector2(-32, 120), TextAnchor.UpperLeft);
            UiTheme.Stretch(resBody.rectTransform, new Vector2(0, 0.42f), new Vector2(1, 1),
                new Vector2(20, 8), new Vector2(-20, -48));
            var resChoices = new GameObject("ResolveChoices", typeof(RectTransform), typeof(VerticalLayoutGroup));
            resChoices.transform.SetParent(resolvePanel.transform, false);
            UiTheme.Stretch(resChoices.GetComponent<RectTransform>(), new Vector2(0, 0.04f), new Vector2(1, 0.42f),
                new Vector2(20, 14), new Vector2(-20, -8));
            var rVlg = resChoices.GetComponent<VerticalLayoutGroup>();
            rVlg.spacing = UiStyleTokens.ButtonGap;
            rVlg.childControlWidth = true;
            rVlg.childControlHeight = false;
            rVlg.childForceExpandWidth = true;
            var resClose = resolvePanel.transform.Find("ResolveClose").GetComponent<Button>();

            var menuPanel = UiTheme.CreateCenteredSheet(canvasGo.transform, "MenuPanel");
            var menuClose = UiTheme.AddSheetHeader(menuPanel, "Menu", "MenuClose", out _);
            var menuButtons = new GameObject("MenuButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
            menuButtons.transform.SetParent(menuPanel.transform, false);
            UiTheme.Stretch(menuButtons.GetComponent<RectTransform>(), new Vector2(0.18f, 0.08f), new Vector2(0.82f, 0.82f),
                new Vector2(0, 12), new Vector2(0, -12));
            var mVlg = menuButtons.GetComponent<VerticalLayoutGroup>();
            mVlg.spacing = 10;
            mVlg.childAlignment = TextAnchor.MiddleCenter;
            mVlg.childControlWidth = true;
            mVlg.childControlHeight = false;
            mVlg.childForceExpandWidth = true;
            var menuInbox = CreateMenuButton(menuButtons.transform, "MenuInbox", "Inbox", true);
            var menuMap = CreateMenuButton(menuButtons.transform, "MenuMap", "Map", true);
            var menuCompanies = CreateMenuButton(menuButtons.transform, "MenuCompanies", "Companies", true);
            var menuStory = CreateMenuButton(menuButtons.transform, "MenuStory", "Story", true);
            var menuResume = CreateMenuButton(menuButtons.transform, "MenuResume", "Resume", false);

            var storyPanel = UiTheme.CreateCenteredSheet(canvasGo.transform, "StoryPanel");
            var storyClose = UiTheme.AddSheetHeader(storyPanel, "Story", "StoryClose", out _);
            var storySheetStatus = UiTheme.CreateText(storyPanel.transform, "StorySheetStatus", "Act 1",
                UiStyleTokens.BodyFontSize, new Vector2(20, -56), new Vector2(-40, 200), TextAnchor.UpperLeft);
            storySheetStatus.color = UiTheme.Muted;
            UiTheme.Stretch(storySheetStatus.rectTransform, new Vector2(0, 0.22f), new Vector2(1, 1),
                new Vector2(20, 8), new Vector2(-20, -48));
            var storyContinue = UiTheme.CreateButton(storyPanel.transform, "StoryContinue", "Continue",
                new Vector2(20, 14), new Vector2(140, 32), true);

            var dialoguePanel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            dialoguePanel.transform.SetParent(canvasGo.transform, false);
            var dRt = dialoguePanel.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0f, DialogueStripLayout.AnchorMinY);
            dRt.anchorMax = new Vector2(1f, DialogueStripLayout.AnchorMaxY);
            dRt.offsetMin = new Vector2(0f, DialogueStripLayout.BottomPad);
            dRt.offsetMax = Vector2.zero;
            var dImg = dialoguePanel.GetComponent<Image>();
            dImg.color = UiTheme.DialogueFill;
            dImg.raycastTarget = true;
            UiTheme.CreateHairline(dialoguePanel.transform, "DialogueTopEdge",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, 1), new Color(UiTheme.Border.r, UiTheme.Border.g, UiTheme.Border.b, 0.35f));

            var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGo.transform.SetParent(dialoguePanel.transform, false);
            var portraitRt = portraitGo.GetComponent<RectTransform>();
            portraitRt.anchorMin = new Vector2(0, 0);
            portraitRt.anchorMax = new Vector2(0, 1);
            portraitRt.pivot = new Vector2(0, 0.5f);
            portraitRt.anchoredPosition = new Vector2(16, 0);
            portraitRt.sizeDelta = new Vector2(88, -20);
            var portraitImg = portraitGo.GetComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.preserveAspect = true;
            portraitImg.raycastTarget = false;

            var speaker = UiTheme.CreateText(dialoguePanel.transform, "Speaker", "",
                UiStyleTokens.CaptionFontSize, new Vector2(118, -10), new Vector2(420, 18), TextAnchor.MiddleLeft);
            speaker.color = UiTheme.Accent;
            speaker.horizontalOverflow = HorizontalWrapMode.Overflow;
            var body = UiTheme.CreateText(dialoguePanel.transform, "Body", "…",
                UiStyleTokens.BodyFontSize, new Vector2(118, -32), new Vector2(-140, 56), TextAnchor.UpperLeft);
            body.color = UiTheme.Title;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiTheme.Stretch(body.rectTransform, new Vector2(0, 0.36f), new Vector2(1, 0.82f),
                new Vector2(118, 8), new Vector2(-120, -6));
            var contBtn = UiTheme.CreateGhostButton(dialoguePanel.transform, "ContinueButton", "Continue",
                new Vector2(-16, 10), new Vector2(110, 24), new Vector2(1, 0), new Vector2(1, 0));

            var choicesRoot = new GameObject("ChoicesRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            choicesRoot.transform.SetParent(dialoguePanel.transform, false);
            UiTheme.Stretch(choicesRoot.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0.36f),
                new Vector2(118, 36), new Vector2(-16, 4));
            var vlg = choicesRoot.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            var inboxUi = canvasGo.AddComponent<InboxUI>();
            inboxUi.Bind(session, inboxPanel, inboxList, inboxDetail, acceptBtn, inboxClose, null);
            inboxUi.BindResolveButton(resolveBtn);
            inboxUi.BindNavButtons(prevTicket, nextTicket);

            var travelUi = canvasGo.AddComponent<TravelMapUI>();
            travelUi.Bind(session, travelPanel, travelList, travelBtn, travelClose, null);
            travelUi.BindLocationNav(prevLoc, nextLoc);

            var dialogueUi = canvasGo.AddComponent<DialogueUI>();
            dialogueUi.Bind(dialoguePanel, speaker, body, contBtn, choicesRoot.transform, portraitImg);

            var coUi = canvasGo.AddComponent<CompanyDashboardUI>();
            coUi.Bind(session, coPanel, coBody, foundZip2, foundX, coClose, null);

            var resolveUi = canvasGo.AddComponent<ProblemResolveUI>();
            resolveUi.Bind(resolvePanel, resHeader, resBody, resChoices.transform, resClose);

            var storyUi = canvasGo.AddComponent<Act1StoryUI>();
            storyUi.Bind(session, storyContinue, storySheetStatus, null, storyClose);

            canvasGo.AddComponent<HudLocationLabel>().Init(locLabel, storyStatus);

            var hud = canvasGo.AddComponent<HudPanelController>();
            hud.Bind(topBar, inboxPanel, travelPanel, coPanel, resolvePanel, dialoguePanel, menuPanel, storyPanel, overlay);
            hud.BindNav(navInbox, navMap, navCompanies, navStory);

            navInbox.onClick.AddListener(() => inboxUi.Toggle());
            navMap.onClick.AddListener(() => travelUi.Toggle());
            navCompanies.onClick.AddListener(() => coUi.Toggle());
            navStory.onClick.AddListener(() =>
            {
                hud.Toggle(HudLargePanel.Story);
                if (hud.IsOpen(HudLargePanel.Story))
                    storyUi.RefreshStatus();
            });

            menuInbox.onClick.AddListener(() => inboxUi.Show());
            menuMap.onClick.AddListener(() => travelUi.Show());
            menuCompanies.onClick.AddListener(() => coUi.Show());
            menuStory.onClick.AddListener(() =>
            {
                hud.Open(HudLargePanel.Story);
                storyUi.RefreshStatus();
            });
            menuResume.onClick.AddListener(() => hud.Close());
            menuClose.onClick.AddListener(() => hud.Close());

            inboxPanel.SetActive(false);
            travelPanel.SetActive(false);
            coPanel.SetActive(false);
            resolvePanel.SetActive(false);
            menuPanel.SetActive(false);
            storyPanel.SetActive(false);
            dialoguePanel.SetActive(false);
            overlay.SetActive(false);
            topBar.SetActive(true);

            DontDestroyOnLoad(canvasGo);
        }

        private static Button CreateTopBarNav(Transform parent, string name, int index)
        {
            var rect = TopBarLayout.NavButton(index);
            var btn = UiTheme.CreateButton(
                parent,
                name,
                TopBarLayout.NavLabels[index],
                new Vector2(rect.X, TopBarLayout.UnityYFromBottom(rect)),
                new Vector2(rect.W, rect.H),
                primary: false);
            UiTheme.ApplyNavVisual(btn, false);
            return btn;
        }

        private static void PlaceStatusRect(RectTransform rt, HudRect rect)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(rect.W, rect.H);
            rt.anchoredPosition = new Vector2(rect.X, TopBarLayout.UnityYFromBottom(rect));
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, bool primary)
        {
            var btn = UiTheme.CreateButton(parent, name, label, Vector2.zero, new Vector2(0, 40), primary);
            var rt = btn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 40);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 40;
            le.minHeight = 40;
            le.flexibleWidth = 1f;
            return btn;
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
    }

    public sealed class HudLocationLabel : MonoBehaviour
    {
        private Text _label;
        private Text _act;

        public void Init(Text label, Text act = null)
        {
            _label = label;
            _act = act;
            Refresh();
        }

        private void OnEnable() => Refresh();

        private void Refresh()
        {
            var session = GameBootstrap.RequireSession();
            var loc = session?.Travel.CurrentLocation;
            string id = loc?.Id ?? PrototypeContent.LocationPretoria;
            string display = loc != null ? loc.DisplayName : "Pretoria, South Africa";
            if (_label != null)
                _label.text = HudStatusCopy.LocationLine(display);
            if (_act != null)
                _act.text = HudStatusCopy.ActLineForLocation(id);
        }
    }
}
