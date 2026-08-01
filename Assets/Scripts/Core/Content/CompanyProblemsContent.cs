using System.Collections.Generic;
using System.Linq;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Historically inspired Zip2 + early X.com inbox problems (4–6) with 2–3 choices each.
    /// Choices apply company-stat consequences via CompanyManager.ApplyChoice.
    /// Tone: respectful / non-mocking approximations of real business pressures.
    /// </summary>
    public static class CompanyProblemsContent
    {
        public const string Zip2FirstCustomer = "zip2_first_big_customer";
        public const string Zip2MapsTech = "zip2_maps_directories_tech";
        public const string Zip2Sale = "zip2_sale_to_compaq";
        public const string XComFraudBanking = "xcom_fraud_banking_pressure";
        public const string XComMerger = "xcom_confinity_merger";
        public const string XComPower = "xcom_internal_leadership";

        public static IReadOnlyList<ProblemDefinition> CreateAll()
        {
            return new List<ProblemDefinition>
            {
                Zip2FirstBigCustomer(),
                Zip2MapsAndDirectories(),
                Zip2SaleDecision(),
                XComBankingAndFraud(),
                XComConfinityMerger(),
                XComLeadershipStruggle()
            };
        }

        public static IReadOnlyList<ProblemDefinition> CreateZip2Problems()
        {
            return CreateAll().Where(p => p.CompanyId == CompanyContent.Zip2).ToList();
        }

        public static IReadOnlyList<ProblemDefinition> CreateXComProblems()
        {
            return CreateAll().Where(p => p.CompanyId == CompanyContent.XCom).ToList();
        }

        public static bool TryGet(string problemId, out ProblemDefinition problem)
        {
            foreach (var p in CreateAll())
            {
                if (p.Id == problemId)
                {
                    problem = p;
                    return true;
                }
            }
            problem = null;
            return false;
        }

        private static ProblemDefinition Zip2FirstBigCustomer()
        {
            return new ProblemDefinition(
                Zip2FirstCustomer,
                CompanyContent.Zip2,
                "Zip2",
                PrototypeContent.LocationPaloAlto,
                "Palo Alto / Silicon Valley",
                "The Yellow Pages Deal",
                "Newspapers want digital city guides. A major yellow-pages style partnership could put Zip2 " +
                "on the map — but the sales cycle is brutal and the product still needs to prove it can " +
                "scale maps and listings for real publishers.",
                difficulty: 3,
                rewardDescription: "Money + Progress · or engineering debt if overpromised",
                choices: new List<ProblemChoice>
                {
                    new ProblemChoice(
                        "close_deal",
                        "Push hard to close the big customer — ship what they need",
                        "You land the deal. Cash and credibility jump; the team sprints to keep up.",
                        moneyDelta: 40,
                        progressDelta: 15,
                        publicOpinionDelta: 10,
                        engineeringDelta: 5),
                    new ProblemChoice(
                        "overpromise",
                        "Promise everything on the roadmap to win the contract",
                        "They sign. Reputation spikes — then engineering staggers under the weight of promises.",
                        moneyDelta: 50,
                        progressDelta: 5,
                        publicOpinionDelta: 15,
                        engineeringDelta: -5),
                    new ProblemChoice(
                        "walk_away",
                        "Walk away if terms would break the product",
                        "No deal today. Painful, but the product stays coherent and the team trusts you.",
                        moneyDelta: -5,
                        progressDelta: 5,
                        publicOpinionDelta: -5,
                        engineeringDelta: 10)
                });
        }

        private static ProblemDefinition Zip2MapsAndDirectories()
        {
            return new ProblemDefinition(
                Zip2MapsTech,
                CompanyContent.Zip2,
                "Zip2",
                PrototypeContent.LocationPaloAlto,
                "Palo Alto / Silicon Valley",
                "Maps & Directories Under Load",
                "Online maps and business directories are harder than the demo. Data is messy, rendering is " +
                "slow, and every newspaper wants their own quirks. Someone has to choose where the scarce " +
                "engineering hours go.",
                difficulty: 3,
                rewardDescription: "Engineering / Progress trade-offs",
                choices: new List<ProblemChoice>
                {
                    new ProblemChoice(
                        "invest_maps",
                        "Invest in map performance and cleaner data pipelines",
                        "Maps get usable. Engineering level rises; growth is a bit slower this quarter.",
                        moneyDelta: -15,
                        progressDelta: 10,
                        publicOpinionDelta: 5,
                        engineeringDelta: 20),
                    new ProblemChoice(
                        "sales_features",
                        "Prioritize custom features sales is promising",
                        "Deals stay warm and progress looks good on slides — the core stack gets messier.",
                        moneyDelta: 20,
                        progressDelta: 15,
                        publicOpinionDelta: 5,
                        engineeringDelta: -5),
                    new ProblemChoice(
                        "hire_carefully",
                        "Hire carefully and refactor before the next wave of customers",
                        "Short-term revenue is quiet; the foundation for scale improves.",
                        moneyDelta: -25,
                        progressDelta: 5,
                        publicOpinionDelta: 0,
                        engineeringDelta: 15)
                });
        }

        private static ProblemDefinition Zip2SaleDecision()
        {
            return new ProblemDefinition(
                Zip2Sale,
                CompanyContent.Zip2,
                "Zip2",
                PrototypeContent.LocationPaloAlto,
                "Palo Alto / Silicon Valley",
                "The Acquisition Offer",
                "Compaq (via AltaVista) is ready to buy Zip2 for a life-changing sum. Selling would free " +
                "you to aim at the internet's next layer — payments — but means letting go of the company " +
                "you and your brother built from nothing.",
                difficulty: 4,
                rewardDescription: "Massive liquidity · Zip2 Sold · unlock X.com path",
                choices: new List<ProblemChoice>
                {
                    new ProblemChoice(
                        "sell",
                        "Sell Zip2 — take the win and prepare for the next company",
                        "The sale closes. Zip2 is Sold. Capital and reputation open the door to X.com.",
                        moneyDelta: 200,
                        progressDelta: 10,
                        publicOpinionDelta: 20,
                        engineeringDelta: 0,
                        resultStatus: CompanyStatus.Sold),
                    new ProblemChoice(
                        "hold_out",
                        "Hold out for a better valuation",
                        "Talks drag. You might get more — or risk the window cooling. For now Zip2 stays Active.",
                        moneyDelta: 20,
                        progressDelta: 5,
                        publicOpinionDelta: -5,
                        engineeringDelta: 5,
                        resultStatus: CompanyStatus.Active),
                    new ProblemChoice(
                        "stay_independent",
                        "Refuse and stay independent longer",
                        "Independence preserved. Cash is tighter; the team keeps building city software.",
                        moneyDelta: -10,
                        progressDelta: 10,
                        publicOpinionDelta: 5,
                        engineeringDelta: 10,
                        resultStatus: CompanyStatus.Active)
                });
        }

        private static ProblemDefinition XComBankingAndFraud()
        {
            return new ProblemDefinition(
                XComFraudBanking,
                CompanyContent.XCom,
                "X.com",
                PrototypeContent.LocationPaloAlto,
                "Palo Alto / Silicon Valley",
                "Fraud, Risk & the Banking Gate",
                "Online money moves attract fraud. Regulators and banking partners want controls; growth " +
                "teams want fewer friction points. Get this wrong and you burn capital or lose the license " +
                "to operate.",
                difficulty: 4,
                rewardDescription: "Risk vs growth · opinion and money swing",
                choices: new List<ProblemChoice>
                {
                    new ProblemChoice(
                        "strict_controls",
                        "Ship strict fraud controls even if growth slows",
                        "Losses shrink. Partners calm down. Users grumble about friction.",
                        moneyDelta: -20,
                        progressDelta: 5,
                        publicOpinionDelta: 10,
                        engineeringDelta: 15),
                    new ProblemChoice(
                        "growth_first",
                        "Optimize for growth and patch fraud as it appears",
                        "User numbers soar — so do chargebacks. Cash and opinion take a hit until fixed.",
                        moneyDelta: -40,
                        progressDelta: 20,
                        publicOpinionDelta: -15,
                        engineeringDelta: 5),
                    new ProblemChoice(
                        "partner_bank",
                        "Lean on a partner bank and formal processes",
                        "Slower product cycles, stronger institutional trust, modest engineering gains.",
                        moneyDelta: 10,
                        progressDelta: 10,
                        publicOpinionDelta: 15,
                        engineeringDelta: 5)
                });
        }

        private static ProblemDefinition XComConfinityMerger()
        {
            return new ProblemDefinition(
                XComMerger,
                CompanyContent.XCom,
                "X.com",
                PrototypeContent.LocationPaloAlto,
                "Palo Alto / Silicon Valley",
                "Merger with Confinity (PayPal)",
                "Two young payments companies circle the same market. A merger could create a category " +
                "winner — and force painful product, brand, and culture choices under one roof.",
                difficulty: 5,
                rewardDescription: "Status → Merged · big progress swing",
                choices: new List<ProblemChoice>
                {
                    new ProblemChoice(
                        "merge_paypal_brand",
                        "Merge and lean into the PayPal brand for consumers",
                        "The combined company gains focus. Status becomes Merged; market position improves.",
                        moneyDelta: 30,
                        progressDelta: 25,
                        publicOpinionDelta: 15,
                        engineeringDelta: 10,
                        resultStatus: CompanyStatus.Merged),
                    new ProblemChoice(
                        "merge_x_brand",
                        "Merge but fight to keep the X.com financial-super-app vision front and center",
                        "Vision stays bold; integration is rockier. Progress still jumps.",
                        moneyDelta: 15,
                        progressDelta: 15,
                        publicOpinionDelta: 5,
                        engineeringDelta: 5,
                        resultStatus: CompanyStatus.Merged),
                    new ProblemChoice(
                        "no_merge",
                        "Refuse the merger and compete head-on",
                        "Independence is costly. War for users drains cash; engineering races.",
                        moneyDelta: -30,
                        progressDelta: 10,
                        publicOpinionDelta: -5,
                        engineeringDelta: 15,
                        resultStatus: CompanyStatus.Active)
                });
        }

        private static ProblemDefinition XComLeadershipStruggle()
        {
            return new ProblemDefinition(
                XComPower,
                CompanyContent.XCom,
                "X.com",
                PrototypeContent.LocationPaloAlto,
                "Palo Alto / Silicon Valley",
                "Boardroom & Product Direction",
                "After the merger chaos, leadership and product direction are contested. Do you push for " +
                "full-stack banking ambition, double down on payments excellence, or step back operationally " +
                "to reset trust with the team and board?",
                difficulty: 4,
                rewardDescription: "Opinion / engineering / progress shifts",
                choices: new List<ProblemChoice>
                {
                    new ProblemChoice(
                        "payments_focus",
                        "Champion payments reliability as the core product",
                        "Focus restores trust. Engineering and opinion climb; the super-app waits.",
                        moneyDelta: 10,
                        progressDelta: 15,
                        publicOpinionDelta: 15,
                        engineeringDelta: 10),
                    new ProblemChoice(
                        "full_stack",
                        "Keep pushing the broader financial platform vision",
                        "Ambition inspires some, exhausts others. Progress mixed; opinion divides.",
                        moneyDelta: -10,
                        progressDelta: 10,
                        publicOpinionDelta: -10,
                        engineeringDelta: 5),
                    new ProblemChoice(
                        "reset_trust",
                        "Reset — listen hard, re-align roles, reduce drama",
                        "Short-term velocity dips; culture and opinion recover for the next fight.",
                        moneyDelta: 0,
                        progressDelta: 5,
                        publicOpinionDelta: 20,
                        engineeringDelta: 5)
                });
        }
    }
}
