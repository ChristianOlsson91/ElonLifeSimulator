using System;
using System.Collections.Generic;

namespace ElonLifeSim.Core.Models
{
    /// <summary>
    /// One meaningful choice when resolving an inbox company problem.
    /// Applying it mutates company stats (and optionally status) via CompanyManager.
    /// </summary>
    [Serializable]
    public sealed class ProblemChoice
    {
        public string Id { get; }
        public string Text { get; }
        public string OutcomeNarration { get; }
        public int MoneyDelta { get; }
        public int ProgressDelta { get; }
        public int PublicOpinionDelta { get; }
        public int EngineeringDelta { get; }
        /// <summary>If set, company status becomes this after the choice (e.g. Sold, Merged).</summary>
        public CompanyStatus? ResultStatus { get; }

        public ProblemChoice(
            string id,
            string text,
            string outcomeNarration,
            int moneyDelta = 0,
            int progressDelta = 0,
            int publicOpinionDelta = 0,
            int engineeringDelta = 0,
            CompanyStatus? resultStatus = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Choice id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Choice text is required.", nameof(text));

            Id = id;
            Text = text;
            OutcomeNarration = outcomeNarration ?? string.Empty;
            MoneyDelta = moneyDelta;
            ProgressDelta = progressDelta;
            PublicOpinionDelta = publicOpinionDelta;
            EngineeringDelta = engineeringDelta;
            ResultStatus = resultStatus;
        }
    }

    /// <summary>
    /// Full company problem definition: becomes an Inbox ticket + 2–3 resolvable choices.
    ///
    /// EXTENSION: author more via content factories or ScriptableObjects; register in a catalog.
    /// </summary>
    [Serializable]
    public sealed class ProblemDefinition
    {
        public string Id { get; }
        public string CompanyId { get; }
        public string CompanyDisplayName { get; }
        public string LocationId { get; }
        public string LocationDisplayName { get; }
        public string Title { get; }
        public string Description { get; }
        public int Difficulty { get; }
        public string RewardDescription { get; }
        public IReadOnlyList<ProblemChoice> Choices { get; }

        public ProblemDefinition(
            string id,
            string companyId,
            string companyDisplayName,
            string locationId,
            string locationDisplayName,
            string title,
            string description,
            int difficulty,
            string rewardDescription,
            IList<ProblemChoice> choices)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Problem id is required.", nameof(id));
            if (choices == null || choices.Count < 2)
                throw new ArgumentException("Problems need at least 2 choices.", nameof(choices));
            if (choices.Count > 3)
                throw new ArgumentException("Keep choices to 2–3 for this prototype.", nameof(choices));

            Id = id;
            CompanyId = companyId;
            CompanyDisplayName = companyDisplayName ?? companyId;
            LocationId = locationId;
            LocationDisplayName = locationDisplayName ?? locationId;
            Title = title;
            Description = description;
            Difficulty = difficulty;
            RewardDescription = rewardDescription ?? string.Empty;
            Choices = new List<ProblemChoice>(choices);
        }

        /// <summary>Builds the inbox ticket surface for this problem.</summary>
        public InboxTicket ToTicket()
        {
            return new InboxTicket(
                Id,
                CompanyId,
                CompanyDisplayName,
                LocationId,
                LocationDisplayName,
                Title,
                Description,
                Difficulty,
                RewardDescription);
        }
    }
}
