using System;
using System.Collections.Generic;
using System.Linq;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Pure core Inbox: receive tickets over time, list them, accept one.
    ///
    /// No Unity dependencies — unit-testable. MonoBehaviour UI (InboxUI) observes this.
    ///
    /// Extension: call <see cref="ReceiveTicket"/> from a timed scheduler or narrative
    /// event system when delivering new problems; never invent tickets only in UI code.
    /// </summary>
    public sealed class InboxService
    {
        private readonly Dictionary<string, InboxTicket> _tickets =
            new Dictionary<string, InboxTicket>(StringComparer.Ordinal);

        /// <summary>Raised after a ticket is added or its status changes.</summary>
        public event Action InboxChanged;

        public int Count => _tickets.Count;

        /// <summary>
        /// Delivers a new ticket into the inbox. Idempotent on id: returns false if already present.
        /// </summary>
        public bool ReceiveTicket(InboxTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            if (_tickets.ContainsKey(ticket.Id))
                return false;

            // Store a clone so callers cannot mutate shared status behind our back.
            var stored = ticket.Clone();
            if (stored.Status != TicketStatus.Pending)
                stored.SetStatus(TicketStatus.Pending);

            _tickets[stored.Id] = stored;
            InboxChanged?.Invoke();
            return true;
        }

        /// <summary>All tickets currently in the inbox (newest first is content's job; order is insertion order).</summary>
        public IReadOnlyList<InboxTicket> ListTickets()
        {
            return _tickets.Values.Select(t => t.Clone()).ToList();
        }

        /// <summary>Pending tickets only (player still needs to act).</summary>
        public IReadOnlyList<InboxTicket> ListPending()
        {
            return _tickets.Values
                .Where(t => t.Status == TicketStatus.Pending)
                .Select(t => t.Clone())
                .ToList();
        }

        public bool TryGet(string ticketId, out InboxTicket ticket)
        {
            if (ticketId != null && _tickets.TryGetValue(ticketId, out var found))
            {
                ticket = found.Clone();
                return true;
            }
            ticket = null;
            return false;
        }

        /// <summary>
        /// Accepts a pending ticket so the player can travel and solve it.
        /// Returns false if missing or not pending.
        /// </summary>
        public bool AcceptTicket(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId) || !_tickets.TryGetValue(ticketId, out var ticket))
                return false;

            if (ticket.Status != TicketStatus.Pending)
                return false;

            ticket.SetStatus(TicketStatus.Accepted);
            InboxChanged?.Invoke();
            return true;
        }

        /// <summary>Marks an accepted/in-progress ticket complete (after challenge/dialogue).</summary>
        public bool CompleteTicket(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId) || !_tickets.TryGetValue(ticketId, out var ticket))
                return false;

            if (ticket.Status != TicketStatus.Accepted && ticket.Status != TicketStatus.InProgress)
                return false;

            ticket.SetStatus(TicketStatus.Completed);
            InboxChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            _tickets.Clear();
            InboxChanged?.Invoke();
        }
    }
}
