using UnityEngine;
using UnityEngine.UI;
using Eduzo.Games.CookieMonster.Core;

namespace Eduzo.Games.CookieMonster.UI
{
    /// <summary>
    /// Main gameplay screen container
    /// Holds the game HUD and gameplay area
    /// </summary>
    public class CookieMonsterGameplayScreen : CookieMonsterUIScreen
    {
        [Header("References")]
        [SerializeField] private CookieMonsterGameHUD cookieMonsterGameHUD;
        [SerializeField] private Button _pauseButton;

        private CookieMonsterEventManager _cookieMonsterEventManager;

        public override void Initialize()
        {
            base.Initialize();

            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();

            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        protected override void OnShow()
        {
            base.OnShow();

            if (cookieMonsterGameHUD != null)
            {
                cookieMonsterGameHUD.gameObject.SetActive(true);
                cookieMonsterGameHUD.ResetHUD();
            }
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (cookieMonsterGameHUD != null)
            {
                cookieMonsterGameHUD.gameObject.SetActive(false);
            }
        }

        private void OnPauseClicked()
        {
            CookieMonsterGameManager.Instance?.PauseGame();
            // Show pause menu (could be implemented as a popup)
        }
    }
}
