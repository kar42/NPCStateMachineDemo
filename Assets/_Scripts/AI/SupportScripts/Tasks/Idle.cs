using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts
{
    public class Idle : Task
    {
        private State _idleState;

        public Idle(Animator animator, Character character) : base(animator, character)
        {
            _idleState = new State(
                GetAnimator(),
                Constants.StateName.Idle, 
                Constants.Idle);
        }
        
        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _idleState
            };
        }

        public override State DetermineState()
        {
            return _idleState;
        }

        public override void HandleStates(Vector2 goalPosition)
        {
            if (!_idleState.IsAnimationInProgress())
            {
                _idleState.TriggerState();
            }
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.Idle;
        }
    }
}