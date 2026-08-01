using System.Collections.Generic;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Starting company definitions (Zip2, X.com first). Status begins NotFounded.
    /// EXTENSION: add Tesla/SpaceX later the same way.
    /// </summary>
    public static class CompanyContent
    {
        public const string Zip2 = "zip2";
        public const string XCom = "xcom";

        public static IReadOnlyList<CompanyState> CreateStartingCompanies()
        {
            return new List<CompanyState>
            {
                new CompanyState(
                    Zip2,
                    "Zip2",
                    summary: "Online city guide / maps & directory software for newspapers (mid–late 1990s).",
                    money: 0,
                    progress: 0,
                    publicOpinion: 40,
                    engineeringLevel: 0,
                    status: CompanyStatus.NotFounded),
                new CompanyState(
                    XCom,
                    "X.com",
                    summary: "Online banking / payments that will collide with Confinity and become PayPal.",
                    money: 0,
                    progress: 0,
                    publicOpinion: 40,
                    engineeringLevel: 0,
                    status: CompanyStatus.NotFounded)
            };
        }
    }
}
