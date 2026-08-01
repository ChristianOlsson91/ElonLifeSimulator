using ElonLifeSim.Core.Content;
using ElonLifeSim.Core.Services;
using UnityEngine;

namespace ElonLifeSim.Unity.Bootstrap
{
    /// <summary>
    /// Persistent game entry: owns the pure <see cref="GameSession"/> singleton for the play session.
    /// Place on a bootstrap object in MainMenu (or first loaded scene); DontDestroyOnLoad.
    ///
    /// Extension: swap content providers later without changing UI controllers.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        public GameSession Session { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            // Optional auto-create if scene forgot the bootstrap object.
            if (Instance != null) return;
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Session = new GameSession();
        }

        /// <summary>Called by Main Menu "New Game".</summary>
        public void StartNewGame()
        {
            if (Session == null)
                Session = new GameSession();
            Session.StartNewGame();
        }

        public static GameSession RequireSession()
        {
            if (Instance == null || Instance.Session == null)
            {
                Debug.LogError("[ElonLifeSim] GameBootstrap/Session missing.");
                return null;
            }
            return Instance.Session;
        }

        /// <summary>Scene name helpers for UI / travel.</summary>
        public static string MainMenuScene => PrototypeContent.SceneMainMenu;
        public static string StartScene => PrototypeContent.SceneSouthAfrica;
    }
}
