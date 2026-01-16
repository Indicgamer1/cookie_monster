using UnityEngine;
using Eduzo.Games.CookieMonster.Patterns;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.UI;

namespace Eduzo.Games.CookieMonster.GameStates
{
    /// <summary>
    /// Loading state - simulates loading game assets
    /// In production, this would load actual assets asynchronously
    /// </summary>
    public class CookieMonsterLoadingState : CookieMonsterGameState
    {
        private const float LOADING_DURATION = 1.5f;
        private float _timer;
        private CookieMonsterEventManager _cookieMonsterEventManager;
        private CookieMonsterUIManager _uiManager;

        public override void Enter(CookieMonsterGameStateManager manager)
        {
            base.Enter(manager);

            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();
            _uiManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterUIManager>();

            _timer = LOADING_DURATION;
            _uiManager?.ShowScreen("LoadingScreen");

            Debug.Log("Entered Loading State");
        }

        public override void Update()
        {
            _timer -= Time.deltaTime;

            // Update loading progress (simulated)
            float progress = 1f - (_timer / LOADING_DURATION);
            _uiManager?.UpdateLoadingProgress(progress);

            if (_timer <= 0f)
            {
                // Transition directly to main menu (Student Name screen removed)
                stateManager.ChangeState(new CookieMonsterMainMenuState());
            }
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("LoadingScreen");
            Debug.Log("Exited Loading State");
        }
    }
}
