using UnityEngine;
using System.Collections.Generic;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.Patterns;

namespace Eduzo.Games.CookieMonster.Gameplay
{
    /// <summary>
    /// Spawns cookies one at a time at the spawner location.
    /// Starts with one cookie. When a cookie is grabbed by a monster,
    /// spawns a new cookie at the same location unlimited times.
    /// </summary>
    public class CookieMonsterSingleCookieSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _cookiePrefab; // Can be Cookie or Cookie_Sprite
        [SerializeField] private Transform _spawnParent;

        [Header("Settings")]
        [SerializeField] private bool _useObjectPooling = true;

        [Header("Visual Settings")]
        [SerializeField] private int _baseSortingOrder = 1;

        private CookieMonsterPoolManager _poolManager;
        private CookieMonsterEventManager _cookieMonsterEventManager;
        private List<GameObject> _spawnedCookies = new List<GameObject>();

        // Cookie spawning tracking
        private int _maxCookies = 0; // Maximum cookies to spawn (from dividend)
        private int _cookiesSpawned = 0; // How many cookies have been spawned so far

        private void Start()
        {
            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();

            // Setup object pooling if enabled
            if (_useObjectPooling)
            {
                _poolManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterPoolManager>();
                if (_cookiePrefab != null && _poolManager != null)
                {
                    // Get the Cookie or Cookie_Sprite component from prefab
                    CookieMonsterCookieSprite cookieMonsterCookieSprite = _cookiePrefab.GetComponent<CookieMonsterCookieSprite>();

                    if (cookieMonsterCookieSprite != null)
                    {
                        _poolManager.CreatePool("xCookie", cookieMonsterCookieSprite, 20);
                    }
                    else
                    {
                        Debug.LogError("SingleCookieSpawner: Cookie prefab must have Cookie or Cookie_Sprite component!");
                        _useObjectPooling = false;
                    }
                }
            }

            // Subscribe to events
            _cookieMonsterEventManager?.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _cookieMonsterEventManager?.Subscribe<CookieDroppedOnMonsterEvent>(OnCookieGrabbed);
        }

        /// <summary>
        /// Called when a new question is generated
        /// </summary>
        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            // Clear existing cookies
            ClearCookies();

            // Initialize cookie spawning tracking
            _maxCookies = eventData.dividend;
            _cookiesSpawned = 0;

            // Spawn the first cookie at the spawner location
            SpawnSingleCookie();

            Debug.Log($"SingleCookieSpawner: New question - dividend: {_maxCookies}, spawned first cookie");
        }

        /// <summary>
        /// Called when a cookie is grabbed by a monster
        /// Spawns a new cookie unlimited times
        /// </summary>
        private void OnCookieGrabbed(CookieDroppedOnMonsterEvent eventData)
        {
            // Always spawn a new cookie when one is grabbed (unlimited spawning)
            SpawnSingleCookie();

            Debug.Log($"SingleCookieSpawner: Cookie Grabbed! Spawned cookie {_cookiesSpawned}/{_maxCookies} (unlimited mode)");
        }

        /// <summary>
        /// Spawns a single cookie at the spawner's transform position
        /// </summary>
        private void SpawnSingleCookie()
        {
            if (_cookiePrefab == null)
            {
                Debug.LogError("SingleCookieSpawner: Cookie prefab not assigned!");
                return;
            }

            // Use spawner's transform position as the spawn location
            Vector3 spawnPosition = _spawnParent.position;

            // Spawn the cookie at this position
            GameObject cookieObj = null;

            // Use object pooling if enabled
            if (_useObjectPooling && _poolManager != null)
            {
                // Try to get Cookie_Sprite from pool first
                CookieMonsterCookieSprite cookieSprite = _poolManager.Get<CookieMonsterCookieSprite>("xCookie");
                if (cookieSprite != null)
                {
                    cookieObj = cookieSprite.gameObject;
                }
            }
            else
            {
                // Instantiate new cookie
                cookieObj = Instantiate(_cookiePrefab, spawnPosition, Quaternion.identity, _spawnParent);
            }

            if (cookieObj == null)
            {
                Debug.LogError("SingleCookieSpawner: Failed to create cookie object!");
                return;
            }

            cookieObj.name = $"Cookie_{_cookiesSpawned}";
            cookieObj.transform.position = spawnPosition;

            // Reset scale to default
            cookieObj.transform.localScale = Vector3.one;

            // Setup sprite-based cookie
            CookieMonsterCookieSprite cookieMonsterCookieSprite = cookieObj.GetComponent<CookieMonsterCookieSprite>();
            if (cookieMonsterCookieSprite != null)
            {
                cookieMonsterCookieSprite.SetSpawnPosition(spawnPosition);
            }

            // Set sorting order for sprite renderer
            SpriteRenderer spriteRenderer = cookieObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = _baseSortingOrder;
            }

            // Ensure collider is enabled for clicking
            Collider2D collider = cookieObj.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            _spawnedCookies.Add(cookieObj);
            _cookiesSpawned++;

            Debug.Log($"SingleCookieSpawner: Spawned cookie {_cookiesSpawned}/{_maxCookies} at position {spawnPosition}");
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

                    CookieMonsterCookieSprite cookieMonsterCookieSprite = cookieObj.GetComponent<CookieMonsterCookieSprite>();
                    if (cookieMonsterCookieSprite != null)
                    {
                        _poolManager.Return("xCookie", cookieMonsterCookieSprite);
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

        private void OnDestroy()
        {
            _cookieMonsterEventManager?.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _cookieMonsterEventManager?.Unsubscribe<CookieDroppedOnMonsterEvent>(OnCookieGrabbed);
            ClearCookies();
        }
    }
}
