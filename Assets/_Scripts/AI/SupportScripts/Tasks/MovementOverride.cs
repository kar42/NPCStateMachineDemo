using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts
{
    /**
     * Movement Override is one of the more complex Task classes.
     * It requires checking whether external interactions have occurred with an Enemy Character,
     * such as if they should be falling or have been hurt.
     * Movement Overrides should always take highest priority, second only to Death.
     */
    public class MovementOverride : Task
    {
        protected Constants.EnemyMovementOverride movementOverride = Constants.EnemyMovementOverride.None;
        protected float movementOverrideDistance = 0f;
        protected float movementOverrideDuration = 0f;
        protected float movementOverrideCooldown = 0f;
        private float movementOverrideTimer = 0f;
        private Health _health;
        private Rigidbody2D _rb2d;
        private State _idleState;
        private State _hurtState;
        private State _fallState;
        private State _freezeState;
        private bool _hurting;

        public MovementOverride(Animator animator, Character character): base(animator, character)
        {
            _rb2d = character.GetComponent<Rigidbody2D>();
            _health = character.GetComponent<Health>();
            _idleState = new State(
                GetAnimator(),
                Constants.StateName.Idle, 
                Constants.Idle);
            _hurtState = new State(
                GetAnimator(),
                Constants.StateName.Hurt, 
                Constants.Hurt);
            _fallState = new State(
                GetAnimator(),
                Constants.StateName.Fall,
                Constants.Fall);
            _freezeState = new State(
                GetAnimator(),
                Constants.StateName.Freeze,
                Constants.Hurt);

        }

        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _idleState,
                _hurtState,
                _fallState,
                _freezeState
            };
        }

        public override void TaskSwitchBehavior()
        {
            CheckForStateChange();
        }

        public override State DetermineState()
        {
            //maybe update what the current movementOverride is
            CheckForStateChange();
            
            //return the state that corresponds to the character's override
            if (movementOverride == Constants.EnemyMovementOverride.Fall)
            {
                return _fallState;
            }
            if (movementOverride == Constants.EnemyMovementOverride.Hurt)
            { 
                return _hurtState;
            }
            if (movementOverride == Constants.EnemyMovementOverride.Freeze)
            {
                return _freezeState;
            }
            //default if nothing found - should not happen
            movementOverride = Constants.EnemyMovementOverride.None;
            return _idleState;
        }

        public override void HandleStates(Vector2 goalPosition)
        {
            //Update the current goalPosition
            _goalPosition = goalPosition;
            //Handle the current movement override state
            switch (movementOverride)
            {
                case Constants.EnemyMovementOverride.Fall:
                    UnFreezeRigidbody();
                    Fall();
                    return;
                case Constants.EnemyMovementOverride.Freeze:
                    Freeze();
                    return;
                /*case Constants.EnemyMovementOverride.KnockBack:
                    KnockBack();
                    return;*/
                case Constants.EnemyMovementOverride.Hurt:
                    Hurt();
                    return;
                default :
                    return;
            }
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.MovementOverride;
        }

        private void CheckForStateChange()
        {
            //check what the latest movement override for the character is
            Constants.EnemyMovementOverride updatedMovementOverride = 
                _character.CheckForMovementOverride();
            if (updatedMovementOverride != movementOverride)
            {
                //if it has changed, maybe perform switching behavior
                switch (movementOverride)
                {
                    case Constants.EnemyMovementOverride.Fall:
                        break;
                    case Constants.EnemyMovementOverride.Freeze:
                        ClearFreeze();
                        break;
                    case Constants.EnemyMovementOverride.KnockBack:
                        break;
                    case Constants.EnemyMovementOverride.Hurt:
                        break;
                    default :
                        break;
                }
                //update the class' movement override value for processing
                movementOverride = updatedMovementOverride;
            }
        }

        //** HURT CODE **//
        protected void Hurt()
        {
            //Check if hurting is already happening
            if (!HandlingHurtInProgress())
            {
                _hurtState.TriggerState();
                _hurting = true;
                if (_character.IsCharacterNeutral())
                {
                    //Make sure character is aggressive
                    _enemy.SetCurrentDemeanor(Personality.EnemyPersonality.Aggressive);
                    //prevent them from passing the player (like when docile or neutral)
                    _character.ResetAllIgnoredColliders();
                }
            }
        }
        
        private bool HandlingHurtInProgress()
        {
            if (_hurting && _hurtState.IsAnimationInProgress())
            { 
                //if currently hurting, make sure no velocity
                _rb2d.velocity = new Vector2(0, _rb2d.velocity.y);
                return true;
            } else if (!_hurting && _hurtState.IsAnimationInProgress())
            {
                //if not hurting but animation started, set hurting to true
                _hurting = true;
                _rb2d.velocity = new Vector2(0, _rb2d.velocity.y);
                return true;
            } else if (_hurting && !_hurtState.IsAnimationInProgress())
            {
                //if hurting is true but animation ended, set hurting to false
                //but allow this update loop to complete
                _character.SetMovementOverride(Constants.EnemyMovementOverride.None);
                _hurting = false;
                return true;
            }
            //hurt animation is not in progress and hurting is not true
            return false;
        }

        
        //** FALLING CODE **//
        protected void Fall()
        {
            //check if character is on the ground
            if (!_character.IsGrounded())
            {
                //if not trigger fall animation
                _rb2d.velocity = Vector2.down * GetCharacter().GetRunSpeed();
                _fallState.TriggerState();
            }
            else
            {
                //if they are, end fall override
                _character.SetMovementOverride(Constants.EnemyMovementOverride.None);
            }
        }


        //** FREEZE CODE / SPEAR ATTACK **//
        protected void Freeze(float damage = 0)
        {
            //prevent all movement
            FreezeRigidbodyInPlace();
            //change character to black (placeholder visual effect)
            _character.SetCharacterColor(new Color(0, 0, 0, 1));
        }
    
        public void ClearFreeze(float damage = 0)
        {
            //revert character color from black
            _character.ResetCharacterColor();
            //allow normal movement
            UnFreezeRigidbody();
        }

        public void FreezeRigidbodyInPlace()
        {
            _rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            _rb2d.velocity = new Vector2(0, 0);
        }

        public void UnFreezeRigidbody()
        {
            _rb2d.constraints = RigidbodyConstraints2D.None;
            _rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        }


        //** KNOCKBACK CODE - DISABLED **//
        protected bool KnockBack()
        {
            if (movementOverride == Constants.EnemyMovementOverride.KnockBack && movementOverrideTimer > movementOverrideDuration)
            {
                movementOverrideTimer = 0f;
                movementOverride = Constants.EnemyMovementOverride.None;
                _rb2d.velocity = new Vector2(0, _rb2d.velocity.y);
                return false;
            }
            else if (movementOverride == Constants.EnemyMovementOverride.KnockBack && movementOverrideTimer <= movementOverrideDuration)
            {
                UnFreezeRigidbody();
                if (GetCharacter().IsCharacterFacingRight())
                {
                    _rb2d.velocity = new Vector2(Vector2.left.x * movementOverrideDistance, _rb2d.velocity.y);
                }
                else
                {
                    _rb2d.velocity = new Vector2(Vector2.right.x * movementOverrideDistance, _rb2d.velocity.y);
                }
                movementOverrideTimer += Time.deltaTime;
                return true;
            }
            else
            {
                movementOverride = Constants.EnemyMovementOverride.None;
                movementOverrideTimer = 0f;
                _rb2d.velocity = new Vector2(0, _rb2d.velocity.y);
                return false;
            }
        }

    }
}