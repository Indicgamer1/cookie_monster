using UnityEngine;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.Data;
using Eduzo.Games.CookieMonster.Patterns;

namespace Eduzo.Games.CookieMonster.VFX
{
    /// <summary>
    /// VFX Manager - handles all particle effects and visual feedback
    /// Uses object pooling for performance
    /// Follows Single Responsibility Principle
    /// </summary>
    public class CookieMonsterVFXManager : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private CookieMonsterVFXDatabase _vfxDatabase;

        private CookieMonsterEventManager _cookieMonsterEventManager;
        private CookieMonsterPoolManager _poolManager;

        private void Start()
        {
            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();
            _poolManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterPoolManager>();

            // Subscribe to VFX request events
            _cookieMonsterEventManager?.Subscribe<VFXRequestEvent>(OnVFXRequest);

            // Initialize VFX database
            if (_vfxDatabase != null)
            {
                _vfxDatabase.Initialize();
                CreateVFXPools();
            }
        }

        private void CreateVFXPools()
        {
            foreach (var vfxEntry in _vfxDatabase.particleEffects)
            {
                if (vfxEntry.prefab != null)
                {
                    // Try to get or add the required component
                    var prefabComponent = vfxEntry.prefab.GetComponent<CookieMonsterVFXInstance>();
                    if (prefabComponent == null)
                    {
                        // If the prefab doesn't have the component, add it temporarily for pooling
                        prefabComponent = vfxEntry.prefab.AddComponent<CookieMonsterVFXInstance>();
                    }

                    _poolManager?.CreatePool(vfxEntry.key, prefabComponent, 5);
                }
            }

            Debug.Log("VFX pools created");
        }

        private void OnVFXRequest(VFXRequestEvent eventData)
        {
            PlayVFX(eventData.vfxName, eventData.position);
        }

        /// <summary>
        /// Plays a VFX at the specified position
        /// </summary>
        public void PlayVFX(string vfxName, Vector3 position, Transform parent = null)
        {
            if (_vfxDatabase == null)
            {
                Debug.LogWarning("VFX Database not assigned!");
                return;
            }

            var vfxEntry = _vfxDatabase.GetVFX(vfxName);
            if (vfxEntry != null && vfxEntry.prefab != null)
            {
                CookieMonsterVFXInstance cookieMonsterVFX = _poolManager.Get<CookieMonsterVFXInstance>(vfxEntry.key);
                if (cookieMonsterVFX != null)
                {
                    cookieMonsterVFX.transform.position = position;
                    cookieMonsterVFX.transform.SetParent(parent);
                    cookieMonsterVFX.Play(vfxEntry.duration, vfxEntry.autoDestroy);

                    Debug.Log($"Playing VFX: {vfxName} at {position}");
                }
            }
            else
            {
                Debug.LogWarning($"VFX not found: {vfxName}");
            }
        }

        /// <summary>
        /// Plays a VFX and returns the instance for further control
        /// </summary>
        public CookieMonsterVFXInstance PlayVFXWithReturn(string vfxName, Vector3 position, Transform parent = null)
        {
            if (_vfxDatabase == null) return null;

            var vfxEntry = _vfxDatabase.GetVFX(vfxName);
            if (vfxEntry != null && vfxEntry.prefab != null)
            {
                CookieMonsterVFXInstance cookieMonsterVFX = _poolManager.Get<CookieMonsterVFXInstance>(vfxEntry.key);
                if (cookieMonsterVFX != null)
                {
                    cookieMonsterVFX.transform.position = position;
                    cookieMonsterVFX.transform.SetParent(parent);
                    cookieMonsterVFX.Play(vfxEntry.duration, vfxEntry.autoDestroy);
                    return cookieMonsterVFX;
                }
            }

            return null;
        }

        /// <summary>
        /// Stops and returns VFX to pool
        /// </summary>
        public void StopVFX(CookieMonsterVFXInstance cookieMonsterVFX, string poolKey)
        {
            if (cookieMonsterVFX != null)
            {
                cookieMonsterVFX.Stop();
                _poolManager?.Return(poolKey, cookieMonsterVFX);
            }
        }

        private void OnDestroy()
        {
            _cookieMonsterEventManager?.Unsubscribe<VFXRequestEvent>(OnVFXRequest);
        }
    }
}
