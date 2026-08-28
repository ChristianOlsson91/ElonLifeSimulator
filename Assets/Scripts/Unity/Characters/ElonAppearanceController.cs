using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;

namespace ElonLifeSim.Unity.Characters
{
    /// <summary>
    /// Listens to <see cref="TravelService.LocationChanged"/> and refreshes Elon
    /// sprites even when the destination scene is already loaded (or the player
    /// object survives the load).
    /// </summary>
    public sealed class ElonAppearanceController : MonoBehaviour
    {
        private GameSession _session;
        private bool _subscribed;

        public static ElonAppearanceController Ensure()
        {
            var existing = Object.FindFirstObjectByType<ElonAppearanceController>();
            if (existing != null)
                return existing;

            var go = new GameObject(nameof(ElonAppearanceController));
            Object.DontDestroyOnLoad(go);
            return go.AddComponent<ElonAppearanceController>();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
            ApplyCurrent();
        }

        private void OnDisable()
        {
            if (_session != null && _subscribed)
            {
                _session.Travel.LocationChanged -= OnLocationChanged;
                _subscribed = false;
            }
        }

        private void TrySubscribe()
        {
            var session = GameBootstrap.RequireSession();
            if (session == null || _subscribed)
                return;
            _session = session;
            _session.Travel.LocationChanged += OnLocationChanged;
            _subscribed = true;
        }

        private void OnLocationChanged(string previousId, string newId)
        {
            ElonAppearanceApplier.Apply(newId);
        }

        private void ApplyCurrent()
        {
            var loc = _session?.Travel.CurrentLocationId;
            if (!string.IsNullOrEmpty(loc))
                ElonAppearanceApplier.Apply(loc);
        }
    }
}
