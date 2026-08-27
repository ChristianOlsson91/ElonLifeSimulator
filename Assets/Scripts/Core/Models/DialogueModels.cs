using System;
using System.Collections.Generic;

namespace ElonLifeSim.Core.Models
{
    /// <summary>A single spoken/narrated line in a dialogue sequence.</summary>
    [Serializable]
    public sealed class DialogueLine
    {
        public string Speaker { get; }
        public string Text { get; }

        public DialogueLine(string speaker, string text)
        {
            Speaker = speaker ?? string.Empty;
            Text = text ?? string.Empty;
        }
    }

    /// <summary>
    /// A player choice that may branch to another node id (null = end dialogue).
    /// Act 1 choices carry a tag and Focus / ThickSkin / ExitPlan (and optional money) deltas.
    /// </summary>
    [Serializable]
    public sealed class DialogueChoice
    {
        public string Text { get; }
        public string NextNodeId { get; }
        public string Tag { get; }
        public int FocusDelta { get; }
        public int ThickSkinDelta { get; }
        public int ExitPlanDelta { get; }
        public int MoneyDelta { get; }

        public DialogueChoice(
            string text,
            string nextNodeId = null,
            string tag = null,
            int focusDelta = 0,
            int thickSkinDelta = 0,
            int exitPlanDelta = 0,
            int moneyDelta = 0)
        {
            Text = text ?? string.Empty;
            NextNodeId = nextNodeId;
            Tag = tag ?? string.Empty;
            FocusDelta = focusDelta;
            ThickSkinDelta = thickSkinDelta;
            ExitPlanDelta = exitPlanDelta;
            MoneyDelta = moneyDelta;
        }

        public bool EffectsDifferFrom(DialogueChoice other)
        {
            if (other == null)
                return true;
            return Tag != other.Tag
                   || FocusDelta != other.FocusDelta
                   || ThickSkinDelta != other.ThickSkinDelta
                   || ExitPlanDelta != other.ExitPlanDelta
                   || MoneyDelta != other.MoneyDelta;
        }
    }

    /// <summary>
    /// One node in a dialogue graph: lines shown in order, then optional choices.
    /// If Choices is empty, advancing past the last line ends the dialogue (continue path).
    /// </summary>
    [Serializable]
    public sealed class DialogueNode
    {
        public string Id { get; }
        public IReadOnlyList<DialogueLine> Lines { get; }
        public IReadOnlyList<DialogueChoice> Choices { get; }

        public DialogueNode(string id, IList<DialogueLine> lines, IList<DialogueChoice> choices = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Node id is required.", nameof(id));
            Id = id;
            Lines = new List<DialogueLine>(lines ?? Array.Empty<DialogueLine>());
            Choices = new List<DialogueChoice>(choices ?? Array.Empty<DialogueChoice>());
        }
    }

    /// <summary>A full dialogue definition (intro, problem briefing, etc.).</summary>
    [Serializable]
    public sealed class DialogueDefinition
    {
        public string Id { get; }
        public string StartNodeId { get; }
        public IReadOnlyDictionary<string, DialogueNode> Nodes { get; }

        public DialogueDefinition(string id, string startNodeId, IEnumerable<DialogueNode> nodes)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Dialogue id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(startNodeId))
                throw new ArgumentException("Start node id is required.", nameof(startNodeId));

            Id = id;
            StartNodeId = startNodeId;
            var map = new Dictionary<string, DialogueNode>(StringComparer.Ordinal);
            foreach (var node in nodes ?? Array.Empty<DialogueNode>())
            {
                if (node == null) continue;
                map[node.Id] = node;
            }
            if (!map.ContainsKey(startNodeId))
                throw new ArgumentException($"Start node '{startNodeId}' not found in dialogue '{id}'.");
            Nodes = map;
        }
    }
}
