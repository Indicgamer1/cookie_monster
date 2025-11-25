using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CookieGame.Core;
using CookieGame.Data;
using CookieGame.Audio;
using TMPro;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Monster controller with animations and cookie reception
    /// Follows Single Responsibility Principle
    /// Handles visual feedback and state management for monsters
    /// </summary>
    public class MonsterController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _monsterSprite;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _basketTransform;
        [SerializeField] private TextMeshProUGUI _cookieCountText;
        [SerializeField] private TextMeshProUGUI _roundCounterText; // GDD V2.0: Round counter below basket

        [Header("Settings")]
        [SerializeField] private MonsterData _monsterData;
        [SerializeField] private int _monsterIndex;

        private EventManager _eventManager;
        private AudioManager _audioManager;
        private int _cookieCount;
        private int _roundCount; // GDD V2.0: Tracks rounds for this monster
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;

            if (_monsterSprite == null)
                _monsterSprite = GetComponentInChildren<SpriteRenderer>();

            if (_animator == null)
                _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _audioManager = ServiceLocator.Instance.Get<AudioManager>();

            _cookieCount = 0;
            _roundCount = 0;
            UpdateCookieCountDisplay();
            UpdateRoundCounterDisplay();

            // GDD V2.0: Subscribe to new distribution events
            _eventManager?.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _eventManager?.Subscribe<AllMonstersReceivedCookieEvent>(OnDistributionRound);
        }

        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            ResetCookies();
        }

        /// <summary>
        /// GDD V2.0: Called when a distribution round happens (all monsters receive cookies)
        /// </summary>
        private void OnDistributionRound(AllMonstersReceivedCookieEvent eventData)
        {
            // Increment round counter
            _roundCount++;
            _cookieCount++;

            // Update displays
            UpdateCookieCountDisplay();
            UpdateRoundCounterDisplay();

            // Play eating animation with slight delay based on monster index for sequential effect
            float delay = _monsterIndex * 0.15f;
            DOVirtual.DelayedCall(delay, () =>
            {
                PlayEatingAnimation();

                // Play eating sound
                if (_monsterData != null && _monsterData.eatSound != null)
                {
                    _audioManager?.PlaySFX("MonsterEat");
                }
            });

            Debug.Log($"Monster {_monsterIndex}: Round {_roundCount}, Cookies: {_cookieCount}");
        }

        /// <summary>
        /// Manual cookie collection - called when player drags ONE cookie to this monster
        /// </summary>
        public void AddOneCookie()
        {
            _cookieCount++;
            UpdateCookieCountDisplay();
            PlayEatingAnimation();

            // Play eating sound
            if (_monsterData != null && _monsterData.eatSound != null)
            {
                _audioManager?.PlaySFX("MonsterEat");
            }

            Debug.Log($"Monster {_monsterIndex}: Cookie added. Total: {_cookieCount}");
        }

        /// <summary>
        /// Called when a cookie is dropped on this monster
        /// Distributes cookies to all monsters automatically
        /// </summary>
        public void ReceiveCookie()
        {
            // Play eating animation
            PlayEatingAnimation();

            // Add cookie to this monster
            _cookieCount++;
            UpdateCookieCountDisplay();

            // Publish event so other monsters also receive cookies
            _eventManager?.Publish(new CookieDistributedEvent
            {
                monsterIndex = _monsterIndex
            });

            // Distribute to all other monsters
            DistributeToAllMonsters();

            // Play eating sound
            if (_monsterData != null && _monsterData.eatSound != null)
            {
                _audioManager?.PlaySFX("MonsterEat");
            }
        }

        private void DistributeToAllMonsters()
        {
            // Find all monsters and give them a cookie
            MonsterController[] allMonsters = FindObjectsOfType<MonsterController>();

            foreach (var monster in allMonsters)
            {
                if (monster != this && monster._cookieCount == _cookieCount - 1)
                {
                    monster.ReceiveCookieFromDistribution();
                }
            }
        }

        /// <summary>
        /// Called when receiving a cookie from automatic distribution
        /// </summary>
        public void ReceiveCookieFromDistribution()
        {
            _cookieCount++;
            UpdateCookieCountDisplay();
            PlayEatingAnimation();
        }

        /// <summary>
        /// Plays the eating animation
        /// </summary>
        private void PlayEatingAnimation()
        {
            // Scale animation for eating
            transform.DOScale(_originalScale * 1.1f, 0.2f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    transform.DOScale(_originalScale, 0.2f);
                });

            // Trigger animator if available
            if (_animator != null)
            {
                _animator.SetTrigger("Eat");
            }

            // Update sprite if using sprite-based system
            if (_monsterSprite != null && _monsterData != null && _monsterData.eatingSprite != null)
            {
                _monsterSprite.sprite = _monsterData.eatingSprite;

                // Return to idle after animation
                DOVirtual.DelayedCall(0.8f, () =>
                {
                    if (_monsterSprite != null && _monsterData != null)
                    {
                        _monsterSprite.sprite = _monsterData.idleSprite;
                    }
                });
            }
        }

        /// <summary>
        /// Shows happy reaction for correct answer
        /// </summary>
        public void ShowHappyReaction()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Happy");
            }

            if (_monsterSprite != null && _monsterData != null && _monsterData.happySprite != null)
            {
                _monsterSprite.sprite = _monsterData.happySprite;

                DOVirtual.DelayedCall(1f, () =>
                {
                    if (_monsterSprite != null && _monsterData != null)
                    {
                        _monsterSprite.sprite = _monsterData.idleSprite;
                    }
                });
            }

            // Jump animation
            transform.DOJump(transform.position, 0.5f, 1, 0.5f);

            // Play happy sound
            if (_monsterData != null && _monsterData.happySound != null)
            {
                _audioManager?.PlaySFX("MonsterHappy");
            }

            // Show particles
            if (_monsterData != null && _monsterData.happyParticles != null)
            {
                Instantiate(_monsterData.happyParticles, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Shows sad reaction for wrong answer
        /// </summary>
        public void ShowSadReaction()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Sad");
            }

            if (_monsterSprite != null && _monsterData != null && _monsterData.sadSprite != null)
            {
                _monsterSprite.sprite = _monsterData.sadSprite;

                DOVirtual.DelayedCall(1f, () =>
                {
                    if (_monsterSprite != null && _monsterData != null)
                    {
                        _monsterSprite.sprite = _monsterData.idleSprite;
                    }
                });
            }

            // Shake animation
            transform.DOShakePosition(0.5f, 0.2f, 10);

            // Play sad sound
            if (_monsterData != null && _monsterData.sadSound != null)
            {
                _audioManager?.PlaySFX("MonsterSad");
            }

            // Show particles
            if (_monsterData != null && _monsterData.sadParticles != null)
            {
                Instantiate(_monsterData.sadParticles, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Updates the cookie count display
        /// </summary>
        private void UpdateCookieCountDisplay()
        {
            if (_cookieCountText != null)
            {
                _cookieCountText.text = _cookieCount.ToString();
            }
        }

        /// <summary>
        /// GDD V2.0: Updates the round counter display below basket
        /// </summary>
        private void UpdateRoundCounterDisplay()
        {
            if (_roundCounterText != null)
            {
                _roundCounterText.text = $"Round: {_roundCount}";
            }
        }

        /// <summary>
        /// Resets cookie count for new question
        /// </summary>
        public void ResetCookies()
        {
            _cookieCount = 0;
            _roundCount = 0;
            UpdateCookieCountDisplay();
            UpdateRoundCounterDisplay();

            // Reset to idle sprite
            if (_monsterSprite != null && _monsterData != null && _monsterData.idleSprite != null)
            {
                _monsterSprite.sprite = _monsterData.idleSprite;
            }
        }

        public int GetCookieCount() => _cookieCount;
        public int GetRoundCount() => _roundCount; // GDD V2.0: Getter for round count
        public void SetMonsterIndex(int index) => _monsterIndex = index;
        public int GetMonsterId() => _monsterIndex; // GDD V2.0: ID for event tracking
        public Transform GetBasketTransform() => _basketTransform; // Get basket position for cookie animations

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _eventManager?.Unsubscribe<AllMonstersReceivedCookieEvent>(OnDistributionRound);
        }
    }
}
