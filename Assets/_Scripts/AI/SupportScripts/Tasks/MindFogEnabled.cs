using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts
{
    public class MindFogEnabled : Task
    {
        private State _mindFogState;

        public MindFogEnabled(Animator animator, Character character) : base(animator, character)
        {
            _mindFogState = new State(
                GetAnimator(),
                Constants.StateName.MindFog, 
                Constants.MindFogEnabled);
        }
        
        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _mindFogState
            };
        }

        public override State DetermineState()
        {
            return _mindFogState;
        }

        public override void HandleStates(Vector2 goalPosition)
        {
            if (_enemy.IsMindFogEnabled())
            {
                _mindFogState.EnableState();
            }
            else
            {
                _mindFogState.DisableState();
            }
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.MindFog;
        }

        public override void TaskSwitchBehavior()
        {
            _mindFogState.DisableState();
        }
    }
}