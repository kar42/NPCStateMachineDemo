using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts
{
    public class Heal : Task
    {
        private State _healState;

        public Heal(Animator animator, Character character) : base(animator, character)
        {
            _healState = new State(
                GetAnimator(),
                Constants.StateName.Heal, 
                Constants.Heal);
        }
        
        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _healState
            };
        }

        public override State DetermineState()
        {
            return _healState;
        }

        public override void HandleStates(Vector2 goalPosition)
        {
            //character should not be moveable in this state
            _character.FreezeRigidbodyInPlace();
            if (!_healState.IsAnimationInProgress() && !IsOnCooldown())
            {
                _character.HealEnemy();
                _healState.TriggerState();
                ResetTaskCooldown();
            }
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.Heal;
        }

        public override void TaskSwitchBehavior()
        {
            _character.UnFreezeRigidbody();
        }

        override public bool IsOnCooldown()
        {
            return _taskCooldown < 5f;
        }
    }
}