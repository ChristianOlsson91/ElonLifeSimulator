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
            {
                Debug.Log($"[ProblemResolve] Not a company problem: {ticketId}");
                return;
            }

            _problemId = ticketId;
            if (headerText != null)
                headerText.text = problem.Title;
            if (bodyText != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(problem.Description);
                sb.AppendLine();
                sb.AppendLine($"Company: {problem.CompanyDisplayName} · Loc: {problem.LocationDisplayName}");
                sb.AppendLine("Choose carefully — stats will change.");
                bodyText.text = sb.ToString();
            }

            BuildChoices(problem);
            if (panelRoot != null)
                panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null)
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
                rt.sizeDelta = new Vector2(520, 40);
                go.GetComponent<Image>().color = new Color(0.18f, 0.28f, 0.22f, 0.95f);

                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(go.transform, false);
                var label = labelGo.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (label.font == null)
                    label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                label.text = choice.Text;
                label.color = Color.white;
                label.fontSize = 13;
                label.alignment = TextAnchor.MiddleLeft;
                var lrt = labelGo.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(8, 2);
                lrt.offsetMax = new Vector2(-8, -2);

                go.GetComponent<Button>().onClick.AddListener(() => OnChoose(index));
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
            {
                Debug.LogWarning("[ProblemResolve] Resolve failed.");
                return;
            }

            Debug.Log($"[ProblemResolve] {_session.LastResolutionNarration}");
            if (bodyText != null)
                bodyText.text = _session.LastResolutionNarration;

            FindFirstObjectByType<CompanyDashboardUI>()?.Refresh();
            // InboxUI.Refresh re-runs InboxSelection.EnsureSelected so completed tickets do not stick.
            FindFirstObjectByType<InboxUI>()?.Refresh();
            ClearChoices();
        }
    }
}
