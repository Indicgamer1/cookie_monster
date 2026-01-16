using UnityEngine;
using Eduzo.Games.CookieMonster.Patterns;
using Eduzo.Games.CookieMonster.Core;
using Eduzo.Games.CookieMonster.UI;
using Eduzo.Games.CookieMonster.Audio;

namespace Eduzo.Games.CookieMonster.GameStates
{
    /// <summary>
    /// Main menu state - allows player to choose Practice or Test mode
    /// </summary>
    public class CookieMonsterMainMenuState : CookieMonsterGameState
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

            _uiManager?.ShowScreen("MainMenuScreen");
            _audioManager?.PlayMusic("MenuMusic");

            Debug.Log("Entered Main Menu State");
        }

        public void OnPracticeMode()
        {
            stateManager.ChangeState(new CookieMonsterGameplayState(true));
        }

        public void OnTestMode()
        {
            stateManager.ChangeState(new CookieMonsterQuestionSubmissionState());
        }

        public void OnSettings()
        {
            _uiManager?.ShowScreen("SettingsScreen");
        }

        public void OnInfo()
        {
            _uiManager?.ShowScreen("InfoScreen");
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("MainMenuScreen");
            _audioManager?.StopMusic();
            Debug.Log("Exited Main Menu State");
        }
    }
}
