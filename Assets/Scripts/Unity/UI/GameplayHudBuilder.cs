using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Builds the in-game HUD using <see cref="UiTheme"/> so it matches the main menu.
    /// Large sheets open from the Esc menu and are centered on screen.
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

            float barH = UiStyleTokens.TopBarHeight;
            var topBar = UiTheme.CreatePanel(canvasGo.transform, "TopBar",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -barH), new Vector2(0, 0), UiTheme.TopBarFill);

            UiTheme.CreateHairline(topBar.transform, "TopBarEdge",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0),
                new Vector2(0, 2), UiTheme.Primary);

            var escHint = UiTheme.CreateText(topBar.transform, "EscHint", "Esc · Menu",
                UiStyleTokens.CaptionFontSize, new Vector2(16, -18), new Vector2(160, 20), TextAnchor.MiddleLeft);
            escHint.color = UiTheme.Muted;

            var locLabel = UiTheme.CreateText(topBar.transform, "LocationLabel", "Location",
                UiStyleTokens.TopBarLabelFontSize, new Vector2(-16, -10), new Vector2(420, 22), TextAnchor.MiddleRight);
            locLabel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            locLabel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            locLabel.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

            var storyStatus = UiTheme.CreateText(topBar.transform, "StoryStatus", "Act 1",
                UiStyleTokens.CaptionFontSize, new Vector2(-16, -32), new Vector2(520, 18), TextAnchor.MiddleRight);
            storyStatus.color = UiTheme.Muted;
            storyStatus.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            storyStatus.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            storyStatus.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

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

            var dialoguePanel = UiTheme.CreatePanel(canvasGo.transform, "DialoguePanel",
                new Vector2(0.10f, 0), new Vector2(0.90f, 0.26f),
                new Vector2(0, 16), new Vector2(0, 0), UiTheme.PanelFill);

            var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGo.transform.SetParent(dialoguePanel.transform, false);
            var portraitRt = portraitGo.GetComponent<RectTransform>();
            portraitRt.anchorMin = new Vector2(0, 0);
            portraitRt.anchorMax = new Vector2(0, 1);
            portraitRt.pivot = new Vector2(0, 0.5f);
            portraitRt.anchoredPosition = new Vector2(14, 0);
            portraitRt.sizeDelta = new Vector2(92, -24);
            var portraitImg = portraitGo.GetComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.preserveAspect = true;
            portraitImg.raycastTarget = false;

            var speaker = UiTheme.CreateText(dialoguePanel.transform, "Speaker", "Speaker",
                UiStyleTokens.PanelTitleFontSize, new Vector2(118, -12), new Vector2(400, 24), TextAnchor.UpperLeft);
            var body = UiTheme.CreateText(dialoguePanel.transform, "Body", "…",
                UiStyleTokens.BodyFontSize, new Vector2(118, -38), new Vector2(-32, 80), TextAnchor.UpperLeft);
            body.color = UiTheme.Muted;
            UiTheme.Stretch(body.rectTransform, new Vector2(0, 0.38f), new Vector2(1, 0.82f),
                new Vector2(118, 6), new Vector2(-18, -8));
            var contBtn = UiTheme.CreateButton(dialoguePanel.transform, "ContinueButton", "Continue",
                new Vector2(-18, 12), new Vector2(120, 32), true, new Vector2(1, 0), new Vector2(1, 0));

            var choicesRoot = new GameObject("ChoicesRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            choicesRoot.transform.SetParent(dialoguePanel.transform, false);
            UiTheme.Stretch(choicesRoot.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0.38f),
                new Vector2(118, 48), new Vector2(-18, 6));
            var vlg = choicesRoot.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.UpperCenter;
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
            storyUi.Bind(session, storyContinue, storySheetStatus, storyStatus, storyClose);

            canvasGo.AddComponent<HudLocationLabel>().Init(locLabel);

            var hud = canvasGo.AddComponent<HudPanelController>();
            hud.Bind(topBar, inboxPanel, travelPanel, coPanel, resolvePanel, dialoguePanel, menuPanel, storyPanel, overlay);

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
