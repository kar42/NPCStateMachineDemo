using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace SupportScripts
{
    public class RangedAttack : Task
    {
        private State _castFireball; //todo change this to be generic and taken from character?
        private float _timeSinceAttack;

        public RangedAttack(Animator animator, Character character) : base(animator, character)
        {
            //Start attack timer high so they can attack right away on first trigger
            _timeSinceAttack = 3.0f;
            _castFireball = new State(
                GetAnimator(),
                Constants.StateName.CastTrackingBall, 
                Constants.CastTrackingBall);
        }
        
        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _castFireball
            };
        }

        public override void TaskSwitchBehavior()
        {
            _character.UnFreezeRigidbody();
        }

        public override State DetermineState()
        {
            return _castFireball;
        }

        //Core Task method to trigger State behavior
        public override void HandleStates(Vector2 goalPosition)
        {
            //character should not be moveable in this state
            _character.FreezeRigidbodyInPlace();
            //Update the current goalPosition
            _goalPosition = goalPosition;
            //Increase timer that controls attack spamming
            //note: this will only update while this task is active
            _timeSinceAttack += Time.deltaTime;
            //Try to attack
            MaybeCastFireball();
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.RangedAttack;
        }

        private void MaybeCastFireball()
        {
            if (!IsOnCooldown())
            { //attack cooldown has ended, character may attack again
                CastFireball();
            }
        }

        public float GetTimeSinceRangedAttack()
        {
            return _timeSinceAttack;
        }
        
    
        protected void CastFireball()
        {
            _castFireball.TriggerState();
            _timeSinceAttack = 0.0f;
        }

        private Vector2 CharacterFacePosition()
        {
            return SpriteFinder.GetColliderFacingCenterPosition(
                _character.GetCharacterCollider(),
                _character.IsCharacterFacingRight());
        }

        override public bool IsOnCooldown()
        {
            return _timeSinceAttack < 3f;
        }
    }
}