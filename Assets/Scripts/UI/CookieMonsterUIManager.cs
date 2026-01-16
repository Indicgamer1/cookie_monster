using UnityEngine;
using System.Collections.Generic;
using Eduzo.Games.CookieMonster.Core;

namespace Eduzo.Games.CookieMonster.UI
{
    /// <summary>
    /// Centralized UI Manager
    /// Follows Single Responsibility Principle - manages UI screens and transitions
    /// Uses dictionary for O(1) screen lookup
    /// </summary>
    public class CookieMonsterUIManager : MonoBehaviour
    {
        [System.Serializable]
        public class ScreenEntry
        {
            public string screenName;
            public CookieMonsterUIScreen screen;
        }

        [Header("Screens")]
        [SerializeField] private List<ScreenEntry> _screens = new List<ScreenEntry>();

        [Header("HUD")]
        [SerializeField] private CookieMonsterGameHUD cookieMonsterGameHUD;

        private Dictionary<string, CookieMonsterUIScreen> _screenLookup;
        private CookieMonsterUIScreen _currentScreen;
        private CookieMonsterEventManager _cookieMonsterEventManager;

        private void Awake()
        {
            // Keep all screen GameObjects active for event subscriptions
            // Visibility is controlled by CanvasGroup (set in UIScreen.Initialize)
            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();

            // Initialize screens (this will set CanvasGroup alpha to 0)
            InitializeScreens();

            Debug.Log("UIManager: All screens initialized (visible via CanvasGroup)");
        }

        private void InitializeScreens()
        {
            _screenLookup = new Dictionary<string, CookieMonsterUIScreen>();

            foreach (var entry in _screens)
            {
                if (entry.screen != null && !string.IsNullOrEmpty(entry.screenName))
                {
                    _screenLookup[entry.screenName] = entry.screen;
                    entry.screen.Initialize();
                }
            }

            Debug.Log($"UI Manager initialized with {_screenLookup.Count} screens");
        }

        /// <summary>
        /// Shows the specified screen
        /// </summary>
        public void ShowScreen(string screenName)
        {
            if (screenName == "PracticeCompleteScreen")
            {
                Debug.Log("Practice mode ended!");
            }
            if (_screenLookup.TryGetValue(screenName, out CookieMonsterUIScreen screen))
            {
                _currentScreen = screen;
                screen.Show();

                _cookieMonsterEventManager?.Publish(new ScreenChangedEvent { screenName = screenName });

                Debug.Log($"Showing screen: {screenName}");
            }
            else
            {
                Debug.LogWarning($"Screen not found: {screenName}");
            }
        }

        /// <summary>
        /// Hides the specified screen
        /// </summary>
        public void HideScreen(string screenName)
        {
            if (_screenLookup.TryGetValue(screenName, out CookieMonsterUIScreen screen))
            {
                screen.Hide();
                Debug.Log($"Hiding screen: {screenName}");
            }
        }

        /// <summary>
        /// Hides current screen and shows new one
        /// </summary>
        public void TransitionToScreen(string screenName)
        {
            _currentScreen?.Hide();
            ShowScreen(screenName);
        }

        /// <summary>
        /// Updates loading progress
        /// </summary>
        public void UpdateLoadingProgress(float progress)
        {
            if (_screenLookup.TryGetValue("LoadingScreen", out CookieMonsterUIScreen screen))
            {
                (screen as CookieMonsterLoadingScreen)?.UpdateProgress(progress);
            }
        }

        /// <summary>
        /// Updates practice results
        /// </summary>
        public void UpdatePracticeResults(int score, float accuracy)
        {
            if (_screenLookup.TryGetValue("PracticeCompleteScreen", out CookieMonsterUIScreen screen))
            {
                (screen as CookieMonsterPracticeCompleteScreen)?.UpdateResults(score, accuracy);
            }
        }

        /// <summary>
        /// Updates game over results
        /// </summary>
        public void UpdateGameOverResults(int score, float accuracy, int highScore)
        {
            if (_screenLookup.TryGetValue("GameOverScreen", out CookieMonsterUIScreen screen))
            {
                (screen as CookieMonsterGameOverScreen)?.UpdateResults(score, accuracy, highScore);
            }
        }

        /// <summary>
        /// Shows VFX at specified position
        /// </summary>
        public void ShowVFX(string vfxName, Vector3 position)
        {
            _cookieMonsterEventManager?.Publish(new VFXRequestEvent
            {
                vfxName = vfxName,
                position = position
            });
        }

        public CookieMonsterUIScreen GetCurrentScreen() => _currentScreen;
        public CookieMonsterGameHUD GetGameHUD() => cookieMonsterGameHUD;
    }
}
