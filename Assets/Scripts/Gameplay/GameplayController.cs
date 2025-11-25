using UnityEngine;
using CookieGame.Core;
using CookieGame.Data;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Main gameplay controller - manages 3-question game loop
    /// Player manually collects cookies in baskets, then submits answer
    /// 10 points per correct answer, 0 for wrong
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private QuestionGenerator _questionGenerator;
        [SerializeField] private MonsterController[] _allMonsters; // All 5 monsters
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private CookieSpawner _cookieSpawner;

        private EventManager _eventManager;
        private GameConfig _config;

        // Game state
        private int _currentQuestionNumber;
        private int _currentDividend;
        private int _currentDivisor;
        private int _currentQuotient;
        private int _totalScore;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _config = Resources.Load<GameConfig>("GameConfig");

            if (_config == null)
            {
                Debug.LogWarning("GameConfig not found! Using default values.");
                _config = ScriptableObject.CreateInstance<GameConfig>();
            }

            // Initialize components
            if (_questionGenerator == null)
                _questionGenerator = GetComponent<QuestionGenerator>();

            if (_scoreManager == null)
                _scoreManager = FindObjectOfType<ScoreManager>();

            if (_cookieSpawner == null)
                _cookieSpawner = FindObjectOfType<CookieSpawner>();

            // Find all monsters if not assigned
            if (_allMonsters == null || _allMonsters.Length == 0)
            {
                _allMonsters = FindObjectsOfType<MonsterController>();
            }

            // Subscribe to events
            _eventManager.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);

            // Initialize game
            _currentQuestionNumber = 0;
            _totalScore = 0;

            // Start first question
            StartNewQuestion();
        }

        /// <summary>
        /// Starts a new question (1 of 3)
        /// </summary>
        private void StartNewQuestion()
        {
            _currentQuestionNumber++;

            Debug.Log($"=== Starting Question {_currentQuestionNumber} of {_config.totalQuestions} ===");

            // Check if game is over
            if (_currentQuestionNumber > _config.totalQuestions)
            {
                OnGameOver();
                return;
            }

            // Generate new question
            _questionGenerator.Initialize();
        
        }

        /// <summary>
        /// Called when a new question is generated
        /// </summary>
        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            _currentDividend = eventData.dividend;
            _currentDivisor = eventData.divisor;
            _currentQuotient = eventData.correctAnswer;

            Debug.Log($"Question {_currentQuestionNumber}: {_currentDividend} ÷ {_currentDivisor} = ?");
            Debug.Log($"Divisor: {_currentDivisor} (Show {_currentDivisor} monsters)");

            // Show/hide monsters based on divisor
            SetActiveMonsters(_currentDivisor);

            // Reset all monster cookie counts
            ResetAllMonsters();

            // Spawn cookies for manual collection
            if (_cookieSpawner != null)
            {
                _cookieSpawner.SpawnCookies(_currentDividend);
            }
        }

        /// <summary>
        /// Shows only the number of monsters equal to divisor
        /// Disables extra monsters if divisor < 5
        /// </summary>
        private void SetActiveMonsters(int divisor)
        {
            if (_allMonsters == null || _allMonsters.Length == 0)
            {
                Debug.LogError("No monsters assigned!");
                return;
            }

            for (int i = 0; i < _allMonsters.Length; i++)
            {
                if (_allMonsters[i] != null)
                {
                    // Show monster if index < divisor
                    bool shouldBeActive = i < divisor;
                    _allMonsters[i].gameObject.SetActive(shouldBeActive);

                    if (shouldBeActive)
                    {
                        _allMonsters[i].SetMonsterIndex(i);
                        Debug.Log($"Monster {i} ENABLED");
                    }
                    else
                    {
                        Debug.Log($"Monster {i} DISABLED");
                    }
                }
            }
        }

        /// <summary>
        /// Resets cookie counts for all monsters
        /// </summary>
        private void ResetAllMonsters()
        {
            foreach (var monster in _allMonsters)
            {
                if (monster != null && monster.gameObject.activeInHierarchy)
                {
                    monster.ResetCookies();
                }
            }
        }

        /// <summary>
        /// Called when player clicks Submit button
        /// Checks if each active monster has correct number of cookies
        /// </summary>
        public void OnSubmitAnswer()
        {
            Debug.Log($"=== Submit Answer - Question {_currentQuestionNumber} ===");

            // Get active monsters
            MonsterController[] activeMonsters = GetActiveMonsters();

            if (activeMonsters.Length != _currentDivisor)
            {
                Debug.LogError($"Active monsters ({activeMonsters.Length}) != divisor ({_currentDivisor})");
                return;
            }

            // Check each monster's cookie count
            bool allCorrect = true;
            for (int i = 0; i < activeMonsters.Length; i++)
            {
                int cookieCount = activeMonsters[i].GetCookieCount();
                Debug.Log($"Monster {i}: Has {cookieCount} cookies (Expected: {_currentQuotient})");

                if (cookieCount != _currentQuotient)
                {
                    allCorrect = false;
                }
            }

            // Award points
            if (allCorrect)
            {
                // CORRECT - All monsters have quotient cookies
                int points = _config.pointsPerCorrectAnswer;
                _totalScore += points;

                Debug.Log($"✓ CORRECT! Each monster has {_currentQuotient} cookies.");
                Debug.Log($"+{points} points! Total Score: {_totalScore}");

                // Show happy reaction
                ShowAllMonstersReaction(true);

                // Publish correct answer event
                _eventManager.Publish(new AnswerSubmittedEvent
                {
                    isCorrect = true,
                    submittedAnswer = _currentQuotient,
                    correctAnswer = _currentQuotient,
                    timeTaken = 0f
                });
            }
            else
            {
                // WRONG - Cookie counts don't match quotient
                int points = _config.pointsPerWrongAnswer;
                _totalScore += points;

                Debug.Log($"✗ WRONG! Correct answer: Each monster should have {_currentQuotient} cookies.");
                Debug.Log($"+{points} points. Total Score: {_totalScore}");

                // Show sad reaction
                ShowAllMonstersReaction(false);

                // Publish wrong answer event
                _eventManager.Publish(new AnswerSubmittedEvent
                {
                    isCorrect = false,
                    submittedAnswer = -1, // No specific answer since manual collection
                    correctAnswer = _currentQuotient,
                    timeTaken = 0f
                });
            }

            // Update score display
            if (_scoreManager != null)
            {
                _scoreManager.SetScore(_totalScore);
            }

            // Wait a bit, then start next question
            Invoke(nameof(StartNewQuestion), 2f);
        }

        /// <summary>
        /// Gets all currently active monsters
        /// </summary>
        private MonsterController[] GetActiveMonsters()
        {
            System.Collections.Generic.List<MonsterController> active = new System.Collections.Generic.List<MonsterController>();

            foreach (var monster in _allMonsters)
            {
                if (monster != null && monster.gameObject.activeInHierarchy)
                {
                    active.Add(monster);
                }
            }

            return active.ToArray();
        }

        /// <summary>
        /// Shows happy or sad reaction on all active monsters
        /// </summary>
        private void ShowAllMonstersReaction(bool isHappy)
        {
            MonsterController[] activeMonsters = GetActiveMonsters();

            foreach (var monster in activeMonsters)
            {
                if (isHappy)
                {
                    monster.ShowHappyReaction();
                }
                else
                {
                    monster.ShowSadReaction();
                }
            }
        }

        /// <summary>
        /// Called when all 3 questions are complete
        /// </summary>
        private void OnGameOver()
        {
            Debug.Log("=================================================");
            Debug.Log($"GAME OVER! Final Score: {_totalScore} / {_config.totalQuestions * _config.pointsPerCorrectAnswer}");
            Debug.Log("=================================================");

            // Calculate accuracy
            float accuracy = (float)_totalScore / (_config.totalQuestions * _config.pointsPerCorrectAnswer);

            // Publish game over event
            _eventManager.Publish(new GameOverEvent
            {
                finalScore = _totalScore,
                accuracy = accuracy
            });

            // TODO: Show game over screen
        }

        public int GetCurrentQuestionNumber() => _currentQuestionNumber;
        public int GetTotalQuestions() => _config.totalQuestions;
        public int GetCurrentScore() => _totalScore;

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
        }
    }
}
