using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts
{
    public class Death : Task
    {
        private State _deathState;
        private State _fallState;
        private Rigidbody2D _rb2d;
        private EnemyHealth _health;
        private bool _deathTriggered = false;
        private bool _deathCompleted = false;
        private float _deathPauseDuration = 3;
        private float _deathPauseTimer = 0;
        
        public Death(Animator animator, Character character) : base(animator, character)
        {
            _health = character.GetComponent<EnemyHealth>();
            _rb2d = character.GetComponent<Rigidbody2D>();
            _deathState = new State(
                GetAnimator(),
                Constants.StateName.Death, 
                Constants.Death);
            _fallState = new State(
                GetAnimator(),
                Constants.StateName.Fall, 
                Constants.Fall);
        }

        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _deathState,
                _fallState
            };
        }

        public override State DetermineState()
        {
            //If character is not grounded, attempt to let them
            // finish falling before processing death.
            if (!_character.IsGrounded())
            {
                return _fallState;
            }
            //Deal with death
            return _deathState;
        }

        public override void HandleStates(Vector2 goalPosition)
        {
            switch (GetCurrentStateName())
            {
                case (Constants.StateName.Fall) :
                    DeathFall();
                    return;
                case (Constants.StateName.Death) :
                    if (!_deathCompleted)
                    {
                        HandleDeath();
                    }
                    return;
            }
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.Die;
        }


        private void HandleDeath()
        {
            //trigger the death animation first
            if (!_deathState.IsAnimationInProgress() && !_deathTriggered && !_deathCompleted)
            {
                _character.FreezeRigidbodyInPlace();
                _deathState.TriggerState();
                AudioManager.instance.PlaySound("EnemyDeath_sfx");
                _health.DisableCompletely();
                DeathPause();
                _deathTriggered = true;
                return;
            } else if (_deathState.IsAnimationInProgress() && _deathTriggered && !_deathCompleted)
            {
                //_health.DisableEnemy();
                //DeathPause();
                _deathTriggered = false;
                _deathCompleted = true;
                _health.SetDeathCompleted(true);
            }
            else
            {
                return;
            }
            if (_deathTriggered && _deathState.IsAnimationInProgress())
            {
                return;
            }
        }

        private bool DeathPause()
        {
            if (_deathTriggered)
            {
                _deathPauseTimer += Time.deltaTime;
                if (_deathPauseTimer < _deathPauseDuration)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        protected void DeathFall()
        {
            _rb2d.velocity = Vector2.down * GetCharacter().GetRunSpeed();
            _fallState.TriggerState();
        }
    }
}