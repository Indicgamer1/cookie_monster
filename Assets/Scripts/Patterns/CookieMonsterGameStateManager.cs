using UnityEngine;

namespace Eduzo.Games.CookieMonster.Patterns
{
    /// <summary>
    /// State Machine pattern for managing game states
    /// Follows Open/Closed Principle - open for extension, closed for modification
    /// </summary>
    public abstract class CookieMonsterGameState
    {
        protected CookieMonsterGameStateManager stateManager;

        public virtual void Enter(CookieMonsterGameStateManager manager)
        {
            stateManager = manager;
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }

        public virtual void Exit()
        {
        }
    }

    public class CookieMonsterGameStateManager : MonoBehaviour
    {
        private CookieMonsterGameState _currentState;

        public void ChangeState(CookieMonsterGameState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter(this);
            Debug.Log($"State changed to: {newState?.GetType().Name}");
        }

        private void Update()
        {
            _currentState?.Update();
        }

        private void FixedUpdate()
        {
            _currentState?.FixedUpdate();
        }

        public CookieMonsterGameState GetCurrentState() => _currentState;
    }
}
