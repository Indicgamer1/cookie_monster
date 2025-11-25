using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using CookieGame.Core;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Cookie game object with drag-and-drop functionality
    /// Implements Interface Segregation Principle with specific drag interfaces
    /// Uses DOTween for smooth animations
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Cookie : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        [Header("Visual Feedback")]
        [SerializeField] private float _dragScale = 1.2f;
        [SerializeField] private float _dropDuration = 0.5f;

        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        private Transform _originalParent;
        private int _originalSortingOrder;
        private bool _isDragging;
        private Camera _mainCamera;
        private Canvas _canvas;
        private EventManager _eventManager;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_collider == null)
                _collider = GetComponent<Collider2D>();

            _mainCamera = Camera.main;
            _originalScale = transform.localScale;
        }

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _originalPosition = transform.position;
            _originalParent = transform.parent;
            _originalSortingOrder = _spriteRenderer.sortingOrder;

            // Bring cookie to front during drag
            _spriteRenderer.sortingOrder = 100;

            // Scale up for visual feedback
            transform.DOScale(_originalScale * _dragScale, 0.2f).SetEase(Ease.OutBack);

            Debug.Log("Cookie drag started");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            // Convert screen position to world position
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
            worldPosition.z = 0f;
            transform.position = worldPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;

            // Check if dropped on a monster
            MonsterController targetMonster = GetMonsterUnderCursor(eventData);

            if (targetMonster != null)
            {
                // GDD V2.0: Valid drop - trigger FULL DISTRIBUTION ROUND to ALL monsters
                TriggerDistributionRound(targetMonster);
            }
            else
            {
                // Invalid drop - return to original position
                ReturnToOriginalPosition();
            }
        }

        private MonsterController GetMonsterUnderCursor(PointerEventData eventData)
        {
            // Raycast to find monster
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = eventData.position
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                MonsterController monster = result.gameObject.GetComponent<MonsterController>();
                if (monster != null && monster != null)
                {
                    return monster;
                }
            }

            // Also try Physics2D raycast for 2D sprites
            RaycastHit2D hit = Physics2D.Raycast(_mainCamera.ScreenToWorldPoint(eventData.position), Vector2.zero);
            if (hit.collider != null)
            {
                return hit.collider.GetComponent<MonsterController>();
            }

            return null;
        }

        /// <summary>
        /// GDD V2.0: Triggers a full distribution round when cookie is dropped on ANY monster
        /// ONE cookie drop = ONE round to ALL monsters
        /// </summary>
        private void TriggerDistributionRound(MonsterController targetMonster)
        {
            // Publish event that cookie was dropped on a monster
            // DistributionManager will handle the actual distribution to ALL monsters
            _eventManager?.Publish(new CookieDroppedOnMonsterEvent
            {
                monsterId = targetMonster.GetMonsterId()
            });

            // Animate cookie back to pile immediately (visual feedback only)
            ReturnToOriginalPosition();

            Debug.Log($"Cookie dropped on {targetMonster.name} - Distribution round triggered");
        }

        /// <summary>
        /// Legacy method - kept for compatibility but not used in GDD V2.0
        /// </summary>
        private void DistributeCookieToMonster(MonsterController monster)
        {
            // Animate cookie to monster
            transform.DOMove(monster.transform.position, _dropDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    monster.ReceiveCookie();
                    ReturnToPool();
                });

            // Restore scale
            transform.DOScale(_originalScale, 0.2f);
            _spriteRenderer.sortingOrder = _originalSortingOrder;

            Debug.Log($"Cookie distributed to {monster.name}");
        }

        private void ReturnToOriginalPosition()
        {
            // Animate back to original position
            transform.DOMove(_originalPosition, _dropDuration)
                .SetEase(Ease.OutBack);

            transform.DOScale(_originalScale, 0.2f);
            _spriteRenderer.sortingOrder = _originalSortingOrder;

            Debug.Log("Cookie returned to original position");
        }

        private void ReturnToPool()
        {
            // Return cookie to object pool
            var poolManager = ServiceLocator.Instance.Get<CookieGame.Patterns.PoolManager>();
            poolManager?.Return("Cookie", this);
        }

        /// <summary>
        /// Resets cookie state when retrieved from pool
        /// </summary>
        public void ResetState()
        {
            transform.localScale = _originalScale;
            _isDragging = false;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = 1;
            }
        }
    }
}
