using System;
using ElonLifeSim.Core.Models;
using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Presents DialogueRunner state: speaker, line text, continue, and choice buttons.
    /// </summary>
    public sealed class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Transform choicesRoot;
        [SerializeField] private Button choiceButtonPrefab;

        private DialogueRunner _runner;
        private Action _onComplete;
        private Button[] _runtimeChoiceButtons;

        public void Bind(GameObject panel, Text speaker, Text body, Button cont, Transform choicesParent)
        {
            panelRoot = panel;
            speakerText = speaker;
            bodyText = body;
            continueButton = cont;
            choicesRoot = choicesParent;
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinue);
            }
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void RefreshPortrait()
        {
        }

        public void Play(DialogueDefinition definition, Action onComplete = null)
        {
            var session = GameBootstrap.RequireSession();
            _runner = session != null ? session.Dialogue : new DialogueRunner();
            _onComplete = onComplete;

            _runner.StateChanged -= Refresh;
            _runner.Completed -= HandleCompleted;
            _runner.StateChanged += Refresh;
            _runner.Completed += HandleCompleted;

            if (!_runner.Start(definition))
            {
                Debug.LogWarning("[Dialogue] Failed to start dialogue.");
                _onComplete?.Invoke();
                return;
            }

            if (panelRoot != null)
                panelRoot.SetActive(true);
            Refresh();
        }

        private void OnDestroy()
        {
            if (_runner != null)
            {
                _runner.StateChanged -= Refresh;
                _runner.Completed -= HandleCompleted;
            }
        }

        private void OnContinue()
        {
            _runner?.Advance();
        }

        private void HandleCompleted()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
            var cb = _onComplete;
            _onComplete = null;
            cb?.Invoke();
        }

        private void Refresh()
        {
            if (_runner == null) return;

            if (_runner.IsComplete)
            {
                if (panelRoot != null)
                    panelRoot.SetActive(false);
                return;
            }

            if (_runner.IsAwaitingChoice)
            {
                if (speakerText != null) speakerText.text = "";
                if (bodyText != null) bodyText.text = "What do you do?";
                if (continueButton != null) continueButton.gameObject.SetActive(false);
                BuildChoiceButtons(_runner.AvailableChoices);
            }
            else
            {
                ClearChoiceButtons();
                var line = _runner.CurrentLine;
                if (speakerText != null)
                    speakerText.text = line != null ? line.Speaker : "";
                if (bodyText != null)
                    bodyText.text = line != null ? line.Text : "";
                if (continueButton != null)
                    continueButton.gameObject.SetActive(true);
            }
        }

        private void BuildChoiceButtons(System.Collections.Generic.IReadOnlyList<DialogueChoice> choices)
        {
            ClearChoiceButtons();
            if (choicesRoot == null || choices == null) return;

            _runtimeChoiceButtons = new Button[choices.Count];
            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                var go = new GameObject($"Choice_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(choicesRoot, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(480, 36);

                var img = go.GetComponent<Image>();
                img.color = new Color(0.15f, 0.2f, 0.35f, 0.95f);

                var btn = go.GetComponent<Button>();
                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(go.transform, false);
                var label = labelGo.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (label.font == null)
                    label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                label.text = choices[i].Text;
                label.color = Color.white;
                label.alignment = TextAnchor.MiddleCenter;
                label.fontSize = 14;
                var lrt = labelGo.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;

                btn.onClick.AddListener(() => _runner.Choose(index));
                _runtimeChoiceButtons[i] = btn;
            }
        }

        private void ClearChoiceButtons()
        {
            if (choicesRoot == null) return;
            for (int i = choicesRoot.childCount - 1; i >= 0; i--)
                Destroy(choicesRoot.GetChild(i).gameObject);
            _runtimeChoiceButtons = null;
        }
    }
}
