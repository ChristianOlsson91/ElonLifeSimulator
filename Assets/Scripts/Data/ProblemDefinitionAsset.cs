using System;
using System.Collections.Generic;
using ElonLifeSim.Core.Models;
using UnityEngine;

namespace ElonLifeSim.Data
{
    /// <summary>
    /// ScriptableObject wrapper for a company inbox problem with 2–3 choices.
    /// Community extension point — mirrors pure ProblemDefinition.
    /// </summary>
    [CreateAssetMenu(fileName = "Problem_", menuName = "ElonLifeSim/Problem Definition", order = 13)]
    public sealed class ProblemDefinitionAsset : ScriptableObject
    {
        public string problemId;
        public string companyId;
        public string companyDisplayName;
        public string locationId;
        public string locationDisplayName;
        public string title;
        [TextArea(3, 8)]
        public string description;
        [Range(1, 5)]
        public int difficulty = 2;
        public string rewardDescription;

        public List<ChoiceData> choices = new List<ChoiceData>();

        [Serializable]
        public class ChoiceData
        {
            public string choiceId;
            public string text;
            [TextArea(2, 4)]
            public string outcomeNarration;
            public int moneyDelta;
            public int progressDelta;
            public int publicOpinionDelta;
            public int engineeringDelta;
            public bool setStatus;
            public CompanyStatus resultStatus;
        }

        public ProblemDefinition ToProblem()
        {
            var list = new List<ProblemChoice>();
            foreach (var c in choices)
            {
                list.Add(new ProblemChoice(
                    c.choiceId,
                    c.text,
                    c.outcomeNarration,
                    c.moneyDelta,
                    c.progressDelta,
                    c.publicOpinionDelta,
                    c.engineeringDelta,
                    c.setStatus ? c.resultStatus : (CompanyStatus?)null));
            }
            return new ProblemDefinition(
                problemId,
                companyId,
                companyDisplayName,
                locationId,
                locationDisplayName,
                title,
                description,
                difficulty,
                rewardDescription,
                list);
        }
    }
}
