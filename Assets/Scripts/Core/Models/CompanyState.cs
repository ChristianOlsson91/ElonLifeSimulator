using System;

namespace ElonLifeSim.Core.Models
{
    /// <summary>
    /// Runtime stats for one company. Pure data — mutated only via <see cref="Services.CompanyManager"/>.
    ///
    /// Stats (GDD): Money/resources, Progress, Public Opinion/reputation, Engineering level, Status.
    /// </summary>
    [Serializable]
    public sealed class CompanyState
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Summary { get; }

        public int Money { get; private set; }
        public int Progress { get; private set; }
        public int PublicOpinion { get; private set; }
        public int EngineeringLevel { get; private set; }
        public CompanyStatus Status { get; private set; }

        public CompanyState(
            string id,
            string displayName,
            string summary = "",
            int money = 0,
            int progress = 0,
            int publicOpinion = 50,
            int engineeringLevel = 0,
            CompanyStatus status = CompanyStatus.NotFounded)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Company id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name is required.", nameof(displayName));

            Id = id;
            DisplayName = displayName;
            Summary = summary ?? string.Empty;
            Money = money;
            Progress = progress;
            PublicOpinion = publicOpinion;
            EngineeringLevel = engineeringLevel;
            Status = status;
        }

        internal void Apply(
            int moneyDelta,
            int progressDelta,
            int publicOpinionDelta,
            int engineeringDelta,
            CompanyStatus? newStatus)
        {
            Money += moneyDelta;
            Progress = ClampNonNeg(Progress + progressDelta);
            PublicOpinion = Clamp(PublicOpinion + publicOpinionDelta, 0, 100);
            EngineeringLevel = ClampNonNeg(EngineeringLevel + engineeringDelta);
            if (newStatus.HasValue)
                Status = newStatus.Value;
        }

        internal void SetStatus(CompanyStatus status) => Status = status;

        public CompanyState Clone()
        {
            return new CompanyState(
                Id, DisplayName, Summary, Money, Progress, PublicOpinion, EngineeringLevel, Status);
        }

        private static int ClampNonNeg(int v) => v < 0 ? 0 : v;

        private static int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public override string ToString()
        {
            return $"{DisplayName} [{Status}] $={Money} Prog={Progress} Eng={EngineeringLevel} Op={PublicOpinion}";
        }
    }
}
