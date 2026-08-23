using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Drives Act 1 beat dialogues from the centered Story sheet:
    /// Continue → PlayCurrentBeat → DialogueUI (bottom banner).
    /// </summary>
    public sealed class Act1StoryUI : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text topBarStatus;

        private GameSession _session;
        private bool _playing;

        public void Bind(GameSession session, Button continueBtn, Text sheetStatus,
            Text topStatus = null, Button close = null)
        {
            _session = session;
            continueButton = continueBtn;
            statusLabel = sheetStatus;
            topBarStatus = topStatus;
            closeButton = close;
            Wire();
            RefreshStatus();
        }

        private void Start()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            Wire();
            RefreshStatus();
        }

        private void Wire()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(PlayCurrentBeat);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(ReturnToMenu);
            }
        }

        private static void ReturnToMenu()
        {
            var hud = HudPanelController.Find();
            if (hud != null)
                hud.Open(HudLargePanel.Menu);
        }

        public void PlayCurrentBeat()
        {
            if (_playing) return;
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;

            if (_session.Act1.IsComplete)
            {
                RefreshStatus();
                return;
            }

            var dialogue = _session.GetAct1Dialogue();
            if (dialogue == null)
            {
                _session.AdvanceAct1Beat();
                RefreshStatus();
                return;
            }

            var dialogueUi = FindFirstObjectByType<DialogueUI>();
            if (dialogueUi == null)
            {
                _session.AdvanceAct1Beat();
                RefreshStatus();
                return;
            }

            HudPanelController.Find()?.PrepareForDialogueOrStory();
            _playing = true;
            dialogueUi.Play(dialogue, onComplete: () =>
            {
                _playing = false;
                _session.AdvanceAct1Beat();
                RefreshStatus();

                if (_session.Act1.IsComplete)
                    FindFirstObjectByType<InboxUI>()?.Show();
            });
        }

        public void RefreshStatus()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null) return;

            string sheet;
            string bar;
            if (_session.Act1.IsComplete)
            {
                sheet = "Act 1 complete  ·  Canada unlocked  ·  Found Zip2 when ready";
                bar = "Act 1 complete";
            }
            else
            {
                var beat = "Act 1  ·  " + _session.Act1.GetBeatLocationLabel();
                sheet = beat;
                bar = beat;
            }

            if (statusLabel != null)
                statusLabel.text = sheet;
            if (topBarStatus != null)
                topBarStatus.text = bar;

            if (continueButton != null)
                continueButton.gameObject.SetActive(!_session.Act1.IsComplete);
        }

        /// <summary>Auto-start first beat once when entering Pretoria.</summary>
        public void TryAutoStart()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (_session == null || _session.Act1.IsComplete) return;
            if (_session.Act1.CurrentBeat == Act1Progression.Beat.HomeIntro)
                PlayCurrentBeat();
        }
    }
}
