using System;
using System.Collections.Generic;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Pure dialogue state machine: advances through lines and choices.
    ///
    /// UI (DialogueUI) calls <see cref="Advance"/> / <see cref="Choose"/> and reads
    /// <see cref="CurrentLine"/> / <see cref="AvailableChoices"/>.
    ///
    /// Extension: author DialogueDefinition graphs in content; load via dialogue id.
    /// </summary>
    public sealed class DialogueRunner
    {
        private DialogueDefinition _definition;
        private DialogueNode _currentNode;
        private int _lineIndex;
        private bool _awaitingChoice;
        private bool _isComplete;

        public bool IsActive => _definition != null && !_isComplete;
        public bool IsComplete => _isComplete;
        public bool IsAwaitingChoice => _awaitingChoice;
        public string DialogueId => _definition?.Id;
        public string CurrentNodeId => _currentNode?.Id;
        public DialogueLine CurrentLine { get; private set; }
        public IReadOnlyList<DialogueChoice> AvailableChoices { get; private set; } =
            Array.Empty<DialogueChoice>();

        public event Action StateChanged;
        public event Action Completed;

        /// <summary>Starts a dialogue from its start node. Returns false if definition invalid.</summary>
        public bool Start(DialogueDefinition definition)
        {
            if (definition == null || definition.Nodes == null || definition.Nodes.Count == 0)
                return false;
            if (!definition.Nodes.TryGetValue(definition.StartNodeId, out var start))
                return false;

            _definition = definition;
            _currentNode = start;
            _lineIndex = 0;
            _awaitingChoice = false;
            _isComplete = false;
            AvailableChoices = Array.Empty<DialogueChoice>();
            PresentCurrentLineOrChoices();
            StateChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Advances past the current line. If at last line with choices, enters choice mode.
        /// If at last line with no choices, completes dialogue.
        /// Returns false if not active or awaiting choice.
        /// </summary>
        public bool Advance()
        {
            if (!IsActive || _awaitingChoice)
                return false;

            if (_currentNode.Lines == null || _currentNode.Lines.Count == 0)
            {
                return EnterChoicesOrComplete();
            }

            _lineIndex++;
            if (_lineIndex < _currentNode.Lines.Count)
            {
                CurrentLine = _currentNode.Lines[_lineIndex];
                StateChanged?.Invoke();
                return true;
            }

            return EnterChoicesOrComplete();
        }

        /// <summary>
        /// Selects a choice by index when <see cref="IsAwaitingChoice"/> is true.
        /// Null NextNodeId completes the dialogue.
        /// </summary>
        public bool Choose(int choiceIndex)
        {
            if (!IsActive || !_awaitingChoice)
                return false;
            if (choiceIndex < 0 || choiceIndex >= AvailableChoices.Count)
                return false;

            var choice = AvailableChoices[choiceIndex];
            if (string.IsNullOrEmpty(choice.NextNodeId))
            {
                Complete();
                return true;
            }

            if (!_definition.Nodes.TryGetValue(choice.NextNodeId, out var next))
            {
                Complete();
                return true;
            }

            _currentNode = next;
            _lineIndex = 0;
            _awaitingChoice = false;
            AvailableChoices = Array.Empty<DialogueChoice>();
            PresentCurrentLineOrChoices();
            StateChanged?.Invoke();
            return true;
        }

        public void Stop()
        {
            if (_definition == null) return;
            Complete();
        }

        private void PresentCurrentLineOrChoices()
        {
            if (_currentNode.Lines != null && _currentNode.Lines.Count > 0)
            {
                CurrentLine = _currentNode.Lines[0];
                _lineIndex = 0;
                _awaitingChoice = false;
            }
            else
            {
                CurrentLine = null;
                EnterChoicesOrComplete();
            }
        }

        private bool EnterChoicesOrComplete()
        {
            if (_currentNode.Choices != null && _currentNode.Choices.Count > 0)
            {
                CurrentLine = null;
                _awaitingChoice = true;
                AvailableChoices = _currentNode.Choices;
                StateChanged?.Invoke();
                return true;
            }

            Complete();
            return true;
        }

        private void Complete()
        {
            _isComplete = true;
            _awaitingChoice = false;
            CurrentLine = null;
            AvailableChoices = Array.Empty<DialogueChoice>();
            StateChanged?.Invoke();
            Completed?.Invoke();
        }
    }
}
