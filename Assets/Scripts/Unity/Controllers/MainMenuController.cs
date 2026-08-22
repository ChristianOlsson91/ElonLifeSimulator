using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.Controllers
{
    /// <summary>
    /// Main menu: New Game starts a session and loads Pretoria. Esc matches Quit.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;

        private void Start()
        {
            if (newGameButton == null)
                newGameButton = FindButtonByName("NewGameButton");
            if (quitButton == null)
                quitButton = FindButtonByName("QuitButton");
            if (titleText == null)
                titleText = FindTextByName("Title");
            if (subtitleText == null)
                subtitleText = FindTextByName("Subtitle");

            if (titleText != null)
                titleText.text = UiStyleTokens.GameTitle;
            if (subtitleText != null)
                subtitleText.text = UiStyleTokens.GameSubtitle;

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnQuit();
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

        private static Text FindTextByName(string name)
        {
            var t = GameObject.Find(name);
            return t != null ? t.GetComponent<Text>() : null;
        }
    }
}
