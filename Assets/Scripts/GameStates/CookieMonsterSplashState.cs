using UnityEngine;
using Eduzo.Games.CookieMonster.Patterns;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.UI;

namespace Eduzo.Games.CookieMonster.GameStates
{
    /// <summary>
    /// Splash screen state - shows game logo
    /// Follows State Pattern for clean state transitions
    /// </summary>
    public class CookieMonsterSplashState : CookieMonsterGameState
    {
        private const float SPLASH_DURATION = 2f;
        private float _timer;
        private CookieMonsterEventManager _cookieMonsterEventManager;
        private CookieMonsterUIManager _uiManager;

        public override void Enter(CookieMonsterGameStateManager manager)
        {
            base.Enter(manager);

            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();
            _uiManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterUIManager>();

            _timer = SPLASH_DURATION;
            _uiManager?.ShowScreen("SplashScreen");

            Debug.Log("Entered Splash State");
        }

        public override void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                // Transition to loading state
                stateManager.ChangeState(new CookieMonsterLoadingState());
            }
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("SplashScreen");
            Debug.Log("Exited Splash State");
        }
    }
}
