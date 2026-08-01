using System;
using System.Collections.Generic;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Pure inbox list selection rules used by InboxUI (and tests).
    /// Keeps the selected ticket on an actionable item when the previous one is completed.
    /// </summary>
    public static class InboxSelection
    {
        /// <summary>
        /// True if the player can still act on this ticket (accept or resolve while open).
        /// Completed/Failed tickets are not primary selection targets.
        /// </summary>
        public static bool IsActionable(InboxTicket t)
        {
            if (t == null) return false;
            return t.Status == TicketStatus.Pending || t.Status == TicketStatus.Accepted ||
                   t.Status == TicketStatus.InProgress;
        }

        /// <summary>
        /// Ensures a sensible selection: keep current if still actionable; otherwise first Pending,
        /// then first other actionable; otherwise first ticket or null.
        /// </summary>
        public static string EnsureSelected(IReadOnlyList<InboxTicket> tickets, string currentId)
        {
            if (tickets == null || tickets.Count == 0)
                return null;

            if (!string.IsNullOrEmpty(currentId))
            {
                for (int i = 0; i < tickets.Count; i++)
                {
                    if (tickets[i].Id == currentId && IsActionable(tickets[i]))
                        return currentId;
                }
            }

            for (int i = 0; i < tickets.Count; i++)
            {
                if (tickets[i].Status == TicketStatus.Pending)
                    return tickets[i].Id;
            }

            for (int i = 0; i < tickets.Count; i++)
            {
                if (IsActionable(tickets[i]))
                    return tickets[i].Id;
            }

            return tickets[0].Id;
        }

        /// <summary>Move selection to the next ticket in list order (wraps).</summary>
        public static string SelectNext(IReadOnlyList<InboxTicket> tickets, string currentId)
        {
            if (tickets == null || tickets.Count == 0)
                return null;

            int idx = 0;
            if (!string.IsNullOrEmpty(currentId))
            {
                for (int i = 0; i < tickets.Count; i++)
                {
                    if (tickets[i].Id == currentId)
                    {
                        idx = i;
                        break;
                    }
                }
            }

            int next = (idx + 1) % tickets.Count;
            return tickets[next].Id;
        }

        /// <summary>Move selection to the previous ticket in list order (wraps).</summary>
        public static string SelectPrevious(IReadOnlyList<InboxTicket> tickets, string currentId)
        {
            if (tickets == null || tickets.Count == 0)
                return null;

            int idx = 0;
            if (!string.IsNullOrEmpty(currentId))
            {
                for (int i = 0; i < tickets.Count; i++)
                {
                    if (tickets[i].Id == currentId)
                    {
                        idx = i;
                        break;
                    }
                }
            }

            int prev = (idx - 1 + tickets.Count) % tickets.Count;
            return tickets[prev].Id;
        }

        /// <summary>Select by id if present; otherwise EnsureSelected.</summary>
        public static string SelectById(IReadOnlyList<InboxTicket> tickets, string id)
        {
            if (tickets == null || tickets.Count == 0)
                return null;
            if (string.IsNullOrEmpty(id))
                return EnsureSelected(tickets, null);

            for (int i = 0; i < tickets.Count; i++)
            {
                if (tickets[i].Id == id)
                    return id;
            }
            return EnsureSelected(tickets, null);
        }
    }
}
