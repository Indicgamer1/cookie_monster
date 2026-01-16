using UnityEngine;
using Eduzo.Games.CookieMonster.Patterns;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.UI;
using Eduzo.Games.CookieMonster.Audio;
using Eduzo.Games.CookieMonster.Gameplay;

namespace Eduzo.Games.CookieMonster.GameStates
{
    /// <summary>
    /// Question submission state - allows player to submit questions before test mode
    /// </summary>
    public class CookieMonsterQuestionSubmissionState : CookieMonsterGameState
    {
        private CookieMonsterEventManager _cookieMonsterEventManager;
        private CookieMonsterUIManager _uiManager;
        private CookieMonsterAudioManager _audioManager;

        public override void Enter(CookieMonsterGameStateManager manager)
        {
            base.Enter(manager);

            _cookieMonsterEventManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterEventManager>();
            _uiManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterUIManager>();
            _audioManager = CookieMonsterServiceLocator.Instance.Get<CookieMonsterAudioManager>();

            // Reset questions when entering submission screen
            if (CookieMonsterNewQuestionGenerator.Instance != null)
            {
                CookieMonsterNewQuestionGenerator.Instance.ClearQuestions();
                CookieMonsterNewQuestionGenerator.Instance.Reset();
            }

            _uiManager?.ShowScreen("QuestionSubmissionScreen");
            _audioManager?.PlayMusic("MenuMusic");

            Debug.Log("Entered Question Submission State");
        }

        public void OnQuestionsSubmitted()
        {
            // Hide question submission screen
            _uiManager?.HideScreen("QuestionSubmissionScreen");
            
            // Start test mode gameplay
            stateManager.ChangeState(new CookieMonsterGameplayState(false));
        }

        public void OnBackToMenu()
        {
            stateManager.ChangeState(new CookieMonsterMainMenuState());
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("QuestionSubmissionScreen");
            Debug.Log("Exited Question Submission State");
        }
    }
}
