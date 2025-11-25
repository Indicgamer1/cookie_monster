using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Core;
using CookieGame.UI;
using CookieGame.Audio;
using CookieGame.Gameplay;

namespace CookieGame.GameStates
{
    /// <summary>
    /// Main gameplay state - handles the division game loop
    /// Follows Single Responsibility by delegating to specialized managers
    /// </summary>
    public class GameplayState : GameState
    {
        private readonly bool _isPracticeMode;

        private EventManager _eventManager;
        private UIManager _uiManager;
        private AudioManager _audioManager;

        private QuestionGenerator _questionGenerator;
        private ScoreManager _scoreManager;
        private LivesManager _livesManager;
        private TimerManager _timerManager;
        private GameplayController _gameplayController;
        private DistributionManager _distributionManager; // GDD V2.0
        private RemainderErrorHandler _remainderErrorHandler; // GDD V2.0

        public GameplayState(bool isPracticeMode)
        {
            _isPracticeMode = isPracticeMode;
        }

        public override void Enter(GameStateManager manager)
        {
            base.Enter(manager);

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();
            _audioManager = ServiceLocator.Instance.Get<AudioManager>();

            // Subscribe to events
            _eventManager.Subscribe<AnswerSubmittedEvent>(OnAnswerSubmitted);
            _eventManager.Subscribe<TimerExpiredEvent>(OnTimerExpired);
            _eventManager.Subscribe<LivesDepletedEvent>(OnLivesDepleted);

            InitializeGameplay();

            _uiManager?.ShowScreen("GameplayScreen");
            _audioManager?.PlayMusic("GameplayMusic");

            Debug.Log($"Entered Gameplay State - Practice Mode: {_isPracticeMode}");
        }

        private void InitializeGameplay()
        {
            // Get or create gameplay managers
            var gameplayObject = GameObject.Find("GameplayController");
            if (gameplayObject == null)
            {
                gameplayObject = new GameObject("GameplayController");
            }

            _questionGenerator = gameplayObject.GetComponent<QuestionGenerator>() ?? gameplayObject.AddComponent<QuestionGenerator>();
            _scoreManager = gameplayObject.GetComponent<ScoreManager>() ?? gameplayObject.AddComponent<ScoreManager>();
            _livesManager = gameplayObject.GetComponent<LivesManager>() ?? gameplayObject.AddComponent<LivesManager>();
            _timerManager = gameplayObject.GetComponent<TimerManager>() ?? gameplayObject.AddComponent<TimerManager>();
            _gameplayController = gameplayObject.GetComponent<GameplayController>() ?? gameplayObject.AddComponent<GameplayController>();

            // GDD V2.0: Initialize distribution system
            _distributionManager = gameplayObject.GetComponent<DistributionManager>() ?? gameplayObject.AddComponent<DistributionManager>();
            _remainderErrorHandler = gameplayObject.GetComponent<RemainderErrorHandler>() ?? gameplayObject.AddComponent<RemainderErrorHandler>();

            // Initialize managers
            _scoreManager.Initialize();
            _livesManager.Initialize(_isPracticeMode);
            _timerManager.Initialize(_isPracticeMode);
            _questionGenerator.Initialize();
            _gameplayController.Initialize();

            // GDD V2.0: Initialize distribution manager with dependencies
            _distributionManager.Initialize(_eventManager, _scoreManager);

            Debug.Log("GDD V2.0: All gameplay managers initialized including DistributionManager");

            // Generate first question
            _questionGenerator.GenerateNewQuestion();
        }

        private void OnAnswerSubmitted(AnswerSubmittedEvent eventData)
        {
            if (eventData.isCorrect)
            {
                _scoreManager.AddCorrectAnswer();
                _audioManager?.PlaySFX("CorrectAnswer");
                _uiManager?.ShowVFX("CorrectVFX", Vector3.zero);
            }
            else
            {
                _scoreManager.AddWrongAnswer();
                _livesManager.LoseLife();
                _audioManager?.PlaySFX("WrongAnswer");
                _uiManager?.ShowVFX("WrongVFX", Vector3.zero);
            }

            // Generate next question if game is still ongoing
            if (_livesManager.HasLives() && _timerManager.HasTime())
            {
                _questionGenerator.GenerateNewQuestion();
            }
        }

        private void OnTimerExpired(TimerExpiredEvent eventData)
        {
            EndGameplay();
        }

        private void OnLivesDepleted(LivesDepletedEvent eventData)
        {
            EndGameplay();
        }

        private void EndGameplay()
        {
            int finalScore = _scoreManager.GetScore();
            float accuracy = _scoreManager.GetAccuracy();

            if (_isPracticeMode)
            {
                stateManager.ChangeState(new PracticeCompleteState(finalScore, accuracy));
            }
            else
            {
                stateManager.ChangeState(new GameOverState(finalScore, accuracy));
            }
        }

        public override void Update()
        {
            // State updates handled by individual managers
        }

        public override void Exit()
        {
            // Unsubscribe from events
            _eventManager.Unsubscribe<AnswerSubmittedEvent>(OnAnswerSubmitted);
            _eventManager.Unsubscribe<TimerExpiredEvent>(OnTimerExpired);
            _eventManager.Unsubscribe<LivesDepletedEvent>(OnLivesDepleted);

            _uiManager?.HideScreen("GameplayScreen");
            _audioManager?.StopMusic();

            // Cleanup
            if (_gameplayController != null)
            {
                Object.Destroy(_gameplayController.gameObject);
            }

            Debug.Log("Exited Gameplay State");
        }
    }

    // Additional events for gameplay
    public struct TimerExpiredEvent { }
    public struct LivesDepletedEvent { }
}
