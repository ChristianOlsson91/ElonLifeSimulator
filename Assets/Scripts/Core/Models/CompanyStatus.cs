namespace ElonLifeSim.Core.Models
{
    /// <summary>
    /// Lifecycle of a company in the life-sim.
    /// Extension: add new statuses carefully; CompanyManager and problem choices switch on these.
    /// </summary>
    public enum CompanyStatus
    {
        NotFounded = 0,
        Active = 1,
        Sold = 2,
        Merged = 3,
        Inactive = 4
    }
}
