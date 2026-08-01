using ElonLifeSim.Core.Models;
using UnityEngine;

namespace ElonLifeSim.Data
{
    /// <summary>
    /// ScriptableObject wrapper for an Inbox ticket — community-friendly content asset.
    /// Create via Assets → Create → ElonLifeSim → Ticket Definition.
    /// Maps to pure <see cref="InboxTicket"/> at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "Ticket_", menuName = "ElonLifeSim/Ticket Definition", order = 10)]
    public sealed class TicketDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string ticketId;
        public string title;

        [Header("Company & Location")]
        public string companyId;
        public string companyDisplayName;
        public string locationId;
        public string locationDisplayName;

        [Header("Problem")]
        [TextArea(3, 8)]
        public string description;
        [Range(1, 5)]
        public int difficulty = 1;
        public string rewardDescription;

        public InboxTicket ToTicket()
        {
            return new InboxTicket(
                ticketId,
                companyId,
                companyDisplayName,
                locationId,
                locationDisplayName,
                title,
                description,
                difficulty,
                rewardDescription);
        }
    }
}
