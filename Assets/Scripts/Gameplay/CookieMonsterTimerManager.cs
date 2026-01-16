using UnityEngine;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.Data;
using Eduzo.Games.CookieMonster.GameStates;

namespace Eduzo.Games.CookieMonster.Gameplay
{
    /// <summary>
    /// Manages game timer
    /// Follows Single Responsibility Principle
    /// </summary>
    public class CookieMonsterTimerManager : MonoBehaviour
    {
        private CookieMonsterGameConfig _config;
        private CookieMonsterEventManager _cookieMonsterEventManager;

        private float _remainingTime;
        private bool _isPracticeMode;
        private bool _isTimerActive;

        public void Initialize(bool isPracticeMode)
        {
            _isPracticeMode = isPracticeMode;

            _config = Resources.Load<CookieMonsterGameConfig>("GameConfig");
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<CookieMonsterGameConfig>();
            }

            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();

            _remainingTime = _config.GetTimeLimit(isPracticeMode);
            _isTimerActive = true;

            // Check for unlimited time in practice mode
            if (_isPracticeMode && _config.practiceModeTimeLimit == 0f)
            {
                _isTimerActive = false;
            }
        }

        private void Update()
        {
            if (!_isTimerActive) return;

            _remainingTime -= Time.deltaTime;

            if (_remainingTime < 0f)
            {
                _remainingTime = 0f;
            }

            // Publish timer update
            _cookieMonsterEventManager.Publish(new TimerUpdatedEvent
            {
                remainingTime = _remainingTime
            });

            // Check if time expired
            if (_remainingTime == 0f)
            {
                _isTimerActive = false;
                _cookieMonsterEventManager.Publish(new TimerExpiredEvent());
                Debug.Log("Timer expired - Game Over!");
            }
        }

        /// <summary>
        /// Pauses the timer
        /// </summary>
        public void PauseTimer()
        {
            _isTimerActive = false;
        }

        /// <summary>
        /// Resumes the timer
        /// </summary>
        public void ResumeTimer()
        {
            if (_isPracticeMode && _config.practiceModeTimeLimit == 0f)
            {
                return; // Don't resume if unlimited time
            }

            _isTimerActive = true;
        }

        /// <summary>
        /// Adds time bonus (for powerups or achievements)
        /// </summary>
        public void AddTime(float seconds)
        {
            _remainingTime += seconds;
        }

        /// <summary>
        /// Checks if there's time remaining
        /// </summary>
        public bool HasTime()
        {
            if (!_isTimerActive)
            {
                return true; // Unlimited time
            }

            return _remainingTime > 0f;
        }

        public float GetRemainingTime() => _remainingTime;
        public bool IsTimerActive() => _isTimerActive;
    }
}
