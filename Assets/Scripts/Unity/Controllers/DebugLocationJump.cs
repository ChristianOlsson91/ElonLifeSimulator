using ElonLifeSim.Core.Services;
using ElonLifeSim.Unity.Bootstrap;
using ElonLifeSim.Unity.Characters;
using UnityEngine;

namespace ElonLifeSim.Unity.Controllers
{
    /// <summary>
    /// Editor / development-build F1–F5 jumps: Unlock + TravelTo, then the existing
    /// appearance refresh. Not present as a cheat menu in release players.
    /// </summary>
    public sealed class DebugLocationJump : MonoBehaviour
    {
        public static void Ensure()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (FindFirstObjectByType<DebugLocationJump>() != null)
                return;
            var go = new GameObject(nameof(DebugLocationJump));
            DontDestroyOnLoad(go);
            go.AddComponent<DebugLocationJump>();
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F1))
                Jump(1);
            else if (Input.GetKeyDown(KeyCode.F2))
                Jump(2);
            else if (Input.GetKeyDown(KeyCode.F3))
                Jump(3);
            else if (Input.GetKeyDown(KeyCode.F4))
                Jump(4);
            else if (Input.GetKeyDown(KeyCode.F5))
                Jump(5);
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static void Jump(int functionKey)
        {
            var session = GameBootstrap.RequireSession();
            if (session == null)
            {
                Debug.LogWarning("[DebugJump] No session.");
                return;
            }

            var result = DebugLocationJumpMap.TryJump(session.Travel, functionKey);
            Debug.Log(result.Log);

            if (result.PlaceMissing)
                return;

            // TravelTo no-ops when already there, so LocationChanged may not fire.
            if (!result.Moved && !string.IsNullOrEmpty(result.ToLocationId))
                ElonAppearanceApplier.Apply(result.ToLocationId);
        }
#endif
    }
}
