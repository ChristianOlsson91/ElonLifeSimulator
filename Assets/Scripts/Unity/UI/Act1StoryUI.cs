using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>
    /// Drives Act 1 beat dialogues: play current beat → on complete → AdvanceAct1Beat → next or unlock notice.
    /// PLACEHOLDER UI hooks via GameplayHudBuilder "Story" button.
    /// </summary>
    public sealed class Act1StoryUI : MonoBehaviour
    {
        [SerializeField] private Button storyButton;
        [SerializeField] private Text statusLabel;

        private GameSession _session;
        private bool _playing;

        public void Bind(GameSession session, Button story, Text status)
        {
            _session = session;
            storyButton = story;
            statusLabel = status;
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
            if (storyButton != null)
            {
                storyButton.onClick.RemoveAllListeners();
                storyButton.onClick.AddListener(PlayCurrentBeat);
            }
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
                // Headless fallback: still advance progression for tests/debug.
                _session.AdvanceAct1Beat();
                RefreshStatus();
                return;
            }

            _playing = true;
            dialogueUi.Play(dialogue, onComplete: () =>
            {
                _playing = false;
                _session.AdvanceAct1Beat();
                RefreshStatus();

                if (_session.Act1.IsComplete)
                {
                    Debug.Log("[ElonLifeSim] Act 1 complete — Canada unlocked. Open Inbox / Map / Companies.");
                    FindFirstObjectByType<InboxUI>()?.Show();
                }
                else
                {
                    // Optional: auto-chain next beat prompt via status label only.
                }
            });
        }

        public void RefreshStatus()
        {
            if (_session == null)
                _session = GameBootstrap.RequireSession();
            if (statusLabel == null || _session == null) return;

            if (_session.Act1.IsComplete)
                statusLabel.text = "Act 1 complete · Canada unlocked · Found Zip2 when ready";
            else
                statusLabel.text = $"Act 1: {_session.Act1.GetBeatLocationLabel()} (Story)";
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
