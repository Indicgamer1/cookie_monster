using UnityEngine;
using Eduzo.Games.CookieMonster.Patterns;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.UI;
using Eduzo.Games.CookieMonster.Audio;

namespace Eduzo.Games.CookieMonster.GameStates
{
    /// <summary>
    /// Game over state - shows final score and results
    /// Saves high score and displays achievements
    /// </summary>
    public class CookieMonsterGameOverState : CookieMonsterGameState
    {
        private readonly int _finalScore;
        private readonly float _accuracy;

        private CookieMonsterEventManager _cookieMonsterEventManager;
        private CookieMonsterUIManager _uiManager;
        private CookieMonsterAudioManager _audioManager;

        public CookieMonsterGameOverState(int finalScore, float accuracy)
        {
            _finalScore = finalScore;
            _accuracy = accuracy;
        }

        public override void Enter(CookieMonsterGameStateManager manager)
        {
            base.Enter(manager);

            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();
            _uiManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterUIManager>();
            _audioManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterAudioManager>();

            // Note: PlayerPrefs removed per coding standards
            // High score should be managed through a proper data persistence system

            _uiManager?.ShowScreen("GameOverScreen");
            _uiManager?.UpdateGameOverResults(_finalScore, _accuracy, 0); // High score removed

            // Play different music based on performance
            if (_accuracy >= 0.8f)
            {
                _audioManager?.PlaySFX("Victory");
            }
            else
            {
                _audioManager?.PlaySFX("GameOver");
            }

            Debug.Log($"Game Over - Score: {_finalScore}, Accuracy: {_accuracy:P}");
        }

        // Note: PlayerPrefs removed per coding standards
        // High score persistence should be handled through a proper data management system

        public void OnPlayAgain()
        {
            // For test mode, restart from question submission screen
            stateManager.ChangeState(new CookieMonsterQuestionSubmissionState());
        }

        public void OnMainMenu()
        {
            stateManager.ChangeState(new CookieMonsterMainMenuState());
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("GameOverScreen");
            Debug.Log("Exited Game Over State");
        }
    }
}
