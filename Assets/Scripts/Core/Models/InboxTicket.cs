using System;

namespace ElonLifeSim.Core.Models
{
    /// <summary>
    /// A single problem/opportunity delivered to the player's Inbox.
    ///
    /// Community extension: create tickets via <see cref="Content.PrototypeContent"/>
    /// or ScriptableObject definitions (TicketDefinition) — do not hard-code tickets
    /// inside UI MonoBehaviours.
    ///
    /// Required fields (GDD): Company, Location, Problem description, Difficulty, Reward.
    /// </summary>
    [Serializable]
    public sealed class InboxTicket
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
        public TicketStatus Status { get; private set; }

        public InboxTicket(
            string id,
            string companyId,
            string companyDisplayName,
            string locationId,
            string locationDisplayName,
            string title,
            string description,
            int difficulty,
            string rewardDescription,
            TicketStatus status = TicketStatus.Pending)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Ticket id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company id is required.", nameof(companyId));
            if (string.IsNullOrWhiteSpace(locationId))
                throw new ArgumentException("Location id is required.", nameof(locationId));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.", nameof(description));
            if (difficulty < 1 || difficulty > 5)
                throw new ArgumentOutOfRangeException(nameof(difficulty), "Difficulty must be 1–5.");

            Id = id;
            CompanyId = companyId;
            CompanyDisplayName = companyDisplayName ?? companyId;
            LocationId = locationId;
            LocationDisplayName = locationDisplayName ?? locationId;
            Title = title;
            Description = description;
            Difficulty = difficulty;
            RewardDescription = rewardDescription ?? string.Empty;
            Status = status;
        }

        public void SetStatus(TicketStatus status)
        {
            Status = status;
        }

        /// <summary>Creates a deep copy so UI/tests can snapshot without sharing mutable status.</summary>
        public InboxTicket Clone()
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
                RewardDescription,
                Status);
        }
    }
}
