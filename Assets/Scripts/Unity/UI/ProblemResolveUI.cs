using System.Text;
using ElonLifeSim.Core.Models;
using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Shows 2–3 problem choices for the active company ticket and applies ResolveProblem.
    /// </summary>
    public sealed class ProblemResolveUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text headerText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Transform choicesRoot;
        [SerializeField] private Button closeButton;

        private GameSession _session;
        private string _problemId;

        public void Bind(GameObject panel, Text header, Text body, Transform choices, Button close)
        {
            panelRoot = panel;
            headerText = header;
            bodyText = body;
            choicesRoot = choices;
            closeButton = close;
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void Start()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
        }

        public void ShowForTicket(string ticketId)
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;
            if (!_session.TryGetProblem(ticketId, out var problem))
                return;

            _problemId = ticketId;
            if (headerText != null)
                headerText.text = problem.Title;
            if (bodyText != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(problem.Description);
                sb.AppendLine();
                sb.AppendLine($"{problem.CompanyDisplayName}  ·  {problem.LocationDisplayName}");
                sb.AppendLine("Choose a path. Money, progress, and opinion will move.");
                bodyText.text = sb.ToString();
            }

            BuildChoices(problem);
            var hud = HudPanelController.Find();
            if (hud != null)
                hud.Open(HudLargePanel.Resolve);
            else if (panelRoot != null)
                panelRoot.SetActive(true);
        }

        public void Hide()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
                hud.Open(HudLargePanel.Menu);
            else if (panelRoot != null)
                panelRoot.SetActive(false);
            ClearChoices();
        }

        private void BuildChoices(ProblemDefinition problem)
        {
            ClearChoices();
            if (choicesRoot == null) return;

            for (int i = 0; i < problem.Choices.Count; i++)
            {
                int index = i;
                var choice = problem.Choices[i];
                var go = new GameObject($"ProblemChoice_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(choicesRoot, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(520, 44);
                var img = go.GetComponent<Image>();
                var btn = go.GetComponent<Button>();
                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(go.transform, false);
                var label = labelGo.GetComponent<Text>();
                label.text = choice.Text;
                label.alignment = TextAnchor.MiddleLeft;
                var lrt = labelGo.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(12, 4);
                lrt.offsetMax = new Vector2(-12, -4);
                UiTheme.StyleChoiceButton(img, btn, label);

                btn.onClick.AddListener(() => OnChoose(index));
            }
        }

        private void ClearChoices()
        {
            if (choicesRoot == null) return;
            for (int i = choicesRoot.childCount - 1; i >= 0; i--)
                Destroy(choicesRoot.GetChild(i).gameObject);
        }

        private void OnChoose(int index)
        {
            if (_session == null || string.IsNullOrEmpty(_problemId))
                return;

            if (!_session.ResolveProblem(_problemId, index))
                return;

            if (bodyText != null)
                bodyText.text = _session.LastResolutionNarration;

            FindFirstObjectByType<CompanyDashboardUI>()?.Refresh();
            FindFirstObjectByType<InboxUI>()?.Refresh();
            ClearChoices();
        }
    }
}
