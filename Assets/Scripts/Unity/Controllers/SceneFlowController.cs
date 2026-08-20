using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using ElonLifeSim.Unity.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElonLifeSim.Unity.Controllers
{
    /// <summary>
    /// Listens to TravelService.LocationChanged and loads the matching Unity scene.
    /// Attach once in gameplay scenes (or on bootstrap).
    /// </summary>
    public sealed class SceneFlowController : MonoBehaviour
    {
        private GameSession _session;
        private bool _subscribed;
        private bool _loading;

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
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
            if (session == null || _subscribed) return;
            _session = session;
            _session.Travel.LocationChanged += OnLocationChanged;
            _subscribed = true;
        }

        private void OnLocationChanged(string previousId, string newId)
        {
            ElonAppearanceApplier.Apply(newId);

            if (_loading) return;
            var sceneName = PrototypeContent.GetSceneForLocation(newId);
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning($"[ElonLifeSim] No scene mapped for location '{newId}'.");
                return;
            }

            if (SceneManager.GetActiveScene().name == sceneName)
                return;

            _loading = true;
            SceneManager.LoadScene(sceneName);
            _loading = false;
        }
    }
}
