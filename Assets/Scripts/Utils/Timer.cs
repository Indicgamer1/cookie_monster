using UnityEngine;
using System;

namespace CookieGame.Utils
{
    /// <summary>
    /// Simple timer utility class
    /// Provides countdown and stopwatch functionality
    /// </summary>
    public class Timer
    {
        private float _duration;
        private float _currentTime;
        private bool _isRunning;
        private bool _isCountdown;

        public event Action OnTimerComplete;
        public event Action<float> OnTimerTick;

        /// <summary>
        /// Creates a countdown timer
        /// </summary>
        public static Timer CreateCountdown(float duration)
        {
            return new Timer(duration, true);
        }

        /// <summary>
        /// Creates a stopwatch (counts up)
        /// </summary>
        public static Timer CreateStopwatch()
        {
            return new Timer(0f, false);
        }

        private Timer(float duration, bool isCountdown)
        {
            _duration = duration;
            _isCountdown = isCountdown;
            _currentTime = isCountdown ? duration : 0f;
            _isRunning = false;
        }

        /// <summary>
        /// Updates the timer
        /// Call this in Update() or FixedUpdate()
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!_isRunning) return;

            if (_isCountdown)
            {
                _currentTime -= deltaTime;

                if (_currentTime <= 0f)
                {
                    _currentTime = 0f;
                    _isRunning = false;
                    OnTimerComplete?.Invoke();
                }
            }
            else
            {
                _currentTime += deltaTime;
            }

            OnTimerTick?.Invoke(_currentTime);
        }

        /// <summary>
        /// Starts or resumes the timer
        /// </summary>
        public void Start()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Pauses the timer
        /// </summary>
        public void Pause()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Stops and resets the timer
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _currentTime = _isCountdown ? _duration : 0f;
        }

        /// <summary>
        /// Resets the timer to initial value
        /// </summary>
        public void Reset()
        {
            _currentTime = _isCountdown ? _duration : 0f;
        }

        public float CurrentTime => _currentTime;
        public float Duration => _duration;
        public bool IsRunning => _isRunning;
        public bool IsComplete => _isCountdown && _currentTime <= 0f;
        public float Progress => _isCountdown ? (_duration - _currentTime) / _duration : 0f;
    }
}
