using ElonLifeSim.Core.Models;
using UnityEngine;

namespace ElonLifeSim.Data
{
    /// <summary>
    /// ScriptableObject for a company. Maps to pure <see cref="CompanyState"/> at runtime.
    /// Create via Assets → Create → ElonLifeSim → Company Definition.
    /// </summary>
    [CreateAssetMenu(fileName = "Company_", menuName = "ElonLifeSim/Company Definition", order = 12)]
    public sealed class CompanyDefinition : ScriptableObject
    {
        public string companyId;
        public string displayName;
        [TextArea(2, 4)]
        public string summary;

        [Header("Starting Stats")]
        public int money;
        public int progress;
        public int publicOpinion = 40;
        public int engineeringLevel;
        public CompanyStatus startingStatus = CompanyStatus.NotFounded;

        public CompanyState ToState()
        {
            return new CompanyState(
                companyId,
                displayName,
                summary,
                money,
                progress,
                publicOpinion,
                engineeringLevel,
                startingStatus);
        }
    }
}
