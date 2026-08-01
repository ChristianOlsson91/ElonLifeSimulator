using System.Collections.Generic;
using ElonLifeSim.Core.Models;

namespace ElonLifeSim.Core.Content
{
    /// <summary>
    /// Shared location ids, scene names, and registry helpers.
    /// Company/Act1/problem content lives in sibling content classes for modularity.
    ///
    /// Tone: respectful, inspiring, slightly humorous — never mocking.
    /// </summary>
    public static class PrototypeContent
    {
        public const string LocationPretoria = "pretoria";
        public const string LocationToronto = "toronto";
        public const string LocationPaloAlto = "palo_alto";

        public const string SceneMainMenu = "MainMenu";
        public const string SceneSouthAfrica = "SouthAfrica_Pretoria";
        public const string SceneCanada = "Canada_Toronto";
        public const string SceneSiliconValley = "SiliconValley_PaloAlto";

        public static IReadOnlyList<GameLocation> CreateLocations()
        {
            return new List<GameLocation>
            {
                new GameLocation(
                    LocationPretoria,
                    "Pretoria, South Africa",
                    SceneSouthAfrica,
                    eraHint: "Act 1 – Childhood",
                    description: "Home, school, library. Books, hard days, and the first spark of ambition."),
                new GameLocation(
                    LocationToronto,
                    "Toronto, Canada",
                    SceneCanada,
                    eraHint: "Act 2 – Canada",
                    description: "A new continent. Study, work, and the road toward Silicon Valley."),
                new GameLocation(
                    LocationPaloAlto,
                    "Palo Alto / Silicon Valley",
                    SceneSiliconValley,
                    eraHint: "Act 2–3 – Companies",
                    description: "Zip2, X.com, and the early internet gold rush. PLACEHOLDER hub.")
            };
        }

        /// <summary>Maps location id → Unity scene name (also on GameLocation.SceneName).</summary>
        public static string GetSceneForLocation(string locationId)
        {
            foreach (var loc in CreateLocations())
            {
                if (loc.Id == locationId)
                    return loc.SceneName;
            }
            return null;
        }
    }
}
