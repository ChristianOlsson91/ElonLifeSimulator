using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Builds the in-game HUD using <see cref="UiTheme"/> so it matches the main menu.
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

            var canvasGo = UiTheme.CreateCanvas("HUD_Canvas_PLACEHOLDER", 50);
            EnsureEventSystem();

            float barH = UiStyleTokens.TopBarHeight;
            var topBar = UiTheme.CreatePanel(canvasGo.transform, "TopBar",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -barH), new Vector2(0, 0), UiTheme.TopBarFill);

            var hair = new GameObject("TopBarEdge", typeof(RectTransform), typeof(Image));
            hair.transform.SetParent(topBar.transform, false);
            var hairRt = hair.GetComponent<RectTransform>();
            hairRt.anchorMin = new Vector2(0, 0);
            hairRt.anchorMax = new Vector2(1, 0);
            hairRt.pivot = new Vector2(0.5f, 0);
            hairRt.sizeDelta = new Vector2(0, 2);
            hairRt.anchoredPosition = Vector2.zero;
            hair.GetComponent<Image>().color = UiTheme.Primary;
            hair.GetComponent<Image>().raycastTarget = false;

            float bw = UiStyleTokens.TopBarButtonWidth;
            float bh = UiStyleTokens.TopBarButtonHeight;
            float gap = UiStyleTokens.ButtonGap;
            float x = 16f;
            float y = -12f;
            var inboxToggle = UiTheme.CreateButton(topBar.transform, "InboxToggle", "Inbox",
                new Vector2(x, y), new Vector2(bw, bh), false, new Vector2(0, 1), new Vector2(0, 1));
            x += bw + gap;
            var mapToggle = UiTheme.CreateButton(topBar.transform, "MapToggle", "Map",
                new Vector2(x, y), new Vector2(bw, bh), false, new Vector2(0, 1), new Vector2(0, 1));
            x += bw + gap;
            var coToggle = UiTheme.CreateButton(topBar.transform, "CompaniesToggle", "Companies",
                new Vector2(x, y), new Vector2(bw + 10, bh), false, new Vector2(0, 1), new Vector2(0, 1));
            x += bw + 10 + gap;
            var storyBtn = UiTheme.CreateButton(topBar.transform, "StoryButton", "Story",
                new Vector2(x, y), new Vector2(bw, bh), false, new Vector2(0, 1), new Vector2(0, 1));
            x += bw + 20;

            var locLabel = UiTheme.CreateText(topBar.transform, "LocationLabel", "Location",
                UiStyleTokens.TopBarLabelFontSize, new Vector2(x, -10), new Vector2(360, 22), TextAnchor.MiddleLeft);
            locLabel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            locLabel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            locLabel.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            var storyStatus = UiTheme.CreateText(topBar.transform, "StoryStatus", "Act 1",
                UiStyleTokens.CaptionFontSize, new Vector2(x, -32), new Vector2(520, 18), TextAnchor.MiddleLeft);
            storyStatus.color = UiTheme.Muted;
            storyStatus.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            storyStatus.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            storyStatus.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

            float topClear = -barH;
            float pad = 16f;

            var inboxPanel = UiTheme.CreatePanel(canvasGo.transform, "InboxPanel",
                new Vector2(0, 0), new Vector2(0.40f, 1f),
                new Vector2(pad, pad), new Vector2(-8, topClear), UiTheme.PanelFill);
            UiTheme.AddSheetHeader(inboxPanel, "Inbox", "InboxClose", out _);
            var inboxList = UiTheme.CreateText(inboxPanel.transform, "InboxList", "Inbox",
                UiStyleTokens.BodyFontSize, new Vector2(16, -48), new Vector2(-32, 160), TextAnchor.UpperLeft);
            UiTheme.Stretch(inboxList.rectTransform, new Vector2(0, 0.48f), new Vector2(1, 1),
                new Vector2(16, 8), new Vector2(-16, -48));
            var inboxDetail = UiTheme.CreateText(inboxPanel.transform, "InboxDetail", "",
                UiStyleTokens.CaptionFontSize, new Vector2(16, -8), new Vector2(-32, 120), TextAnchor.UpperLeft);
            inboxDetail.color = UiTheme.Muted;
            UiTheme.Stretch(inboxDetail.rectTransform, new Vector2(0, 0.16f), new Vector2(1, 0.48f),
                new Vector2(16, 8), new Vector2(-16, -8));
            var acceptBtn = UiTheme.CreateButton(inboxPanel.transform, "AcceptButton", "Accept",
                new Vector2(16, 12), new Vector2(96, 32), true);
            var resolveBtn = UiTheme.CreateButton(inboxPanel.transform, "ResolveButton", "Resolve",
                new Vector2(120, 12), new Vector2(96, 32), true);
            var prevTicket = UiTheme.CreateButton(inboxPanel.transform, "PrevTicket", "Prev",
                new Vector2(224, 12), new Vector2(64, 32), false);
            var nextTicket = UiTheme.CreateButton(inboxPanel.transform, "NextTicket", "Next",
                new Vector2(296, 12), new Vector2(64, 32), false);
            var inboxClose = inboxPanel.transform.Find("InboxClose").GetComponent<Button>();

            var travelPanel = UiTheme.CreatePanel(canvasGo.transform, "TravelPanel",
                new Vector2(0.42f, 0), new Vector2(1, 1),
                new Vector2(8, pad), new Vector2(-pad, topClear), UiTheme.PanelFill);
            UiTheme.AddSheetHeader(travelPanel, "Map", "TravelClose", out _);
            var travelList = UiTheme.CreateText(travelPanel.transform, "TravelList", "Map",
                UiStyleTokens.BodyFontSize, new Vector2(16, -48), new Vector2(-32, 200), TextAnchor.UpperLeft);
            UiTheme.Stretch(travelList.rectTransform, new Vector2(0, 0.16f), new Vector2(1, 1),
                new Vector2(16, 8), new Vector2(-16, -48));
            var travelBtn = UiTheme.CreateButton(travelPanel.transform, "TravelButton", "Travel",
                new Vector2(16, 12), new Vector2(120, 32), true);
            var nextLoc = UiTheme.CreateButton(travelPanel.transform, "NextLocation", "Next Loc",
                new Vector2(144, 12), new Vector2(100, 32), false);
            var travelClose = travelPanel.transform.Find("TravelClose").GetComponent<Button>();

            var coPanel = UiTheme.CreatePanel(canvasGo.transform, "CompaniesPanel",
                new Vector2(0.18f, 0.12f), new Vector2(0.82f, 1f),
                new Vector2(0, 16), new Vector2(0, topClear), UiTheme.PanelFill);
            UiTheme.AddSheetHeader(coPanel, "Companies", "CompaniesClose", out _);
            var coBody = UiTheme.CreateText(coPanel.transform, "CompaniesBody", "Companies",
                UiStyleTokens.BodyFontSize, new Vector2(16, -48), new Vector2(-32, 280), TextAnchor.UpperLeft);
            UiTheme.Stretch(coBody.rectTransform, new Vector2(0, 0.16f), new Vector2(1, 1),
                new Vector2(16, 8), new Vector2(-16, -48));
            var foundZip2 = UiTheme.CreateButton(coPanel.transform, "FoundZip2", "Found Zip2",
                new Vector2(16, 12), new Vector2(140, 32), true);
            var foundX = UiTheme.CreateButton(coPanel.transform, "FoundXCom", "Found X.com",
                new Vector2(164, 12), new Vector2(140, 32), true);
            var coClose = coPanel.transform.Find("CompaniesClose").GetComponent<Button>();

            var resolvePanel = UiTheme.CreatePanel(canvasGo.transform, "ProblemResolvePanel",
                new Vector2(0.18f, 0.16f), new Vector2(0.82f, 0.88f),
                new Vector2(0, 0), new Vector2(0, 0), UiTheme.PanelFill);
            UiTheme.AddSheetHeader(resolvePanel, "Problem", "ResolveClose", out var resHeader);
            var resBody = UiTheme.CreateText(resolvePanel.transform, "ResolveBody", "",
                UiStyleTokens.BodyFontSize, new Vector2(16, -48), new Vector2(-32, 120), TextAnchor.UpperLeft);
            UiTheme.Stretch(resBody.rectTransform, new Vector2(0, 0.42f), new Vector2(1, 1),
                new Vector2(16, 8), new Vector2(-16, -48));
            var resChoices = new GameObject("ResolveChoices", typeof(RectTransform), typeof(VerticalLayoutGroup));
            resChoices.transform.SetParent(resolvePanel.transform, false);
            UiTheme.Stretch(resChoices.GetComponent<RectTransform>(), new Vector2(0, 0.04f), new Vector2(1, 0.42f),
                new Vector2(16, 12), new Vector2(-16, -8));
            var rVlg = resChoices.GetComponent<VerticalLayoutGroup>();
            rVlg.spacing = UiStyleTokens.ButtonGap;
            rVlg.childControlWidth = true;
            rVlg.childControlHeight = false;
            rVlg.childForceExpandWidth = true;
            var resClose = resolvePanel.transform.Find("ResolveClose").GetComponent<Button>();

            var dialoguePanel = UiTheme.CreatePanel(canvasGo.transform, "DialoguePanel",
                new Vector2(0.10f, 0), new Vector2(0.90f, 0.28f),
                new Vector2(0, 12), new Vector2(0, 0), UiTheme.PanelFill);

            var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGo.transform.SetParent(dialoguePanel.transform, false);
            var portraitRt = portraitGo.GetComponent<RectTransform>();
            portraitRt.anchorMin = new Vector2(0, 0);
            portraitRt.anchorMax = new Vector2(0, 1);
            portraitRt.pivot = new Vector2(0, 0.5f);
            portraitRt.anchoredPosition = new Vector2(12, 0);
            portraitRt.sizeDelta = new Vector2(88, -20);
            var portraitImg = portraitGo.GetComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.preserveAspect = true;
            portraitImg.raycastTarget = false;

            var speaker = UiTheme.CreateText(dialoguePanel.transform, "Speaker", "Speaker",
                UiStyleTokens.PanelTitleFontSize, new Vector2(112, -10), new Vector2(400, 24), TextAnchor.UpperLeft);
            var body = UiTheme.CreateText(dialoguePanel.transform, "Body", "…",
                UiStyleTokens.BodyFontSize, new Vector2(112, -36), new Vector2(-32, 80), TextAnchor.UpperLeft);
            body.color = UiTheme.Muted;
            UiTheme.Stretch(body.rectTransform, new Vector2(0, 0.38f), new Vector2(1, 0.82f),
                new Vector2(112, 6), new Vector2(-16, -8));
            var contBtn = UiTheme.CreateButton(dialoguePanel.transform, "ContinueButton", "Continue",
                new Vector2(-16, 10), new Vector2(120, 32), true, new Vector2(1, 0), new Vector2(1, 0));

            var choicesRoot = new GameObject("ChoicesRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            choicesRoot.transform.SetParent(dialoguePanel.transform, false);
            UiTheme.Stretch(choicesRoot.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0.38f),
                new Vector2(112, 44), new Vector2(-16, 6));
            var vlg = choicesRoot.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

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
            Debug.Log("[ElonLifeSim] HUD built.");
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
