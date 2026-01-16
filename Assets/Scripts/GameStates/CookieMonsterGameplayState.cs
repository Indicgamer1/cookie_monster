using UnityEngine;
using Eduzo.Games.CookieMonster.Patterns;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.UI;
using Eduzo.Games.CookieMonster.Audio;
using Eduzo.Games.CookieMonster.Data;
using Eduzo.Games.CookieMonster.Gameplay;

namespace Eduzo.Games.CookieMonster.GameStates
{
    /// <summary>
    /// Main gameplay state - handles UI, audio, and state transitions
    /// Gameplay logic is handled by GameplayController
    /// </summary>
    public class CookieMonsterGameplayState : CookieMonsterGameState
    {
        private readonly bool _isPracticeMode;

        private CookieMonsterGameConfig _config;
        private CookieMonsterEventManager _cookieMonsterEventManager;
        private CookieMonsterUIManager _uiManager;
        private CookieMonsterAudioManager _audioManager;

        private CookieMonsterScoreManager _scoreManager;
        private CookieMonsterLivesManager _livesManager;
        private CookieMonsterTimerManager _timerManager;
        private CookieMonsterGameplayController _cookieMonsterGameplayController;
        private bool _gameEnded = false;

        public bool IsPracticeMode => _isPracticeMode;
        public CookieMonsterGameplayState(bool isPracticeMode)
        {
            _isPracticeMode = isPracticeMode;
            
            _config = Resources.Load<CookieMonsterGameConfig>("GameConfig");

            if (_config == null)
            {
                Debug.LogWarning("CookieMonsterGameConfig not found! Using default values.");
                _config = ScriptableObject.CreateInstance<CookieMonsterGameConfig>();
            }
        }

        public override void Enter(CookieMonsterGameStateManager manager)
        {
            base.Enter(manager);

            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();
            _uiManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterUIManager>();
            _audioManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterAudioManager>();

            // Subscribe to events
            _cookieMonsterEventManager.Subscribe<AnswerSubmittedEvent>(OnAnswerSubmitted);
            _cookieMonsterEventManager.Subscribe<TimerExpiredEvent>(OnTimerExpired);
            _cookieMonsterEventManager.Subscribe<LivesDepletedEvent>(OnLivesDepleted);
            _cookieMonsterEventManager.Subscribe<GameOverEvent>(OnGameOver);

            _gameEnded = false;
            InitializeGameplay();

            _uiManager?.ShowScreen("GameplayScreen");
            _audioManager?.PlayMusic("GameplayMusic");

            // Start the timer UI when gameplay state is entered
            var timerUI = Object.FindObjectOfType<TimerUI>();
            if (timerUI != null)
            {
                timerUI.StartTimer(_isPracticeMode ? (int)_config.practiceModeTimeLimit : (int)_config.testModeTimeLimit);
            }

            Debug.Log($"Entered Gameplay State - Practice Mode: {_isPracticeMode}");
        }

        private void InitializeGameplay()
        {
            // Get or create gameplay controller
            var gameplayObject = GameObject.Find("GameplayController");
            if (gameplayObject == null)
            {
                gameplayObject = new GameObject("GameplayController");
            }

            _cookieMonsterGameplayController = gameplayObject.GetComponent<CookieMonsterGameplayController>();
            if (_cookieMonsterGameplayController == null)
            {
                _cookieMonsterGameplayController = gameplayObject.AddComponent<CookieMonsterGameplayController>();
            }

            // Initialize other managers if they exist
            _scoreManager = gameplayObject.GetComponent<CookieMonsterScoreManager>();
            if (_scoreManager != null)
            {
                _scoreManager.Initialize();
            }

            _livesManager = gameplayObject.GetComponent<CookieMonsterLivesManager>();
            if (_livesManager == null)
            {
                _livesManager = gameplayObject.AddComponent<CookieMonsterLivesManager>();
            }
            // Always initialize to reset lives when starting a new game
            _livesManager.Initialize(_isPracticeMode);

            _timerManager = gameplayObject.GetComponent<CookieMonsterTimerManager>();
            if (_timerManager != null)
            {
                _timerManager.Initialize(_isPracticeMode);
            }

            // Initialize main gameplay controller (this starts the game)
            _cookieMonsterGameplayController.Initialize();
        }

        private void OnAnswerSubmitted(AnswerSubmittedEvent eventData)
        {
            // Handle audio/visual feedback
            if (eventData.isCorrect)
            {
                _audioManager?.PlaySFX("CorrectAnswer");
                _uiManager?.ShowVFX("CorrectVFX", Vector3.zero);
                if (_scoreManager != null)
                {
                    _scoreManager.AddCorrectAnswer();
                }
            }
            else
            {
                _audioManager?.PlaySFX("WrongAnswer");
                _uiManager?.ShowVFX("WrongVFX", Vector3.zero);
                if (_scoreManager != null)
                {
                    _scoreManager.AddWrongAnswer();
                }
                if (_livesManager != null)
                {
                    _livesManager.LoseLife();
                }
            }

            // Check if game should continue
            bool shouldContinue = true;
            if (_livesManager != null && !_livesManager.HasLives())
            {
                shouldContinue = false;
            }
            if (_timerManager != null && !_timerManager.HasTime())
            {
                shouldContinue = false;
            }

            if (!shouldContinue && !_gameEnded)
            {
                // Stop the timer when game ends
                var timerUI = Object.FindObjectOfType<TimerUI>();
                if (timerUI != null)
                {
                    timerUI.StopTimer();
                }
                EndGameplay();
            }
        }

        private void OnTimerExpired(TimerExpiredEvent eventData)
        {
            EndGameplay();
        }

        private void OnLivesDepleted(LivesDepletedEvent eventData)
        {
            if (_gameEnded) return;
            
            // Stop the timer when lives are depleted
            var timerUI = Object.FindObjectOfType<TimerUI>();
            if (timerUI != null)
            {
                timerUI.StopTimer();
            }
            EndGameplay();
        }

        private void OnGameOver(GameOverEvent eventData)
        {
            // Transition to game over state using event data
            if (_isPracticeMode)
            {
                stateManager.ChangeState(new CookieMonsterPracticeCompleteState(eventData.finalScore, eventData.accuracy));
            }
            else
            {
                stateManager.ChangeState(new CookieMonsterGameOverState(eventData.finalScore, eventData.accuracy));
            }
        }

        private void EndGameplay()
        {
            if (_gameEnded) return;
            _gameEnded = true;
            
            int finalScore = 0;
            float accuracy = 0f;

            // Get score from gameplay controller or score manager
            if (_cookieMonsterGameplayController != null)
            {
                finalScore = _cookieMonsterGameplayController.GetCurrentScore();
            }
            else if (_scoreManager != null)
            {
                finalScore = _scoreManager.GetScore();
                accuracy = _scoreManager.GetAccuracy();
            }

            // Clear submitted questions when game ends (will be re-submitted on restart)
            if (!_isPracticeMode && CookieMonsterNewQuestionGenerator.Instance != null)
            {
                CookieMonsterNewQuestionGenerator.Instance.ClearQuestions();
            }

            if (_isPracticeMode)
            {
                stateManager.ChangeState(new CookieMonsterPracticeCompleteState(finalScore, accuracy));
            }
            else
            {
                stateManager.ChangeState(new CookieMonsterGameOverState(finalScore, accuracy));
            }
        }

        public override void Update()
        {
            // State updates handled by individual managers
        }

        public override void Exit()
        {
            // Unsubscribe from events
            _cookieMonsterEventManager.Unsubscribe<AnswerSubmittedEvent>(OnAnswerSubmitted);
            _cookieMonsterEventManager.Unsubscribe<TimerExpiredEvent>(OnTimerExpired);
            _cookieMonsterEventManager.Unsubscribe<LivesDepletedEvent>(OnLivesDepleted);
            _cookieMonsterEventManager.Unsubscribe<GameOverEvent>(OnGameOver);

            _uiManager?.HideScreen("GameplayScreen");
            _audioManager?.StopMusic();

            // Cancel any pending operations in GameplayController (don't destroy, just reset)
            if (_cookieMonsterGameplayController != null)
            {
                _cookieMonsterGameplayController.CancelInvoke();
            }

            Debug.Log("Exited Gameplay State");
        }
    }

}
