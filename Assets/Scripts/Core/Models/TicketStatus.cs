namespace ElonLifeSim.Core.Models
{
    /// <summary>
    /// Lifecycle of an inbox problem ticket.
    /// Extension: new statuses can be added carefully; existing services switch on these values.
    /// </summary>
    public enum TicketStatus
    {
        Pending = 0,
        Accepted = 1,
        InProgress = 2,
        Completed = 3,
        Failed = 4
    }
}
