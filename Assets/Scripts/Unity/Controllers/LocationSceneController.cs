using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Bootstrap;
using ElonLifeSim.Unity.Characters;
using ElonLifeSim.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.Controllers
{
    /// <summary>
    /// Per-location scene glue: location name, Act 1 story auto-start in Pretoria,
    /// ensures HUD (Inbox, Travel, Dialogue, Companies) exists.
    /// </summary>
    public sealed class LocationSceneController : MonoBehaviour
    {
        [Tooltip("Must match PrototypeContent location id, e.g. pretoria or toronto.")]
        [SerializeField] private string locationId = PrototypeContent.LocationPretoria;

        [SerializeField] private Text locationLabel;
        [SerializeField] private bool playAct1OnStart = true;

        private static bool s_act1AutoStarted;

        /// <summary>Called by runtime scene setup when building without inspector wiring.</summary>
        public void SetLocationId(string id)
        {
            if (!string.IsNullOrEmpty(id))
                locationId = id;
        }

        private void Start()
        {
            var session = GameBootstrap.RequireSession();
            if (session == null)
            {
                var bootstrap = GameBootstrap.Instance;
                if (bootstrap != null)
                    bootstrap.StartNewGame();
                session = GameBootstrap.RequireSession();
            }

            // Align travel state with loaded scene without forcing unlock of locked regions.
            if (session != null && session.Travel.CurrentLocationId != locationId)
            {
                if (session.Travel.TryGetLocation(locationId, out _))
                {
                    if (!session.Travel.IsUnlocked(locationId))
                        session.Travel.Unlock(locationId);
                    // Soft-set without treating as player travel from map.
                    session.Travel.SetStartingLocation(locationId);
                }
            }

            if (locationLabel != null && session?.Travel.CurrentLocation != null)
                locationLabel.text = session.Travel.CurrentLocation.DisplayName;
            else if (locationLabel != null)
                locationLabel.text = locationId;

            EnsureHud();
            ElonAppearanceController.Ensure();
            ElonAppearanceApplier.Apply(locationId);

            if (playAct1OnStart && locationId == PrototypeContent.LocationPretoria && !s_act1AutoStarted)
            {
                s_act1AutoStarted = true;
                var story = FindFirstObjectByType<Act1StoryUI>();
                if (story != null)
                    story.TryAutoStart();
            }
        }

        private void EnsureHud()
        {
            if (FindFirstObjectByType<GameplayHudBuilder>() == null)
            {
                var go = new GameObject("GameplayHudBuilder");
                go.AddComponent<GameplayHudBuilder>();
            }
            if (FindFirstObjectByType<SceneFlowController>() == null)
            {
                var go = new GameObject("SceneFlowController");
                go.AddComponent<SceneFlowController>();
            }
        }
    }
}
