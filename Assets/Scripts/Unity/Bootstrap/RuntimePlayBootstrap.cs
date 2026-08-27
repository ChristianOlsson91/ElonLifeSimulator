using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Characters;
using ElonLifeSim.Unity.Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElonLifeSim.Unity.Bootstrap
{
    /// <summary>
    /// Failsafe: after every scene load, ensure the player never stares at an empty blue Game view.
    /// Scenes were authored with only a setup MonoBehaviour (no camera). If that script fails to bind
    /// or the user presses Play on an empty/untitled scene, this still builds the prototype UI.
    /// </summary>
    public static class RuntimePlayBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            // Only in Play Mode (this attribute does not run in Edit Mode for AfterSceneLoad the same way,
            // but guard anyway).
            if (!Application.isPlaying)
                return;

            var scene = SceneManager.GetActiveScene();
            var name = scene.name ?? string.Empty;

            Debug.Log($"[ElonLifeSim] RuntimePlayBootstrap after load: '{name}'");

            // Untitled / empty editor play → treat as main menu so Play always does something.
            if (string.IsNullOrEmpty(name) ||
                name == "Untitled" ||
                name == PrototypeContent.SceneMainMenu ||
                name == "MainMenu")
            {
                EnsureMainMenu();
                return;
            }

            if (name == PrototypeContent.SceneSouthAfrica || name.Contains("SouthAfrica") || name.Contains("Pretoria"))
            {
                EnsureGameplay(PrototypeContent.LocationPretoria);
                return;
            }

            if (name == PrototypeContent.SceneCanada || name.Contains("Canada") || name.Contains("Toronto"))
            {
                EnsureGameplay(PrototypeContent.LocationToronto);
                return;
            }

            if (name == PrototypeContent.SceneSiliconValley || name.Contains("Silicon") || name.Contains("Palo"))
            {
                EnsureGameplay(PrototypeContent.LocationPaloAlto);
                return;
            }

            // Unknown scene: at least provide a camera so Game view is not default void.
            EnsureCamera(new Color(0.08f, 0.1f, 0.16f));
        }

        private static void EnsureMainMenu()
        {
            EnsureCamera(new Color(0.08f, 0.1f, 0.16f));

            var existing = Object.FindFirstObjectByType<MainMenuSceneSetup>();
            if (existing != null)
            {
                existing.EnsureBuilt();
                return;
            }

            var go = new GameObject("MainMenuSceneSetup_Runtime");
            go.AddComponent<MainMenuSceneSetup>().EnsureBuilt();
        }

        private static void EnsureGameplay(string locationId)
        {
            var palette = WorldBackdropTokens.ForLocation(locationId);
            var bg = new Color(palette.SkyR, palette.SkyG, palette.SkyB, 1f);
            var ground = new Color(palette.GroundR, palette.GroundG, palette.GroundB, 1f);
            EnsureCamera(bg);

            var existing = Object.FindFirstObjectByType<GameplaySceneSetup>();
            if (existing == null)
            {
                var go = new GameObject("GameplaySceneSetup_Runtime");
                var setup = go.AddComponent<GameplaySceneSetup>();
                setup.Configure(locationId, bg, ground);
            }
            else
            {
                existing.RefreshPlayer(locationId);
            }

            ElonAppearanceController.Ensure();
            DebugLocationJump.Ensure();
        }

        private static void EnsureCamera(Color background)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                if (Object.FindFirstObjectByType<AudioListener>() == null)
                    camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.enabled = true;
        }
    }
}
