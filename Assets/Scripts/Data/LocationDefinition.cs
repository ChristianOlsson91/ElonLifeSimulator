using ElonLifeSim.Core.Models;
using UnityEngine;

namespace ElonLifeSim.Data
{
    /// <summary>
    /// ScriptableObject for a travel location. Create under Assets/Data/Locations/.
    /// </summary>
    [CreateAssetMenu(fileName = "Location_", menuName = "ElonLifeSim/Location Definition", order = 11)]
    public sealed class LocationDefinition : ScriptableObject
    {
        public string locationId;
        public string displayName;
        [Tooltip("Must match a scene name in Build Settings.")]
        public string sceneName;
        public string eraHint;
        [TextArea(2, 5)]
        public string description;

        public GameLocation ToLocation()
        {
            return new GameLocation(locationId, displayName, sceneName, eraHint, description);
        }
    }
}
