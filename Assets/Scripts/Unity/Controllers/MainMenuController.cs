using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.Controllers
{
    /// <summary>
    /// Main menu: New Game → bootstrap session → load South Africa start scene.
    /// Wire buttons in the MainMenu scene (or use runtime UI builder).
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;

        private void Start()
        {
            // Runtime-friendly: find buttons if not wired in inspector.
            if (newGameButton == null)
                newGameButton = FindButtonByName("NewGameButton");
            if (quitButton == null)
                quitButton = FindButtonByName("QuitButton");

            if (titleText != null)
                titleText.text = "Elon: The Life Simulator";
            if (subtitleText != null)
                subtitleText.text = "A respectful narrative life-sim · Prototype";

            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveAllListeners();
                newGameButton.onClick.AddListener(OnNewGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuit);
            }
        }

        public void OnNewGame()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                var go = new GameObject("GameBootstrap");
                bootstrap = go.AddComponent<GameBootstrap>();
            }

            bootstrap.StartNewGame();
            SceneManager.LoadScene(PrototypeContent.SceneSouthAfrica);
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static Button FindButtonByName(string name)
        {
            var t = GameObject.Find(name);
            return t != null ? t.GetComponent<Button>() : null;
        }
    }
}
