using UnityEngine;
using Eduzo.Games.CookieMonster.Patterns;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.UI;

namespace Eduzo.Games.CookieMonster.GameStates
{
    /// <summary>
    /// Practice mode completion state
    /// Shows results without pressure
    /// </summary>
    public class CookieMonsterPracticeCompleteState : CookieMonsterGameState
    {
        private readonly int _finalScore;
        private readonly float _accuracy;

        private CookieMonsterEventManager _cookieMonsterEventManager;
        private CookieMonsterUIManager _uiManager;

        public CookieMonsterPracticeCompleteState(int finalScore, float accuracy)
        {
            _finalScore = finalScore;
            _accuracy = accuracy;
        }

        public override void Enter(CookieMonsterGameStateManager manager)
        {
            base.Enter(manager);

            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();
            _uiManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterUIManager>();

            _uiManager?.ShowScreen("PracticeCompleteScreen");
            _uiManager?.UpdatePracticeResults(_finalScore, _accuracy);

            Debug.Log($"Practice Complete - Score: {_finalScore}, Accuracy: {_accuracy:P}");
        }

        public void OnTryAgain()
        {
            stateManager.ChangeState(new CookieMonsterGameplayState(true));
        }

        public void OnMainMenu()
        {
            stateManager.ChangeState(new CookieMonsterMainMenuState());
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("PracticeCompleteScreen");
            Debug.Log("Exited Practice Complete State");
        }
    }
}
