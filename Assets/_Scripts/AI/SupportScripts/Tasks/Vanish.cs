using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts
{
    public class Vanish : Task
    {
        private State _vanishState;
        private Rigidbody2D _rb2d;
        private EnemyHealth _health;
        private bool _vanishTriggered = false;
        private bool _vanishCompleted = false;
        private float _vanishPauseDuration = 3;
        private float _vanishPauseTimer = 0;
        
        public Vanish(Animator animator, Character character) : base(animator, character)
        {
            _health = character.GetComponent<EnemyHealth>();
            _rb2d = character.GetComponent<Rigidbody2D>();
            _vanishState = new State(
                GetAnimator(),
                Constants.StateName.Vanish, 
                Constants.Vanish);
        }

        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _vanishState
            };
        }

        public override State DetermineState()
        {
            return _vanishState;
        }

        public override void HandleStates(Vector2 goalPosition)
        {
            HandleVanish();
            /*if (!_vanishState.IsAnimationInProgress())
            {
                _vanishState.TriggerState();
            }
            else
            {
                HandleVanish();
            }*/
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.Vanish;
        }


        private void HandleVanish()
        {
            //trigger the death animation first
            if (!_vanishState.IsAnimationInProgress() && !_vanishTriggered && !_vanishCompleted)
            {
                //_character.FreezeRigidbodyInPlace();
                _vanishState.TriggerState();
                _health.DisableCompletely();
                VanishPause();
                _vanishTriggered = true;
                return;
            } else if (_vanishState.IsAnimationInProgress() && _vanishTriggered && !_vanishCompleted)
            {
                _vanishTriggered = false;
                _vanishCompleted = true;
                _health.SetDeathCompleted(true);
            }
            else
            {
                return;
            }
            if (_vanishTriggered && _vanishState.IsAnimationInProgress())
            {
                return;
            }
        }

        private bool VanishPause()
        {
            if (_vanishTriggered)
            {
                _vanishPauseTimer += Time.deltaTime;
                if (_vanishPauseTimer < _vanishPauseDuration)
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
    }
}