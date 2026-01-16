using UnityEngine;
using Eduzo.Games.CookieMonster.Patterns;
using Eduzo.Games.CookieMonster.Audio;
using Eduzo.Games.CookieMonster.UI;
using Eduzo.Games.CookieMonster.GameStates;
using Eduzo.Games.CookieMonster.Gameplay;

namespace Eduzo.Games.CookieMonster.Core
{
    /// <summary>
    /// Main Game Manager - orchestrates all game systems
    /// Singleton pattern with DontDestroyOnLoad
    /// Follows Single Responsibility Principle by delegating to specialized managers
    /// </summary>
    public class CookieMonsterGameManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private CookieMonsterGameStateManager _stateManager;
        [SerializeField] private CookieMonsterAudioManager _audioManager;
        [SerializeField] private CookieMonsterUIManager _uiManager;
        [SerializeField] private CookieMonsterPoolManager _poolManager;

        private CookieMonsterEventManager _cookieMonsterEventManager;
        private static CookieMonsterGameManager _instance;

        public static CookieMonsterGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CookieMonsterGameManager>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {
            // Create and register all services
            _cookieMonsterEventManager = new CookieMonsterEventManager();
            
            CookieMonsterServiceLocator.Instance.Register(_cookieMonsterEventManager);
            CookieMonsterServiceLocator.Instance.Register(_stateManager);
            CookieMonsterServiceLocator.Instance.Register(_audioManager);
            CookieMonsterServiceLocator.Instance.Register(_uiManager);
            CookieMonsterServiceLocator.Instance.Register(_poolManager);

            Debug.Log("All services initialized and registered");
        }

        private void Start()
        {
            // Start directly at main menu (Splash and Student Name screens removed)
            _stateManager.ChangeState(new CookieMonsterMainMenuState());
        }
        
        public void PauseGame()
        {
            Time.timeScale = 0f;
            _cookieMonsterEventManager.Publish(new GamePausedEvent());
        }
        
        private void OnDestroy()
        {
            CookieMonsterServiceLocator.Instance.Clear();
            _cookieMonsterEventManager?.Clear();
        }
        
        #region Useless Code
        
        /*
        public void StartGame(bool isPracticeMode)
        {
            _eventManager.Publish(new GameStartedEvent());
            _stateManager.ChangeState(new GameplayState(isPracticeMode));
        }
        */

        /*
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            _eventManager.Publish(new GameResumedEvent());
        }
        */

        /*
        public void EndGame(int finalScore, float accuracy)
        {
            _eventManager.Publish(new GameOverEvent { finalScore = finalScore, accuracy = accuracy });
            _stateManager.ChangeState(new GameOverState(finalScore, accuracy));
        }
        */
        /*public void RestartGame()
        {
            _poolManager.ReturnAll<Cookie>("Cookie");
            Time.timeScale = 1f;
            _stateManager.ChangeState(new GameplayState(false));
        }*/
        /*public void QuitToMenu()
        {
            Time.timeScale = 1f;
            _stateManager.ChangeState(new MainMenuState());
        } */       
        #endregion
    }
}
