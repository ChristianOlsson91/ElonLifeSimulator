using System;
using System.Collections.Generic;
using System.Linq;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Services
{
    /// <summary>
    /// Tracks company roster and stats. Pure C# — unit-testable.
    ///
    /// Extension: register new companies from content; apply problem choice deltas via
    /// <see cref="ApplyChoice"/>; never mutate stats only in UI code.
    /// </summary>
    public sealed class CompanyManager
    {
        private readonly Dictionary<string, CompanyState> _companies =
            new Dictionary<string, CompanyState>(StringComparer.Ordinal);

        public event Action CompaniesChanged;

        public int Count => _companies.Count;

        public void Register(CompanyState company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));
            _companies[company.Id] = company.Clone();
            CompaniesChanged?.Invoke();
        }

        public void RegisterMany(IEnumerable<CompanyState> companies)
        {
            if (companies == null) return;
            foreach (var c in companies)
                Register(c);
        }

        public bool TryGet(string companyId, out CompanyState state)
        {
            if (companyId != null && _companies.TryGetValue(companyId, out var found))
            {
                state = found.Clone();
                return true;
            }
            state = null;
            return false;
        }

        public IReadOnlyList<CompanyState> ListCompanies()
        {
            return _companies.Values.Select(c => c.Clone()).ToList();
        }

        /// <summary>
        /// Found a company that is currently NotFounded → Active, with optional seed money.
        /// </summary>
        public bool Found(string companyId, int startingMoney = 0)
        {
            if (string.IsNullOrEmpty(companyId) || !_companies.TryGetValue(companyId, out var c))
                return false;
            if (c.Status != CompanyStatus.NotFounded)
                return false;

            c.Apply(startingMoney, progressDelta: 5, publicOpinionDelta: 5, engineeringDelta: 1,
                newStatus: CompanyStatus.Active);
            CompaniesChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Applies a problem choice's deltas to the target company.
        /// Returns false if company missing.
        /// </summary>
        public bool ApplyChoice(string companyId, ProblemChoice choice)
        {
            if (choice == null)
                throw new ArgumentNullException(nameof(choice));
            if (string.IsNullOrEmpty(companyId) || !_companies.TryGetValue(companyId, out var c))
                return false;

            c.Apply(
                choice.MoneyDelta,
                choice.ProgressDelta,
                choice.PublicOpinionDelta,
                choice.EngineeringDelta,
                choice.ResultStatus);
            CompaniesChanged?.Invoke();
            return true;
        }

        /// <summary>Direct stat/status update for narrative events.</summary>
        public bool ApplyDelta(
            string companyId,
            int money = 0,
            int progress = 0,
            int publicOpinion = 0,
            int engineering = 0,
            CompanyStatus? status = null)
        {
            if (string.IsNullOrEmpty(companyId) || !_companies.TryGetValue(companyId, out var c))
                return false;
            c.Apply(money, progress, publicOpinion, engineering, status);
            CompaniesChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            _companies.Clear();
            CompaniesChanged?.Invoke();
        }
    }
}
