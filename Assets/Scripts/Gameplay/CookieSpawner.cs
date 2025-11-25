using UnityEngine;
using System.Collections.Generic;
using CookieGame.Core;
using CookieGame.Patterns;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Spawns cookies at designated positions
    /// Uses Object Pool pattern for performance
    /// GDD V2.0: Supports both sprite-based and UI-based cookies
    /// </summary>
    public class CookieSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _cookiePrefab; // Can be Cookie or Cookie_Sprite
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private SpawnAreaBounds _spawnAreaBounds; // Defines spawn area (no collider)

        [Header("Spawn Settings")]
        [SerializeField] private int _cookiesPerRow = 5;
        [SerializeField] private float _cookieSpacing = 1.2f; // Increased from 0.5f to prevent overlap
        [SerializeField] private bool _useObjectPooling = true;
        [SerializeField] private bool _randomizePositions = false; // Optional: add slight randomness

        [Header("Visual Settings")]
        [SerializeField] private int _baseSortingOrder = 1;

        private PoolManager _poolManager;
        private EventManager _eventManager;
        private List<GameObject> _spawnedCookies = new List<GameObject>();

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();

            // Setup object pooling if enabled
            if (_useObjectPooling)
            {
                _poolManager = ServiceLocator.Instance.Get<PoolManager>();
                if (_cookiePrefab != null && _poolManager != null)
                {
                    // Get the Cookie or Cookie_Sprite component from prefab
                    Cookie_Sprite cookieSprite = _cookiePrefab.GetComponent<Cookie_Sprite>();
                    Cookie cookieUI = _cookiePrefab.GetComponent<Cookie>();

                    if (cookieSprite != null)
                    {
                        _poolManager.CreatePool("Cookie", cookieSprite, 20);
                    }
                    else if (cookieUI != null)
                    {
                        _poolManager.CreatePool("Cookie", cookieUI, 20);
                    }
                    else
                    {
                        Debug.LogError("CookieSpawner: Cookie prefab must have Cookie or Cookie_Sprite component!");
                        _useObjectPooling = false;
                    }
                }
            }

            // Validate spawn area bounds
            if (_spawnAreaBounds == null)
            {
                Debug.LogError("CookieSpawner: SpawnAreaBounds not assigned! Add SpawnAreaBounds component and assign it.");
            }

            // Subscribe to question generated event
            _eventManager?.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
        }

        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            // Clear existing cookies
            ClearCookies();

            // Spawn new cookies based on dividend
            SpawnCookies(eventData.dividend);
        }

        /// <summary>
        /// Spawns the specified number of cookies in a grid pattern
        /// </summary>
        public void SpawnCookies(int count)
        {
            if (_cookiePrefab == null)
            {
                Debug.LogError("CookieSpawner: Cookie prefab not assigned!");
                return;
            }

            if (_spawnAreaBounds == null)
            {
                Debug.LogError("CookieSpawner: SpawnAreaBounds not assigned!");
                return;
            }

            // Calculate grid positions within spawn area
            List<Vector3> positions = CalculateGridPositions(count);

            // Spawn cookies
            for (int i = 0; i < count && i < positions.Count; i++)
            {
                SpawnCookieAt(positions[i], i);
            }

            Debug.Log($"CookieSpawner: Spawned {_spawnedCookies.Count}/{count} cookies");
        }

        /// <summary>
        /// Calculates grid positions within the spawn area
        /// </summary>
        private List<Vector3> CalculateGridPositions(int count)
        {
            List<Vector3> positions = new List<Vector3>();

            // Get spawn area bounds from either BoxCollider2D or SpawnAreaBounds
            Bounds bounds = GetSpawnBounds();
            Vector3 areaCenter = bounds.center;

            // Calculate grid dimensions
            int columns = Mathf.Min(count, _cookiesPerRow);
            int rows = Mathf.CeilToInt((float)count / columns);

            // Calculate total grid size
            float totalWidth = (columns - 1) * _cookieSpacing;
            float totalHeight = (rows - 1) * _cookieSpacing;

            // Starting position (top-left of grid, centered in spawn area)
            Vector3 startPos = new Vector3(
                areaCenter.x - totalWidth * 0.5f,
                areaCenter.y + totalHeight * 0.5f,
                0f
            );

            // Generate grid positions
            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int col = i % columns;

                Vector3 position = new Vector3(
                    startPos.x + col * _cookieSpacing,
                    startPos.y - row * _cookieSpacing,
                    0f
                );

                // Optional: Add slight randomness to prevent perfect grid overlap
                if (_randomizePositions)
                {
                    position.x += Random.Range(-0.1f, 0.1f);
                    position.y += Random.Range(-0.1f, 0.1f);
                }

                // Clamp to spawn area bounds
                position.x = Mathf.Clamp(position.x, bounds.min.x + 0.25f, bounds.max.x - 0.25f);
                position.y = Mathf.Clamp(position.y, bounds.min.y + 0.25f, bounds.max.y - 0.25f);

                positions.Add(position);
            }

            return positions;
        }

        /// <summary>
        /// Spawns a single cookie at the given position
        /// </summary>
        private void SpawnCookieAt(Vector3 position, int index)
        {
            GameObject cookieObj = null;

            // Use object pooling if enabled
            if (_useObjectPooling && _poolManager != null)
            {
                // Try to get Cookie_Sprite from pool first
                Cookie_Sprite cookie_Sprite = _poolManager.Get<Cookie_Sprite>("Cookie");
                if (cookie_Sprite != null)
                {
                    cookieObj = cookie_Sprite.gameObject;
                }
                else
                {
                    // Try Cookie (UI-based) from pool
                    Cookie cookieUI = _poolManager.Get<Cookie>("Cookie");
                    if (cookieUI != null)
                    {
                        cookieObj = cookieUI.gameObject;
                    }
                }
            }
            else
            {
                // Instantiate new cookie
                cookieObj = Instantiate(_cookiePrefab, position, Quaternion.identity, _spawnParent);
            }

            if (cookieObj == null) return;

            cookieObj.name = $"Cookie_{index}";
            cookieObj.transform.position = position;

            // Setup sprite-based cookie
            Cookie_Sprite cookieSprite = cookieObj.GetComponent<Cookie_Sprite>();
            if (cookieSprite != null)
            {
                cookieSprite.SetSpawnPosition(position);
            }
            else
            {
                // Try UI-based cookie
                Cookie cookieUI = cookieObj.GetComponent<Cookie>();
                if (cookieUI != null)
                {
                    cookieUI.ResetState();
                }
            }

            // Set sorting order for sprite renderer
            // IMPORTANT: Use reverse order so bottom cookies don't block top cookies
            SpriteRenderer spriteRenderer = cookieObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Lower index = higher sorting order (appears on top)
                spriteRenderer.sortingOrder = _baseSortingOrder;
            }

            // Ensure collider is enabled for clicking
            Collider2D collider = cookieObj.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            _spawnedCookies.Add(cookieObj);
        }

        /// <summary>
        /// Clears all spawned cookies
        /// </summary>
        private void ClearCookies()
        {
            if (_useObjectPooling && _poolManager != null)
            {
                // Return to pool - try both component types
                foreach (GameObject cookieObj in _spawnedCookies)
                {
                    if (cookieObj == null) continue;

                    Cookie_Sprite cookieSprite = cookieObj.GetComponent<Cookie_Sprite>();
                    if (cookieSprite != null)
                    {
                        _poolManager.Return("Cookie", cookieSprite);
                    }
                    else
                    {
                        Cookie cookieUI = cookieObj.GetComponent<Cookie>();
                        if (cookieUI != null)
                        {
                            _poolManager.Return("Cookie", cookieUI);
                        }
                    }
                }
            }
            else
            {
                // Destroy instantiated cookies
                foreach (GameObject cookie in _spawnedCookies)
                {
                    if (cookie != null)
                    {
                        Destroy(cookie);
                    }
                }
            }

            _spawnedCookies.Clear();
        }

        /// <summary>
        /// Gets spawn area bounds from SpawnAreaBounds component
        /// </summary>
        private Bounds GetSpawnBounds()
        {
            if (_spawnAreaBounds != null)
            {
                return _spawnAreaBounds.GetBounds();
            }

            // Default bounds if not assigned
            Debug.LogWarning("CookieSpawner: No spawn area defined! Using default bounds.");
            return new Bounds(transform.position, new Vector3(7f, 4f, 0f));
        }

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            ClearCookies();
        }

        // OnDrawGizmos removed - SpawnAreaBounds component draws its own gizmo
    }
}
