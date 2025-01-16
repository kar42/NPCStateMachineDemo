using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts
{
    public class Block : Task
    {
        private State _blockState;

        public Block(Animator animator, Character character) : base(animator, character)
        {
            //Start timer high so they can block right away on first trigger
            _taskCooldown = 10.0f;
            _blockState = new State(
                GetAnimator(),
                Constants.StateName.Block, 
                Constants.Defend);
        }
        
        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _blockState
            };
        }

        public override State DetermineState()
        {
            return _blockState;
        }

        public override void HandleStates(Vector2 goalPosition)
        {
            //Increase timer that controls attack spamming
            //note: this will only update while this task is active
            if (!_blockState.IsAnimationInProgress() && !IsOnCooldown())
            {
                _blockState.TriggerState();
                //_character.SetDefenceWarning(false);
                ResetTaskCooldown();
            } 
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.Block;
        }

        public override void TaskSwitchBehavior()
        {
        }
        
        override public bool IsOnCooldown()
        {
            return _taskCooldown < 5f;
        }
    }
}