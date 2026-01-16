using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Eduzo.Games.CookieMonster.Core;

namespace Eduzo.Games.CookieMonster.UI
{
    /// <summary>
    /// Heads-Up Display for gameplay
    /// GDD V2.0: Shows score, lives, timer, question, cookie pile, and multiplier
    /// </summary>
    public class CookieMonsterGameHUD : MonoBehaviour
    {
        [Header("Question Display")]
        [SerializeField] private TextMeshProUGUI _questionTextTMP;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI _scoreTextTMP;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Image[] _lifeIcons;

        [Header("GDD V2.0: Cookie Pile Display")]
        [SerializeField] private TextMeshProUGUI _cookiePileText;   
        [SerializeField] private TextMeshProUGUI _remainingCookiesText;
      

        [Header("GDD V2.0: Score Multiplier Display")]
        [SerializeField] private TextMeshProUGUI _multiplierText;
        [SerializeField] private GameObject _multiplierPanel;

        [Header("Progress")]
        [SerializeField] private Slider _timerSlider;

        private CookieMonsterEventManager _cookieMonsterEventManager;
        private float _totalTime;
        private int _totalCookies;
        private float _currentMultiplier = 1.0f;

        private void Start()
        {
            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();

            // Subscribe to events
            _cookieMonsterEventManager.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _cookieMonsterEventManager.Subscribe<ScoreUpdatedEvent>(OnScoreUpdated);
            _cookieMonsterEventManager.Subscribe<LivesUpdatedEvent>(OnLivesUpdated);
            _cookieMonsterEventManager.Subscribe<TimerUpdatedEvent>(OnTimerUpdated);

            // GDD V2.0: Subscribe to new events
            _cookieMonsterEventManager.Subscribe<CookiePileUpdatedEvent>(OnCookiePileUpdated);
            _cookieMonsterEventManager.Subscribe<RoundScoreAddedEvent>(OnRoundScoreAdded);

            InitializeHUD();
        }

        private void InitializeHUD()
        {
            UpdateScore(0);
            UpdateTimer(0f);
            HideMultiplier();
        }

        public void ResetHUD()
        {
            UpdateScore(0);
            UpdateTimer(0f);
        }

        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            UpdateQuestion(eventData.dividend, eventData.divisor);

            // GDD V2.0: Initialize cookie pile tracking
            _totalCookies = eventData.dividend;
            UpdateCookiePile(eventData.dividend, eventData.dividend);
        }

        private void OnScoreUpdated(ScoreUpdatedEvent eventData)
        {
            UpdateScore(eventData.newScore);

            // GDD V2.0: Update multiplier display
            if (eventData.multiplier > 1.0f)
            {
                ShowMultiplier(eventData.multiplier);
            }
            else
            {
                HideMultiplier();
            }
        }

        private void OnLivesUpdated(LivesUpdatedEvent eventData)
        {
            UpdateLives(eventData.remainingLives);
        }

        private void OnTimerUpdated(TimerUpdatedEvent eventData)
        {
            UpdateTimer(eventData.remainingTime);
        }

        /// <summary>
        /// GDD V2.0: Called when cookie pile is updated during distribution
        /// </summary>
        private void OnCookiePileUpdated(CookiePileUpdatedEvent eventData)
        {
            UpdateCookiePile(eventData.remainingCookies, eventData.totalCookies);
        }

        /// <summary>
        /// GDD V2.0: Called when round score is added (100 points per round)
        /// </summary>
        private void OnRoundScoreAdded(RoundScoreAddedEvent eventData)
        {
            // Can add visual feedback here (e.g., score pop-up animation)
            Debug.Log($"Round Score Added: +{eventData.roundScore}");
        }

        private void UpdateQuestion(int dividend, int divisor)
        {

            if (_questionTextTMP != null)
            {
                _questionTextTMP.text = $"{dividend} ÷ {divisor}";
            }
        }

        private void UpdateScore(int score)
        {
            if (_scoreTextTMP != null)
            {
                _scoreTextTMP.text = $"Score: {score}";
            }
        }

        /// <summary>
        /// GDD V2.0: Updates cookie pile display showing remaining cookies
        /// </summary>
        private void UpdateCookiePile(int remainingCookies, int totalCookies)
        {
            if (_cookiePileText != null)
            {
                _cookiePileText.text = $"Cookie Pile: {remainingCookies}/{totalCookies}";
            }

            if (_remainingCookiesText != null)
            {
                _remainingCookiesText.text = $"{remainingCookies}";
            }
        }

        /// <summary>
        /// GDD V2.0: Shows score multiplier badge
        /// </summary>
        private void ShowMultiplier(float multiplier)
        {
            _currentMultiplier = multiplier;

            if (_multiplierText != null)
            {
                _multiplierText.text = $"{multiplier:F1}x";
                _multiplierText.color = GetMultiplierColor(multiplier);
            }

            if (_multiplierPanel != null)
            {
                _multiplierPanel.SetActive(true);
            }
        }

        /// <summary>
        /// GDD V2.0: Hides multiplier display
        /// </summary>
        private void HideMultiplier()
        {
            if (_multiplierPanel != null)
            {
                _multiplierPanel.SetActive(false);
            }
        }

        /// <summary>
        /// GDD V2.0: Returns color based on multiplier value
        /// </summary>
        private Color GetMultiplierColor(float multiplier)
        {
            if (multiplier >= 2.0f)
                return new Color(1f, 0.84f, 0f); // Gold for 2x
            else if (multiplier >= 1.5f)
                return new Color(0.75f, 0.75f, 0.75f); // Silver for 1.5x
            else
                return Color.white;
        }

        private void UpdateTimer(float remainingTime)
        {
            if (_timerText != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                _timerText.text = $"{minutes:00}:{seconds:00}";
            }

            if (_timerSlider != null)
            {
                if (_totalTime == 0f)
                {
                    _totalTime = remainingTime;
                }
                _timerSlider.value = remainingTime / _totalTime;
            }
        }

        private void UpdateLives(int lives)
        {
            if (_lifeIcons == null || _lifeIcons.Length == 0)
                return;

            for (int i = 0; i < _lifeIcons.Length; i++)
            {
                if (_lifeIcons[i] != null)
                {
                    _lifeIcons[i].enabled = i < lives;
                }
            }
        }

        private void OnDestroy()
        {
            if (_cookieMonsterEventManager != null)
            {
                _cookieMonsterEventManager.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
                _cookieMonsterEventManager.Unsubscribe<ScoreUpdatedEvent>(OnScoreUpdated);
                _cookieMonsterEventManager.Unsubscribe<LivesUpdatedEvent>(OnLivesUpdated);
                _cookieMonsterEventManager.Unsubscribe<TimerUpdatedEvent>(OnTimerUpdated);

                // GDD V2.0: Unsubscribe from new events
                _cookieMonsterEventManager.Unsubscribe<CookiePileUpdatedEvent>(OnCookiePileUpdated);
                _cookieMonsterEventManager.Unsubscribe<RoundScoreAddedEvent>(OnRoundScoreAdded);
            }
        }
    }
}
