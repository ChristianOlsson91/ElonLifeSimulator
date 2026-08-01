using System;

namespace ElonLifeSim.Core.Models
{
    /// <summary>
    /// A travel destination (city / facility / planet).
    ///
    /// Extension: register new locations in content data and SceneRegistry —
    /// map locationId → Unity scene name.
    /// </summary>
    [Serializable]
    public sealed class GameLocation
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string SceneName { get; }
        public string EraHint { get; }
        public string Description { get; }

        public GameLocation(
            string id,
            string displayName,
            string sceneName,
            string eraHint = "",
            string description = "")
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Location id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name is required.", nameof(displayName));
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("Scene name is required.", nameof(sceneName));

            Id = id;
            DisplayName = displayName;
            SceneName = sceneName;
            EraHint = eraHint ?? string.Empty;
            Description = description ?? string.Empty;
        }
    }
}
