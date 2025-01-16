using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace SupportScripts
{
    public class Attack : Task
    {
        private State _attack; //todo change this to be generic and taken from character?
        private float _timeSinceAttack;

        public Attack(Animator animator, Character character) : base(animator, character)
        {
            _timeSinceAttack = 3.0f;
            _attack = new State(
                GetAnimator(),
                Constants.StateName.Attack, 
                Constants.Attack);
        }
        
        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _attack
            };
        }

        public override void TaskSwitchBehavior()
        {
            _character.UnFreezeRigidbody();
        }

        public override State DetermineState()
        {
            return _attack;
        }

        //Core Task method to trigger State behavior
        public override void HandleStates(Vector2 goalPosition)
        {
            //character should not be moveable in this state
            _character.FreezeRigidbodyInPlace();
            //Update the current goalPosition
            _goalPosition = goalPosition;
            //Increase timer that controls attack spamming
            _timeSinceAttack += Time.deltaTime;
            //Try to attack
            MaybeAttack();
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.Attack;
        }
        

        private void MaybeAttack()
        {
            if (IsOnCooldown())
            {
                return;
            }
            var hasAttacked = false;
            Collider2D[] hitColliders = Physics2D
                .OverlapCircleAll(_enemy.GetAttackPointPosition(), _enemy.GetAttackPointRange());
            //attack them
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (IsPlayer(hitCollider.name))
                {
                    var playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
                    //Debug.Log("bandit collider : " + enemy.name);
                    if (playerHealth != null && !playerHealth.dead)
                    {
                        if (!hasAttacked)
                        { //prevents sprite from continually attacking a dead target
                            _attack.TriggerState();
                            hasAttacked = true;
                            _timeSinceAttack = 0.0f;
                        }

                    }
                }
                if (SpriteFinder.IsEnemyOtherThanMe(hitCollider.name, _character.GetCharacterCollider()))
                { ///allow friendly fire?
                    var otherEnemyHealth = hitCollider.GetComponent<Health>();
                    if (otherEnemyHealth != null && !otherEnemyHealth.dead)
                    {
                        if (!hasAttacked)
                        { //prevents sprite from continually attacking a dead target
                            _attack.TriggerState();
                            hasAttacked = true;
                            _timeSinceAttack = 0.0f;
                        }

                    }
                }
                 else
                {
                    //Debug.Log("bandit has hit nothing");
                    //hasTarget = false;
                }

            }
        }

        private bool IsPlayer(string name)
        { 
            return name == "Player" || name == "Sprite";
        }

        override public bool IsOnCooldown()
        {
            return _timeSinceAttack < 1f;
        }
    }
}