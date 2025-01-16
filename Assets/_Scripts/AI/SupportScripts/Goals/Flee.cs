using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts.Goals
{
    public class Flee : Goal
    {
        [SerializeField] private bool isFleeing = false;
        [SerializeField] private bool shouldVanish = false;
        private float fleeTimer = 0f;
        private Vector2 fleePosition;
        [SerializeField] private int fleeTriggerDistance = 15;
        [SerializeField] private int fleeDistance = 50;
        [SerializeField] private int fleeDuration = 20;
        [SerializeField] private int vanishTriggerDistance = 50;

        internal bool HasMindFogTarget = false;
        internal Vector2 MindFogTarget;
        internal Transform MindFogTargetTransform;
        internal Transform CurrentTarget;
        internal int CurrentTargetLayer;


        protected override void Initialize()
        {
            isFleeing = false;
            fleeTimer = 0;
            shouldVanish = false;
        }

        protected override List<Constants.TaskName> DefaultGoalTaskNames()
        {
            return new List<Constants.TaskName>
            {
                Constants.TaskName.Idle,
                Constants.TaskName.Move,
                Constants.TaskName.MovementOverride,
                Constants.TaskName.Vanish
                //todo add vanish
            };
        }

        protected override bool CheckIfGoalIsAvailable()
        {
            //todo calculate if fox should be fleeing
            return ShouldFlee();
        }

        protected override Vector2 CalculateGoalPosition()
        {
            return SetFleePosition(_character.IsCharacterFacingRight());
        }

        protected override void CalculateTaskPriorities()
        {
            Constants.TaskName selectedTaskName;
           if (_character.HasMovementOverride()){
                GetAvailableTasks()[Constants.TaskName.Vanish].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Move].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Idle].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.MovementOverride].SetPriorityScore(9);
                selectedTaskName = Constants.TaskName.MovementOverride;
            }
            else if (!isFleeing)
            {
                GetAvailableTasks()[Constants.TaskName.Vanish].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.MovementOverride].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Move].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Idle].SetPriorityScore(5);
                selectedTaskName = Constants.TaskName.Idle;
            }
            else if (ShouldVanish())
            {
                GetAvailableTasks()[Constants.TaskName.MovementOverride].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Move].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Idle].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Vanish].SetPriorityScore(9);
                selectedTaskName = Constants.TaskName.Vanish;
            }
            else
            {
                GetAvailableTasks()[Constants.TaskName.Vanish].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.MovementOverride].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Idle].SetPriorityScore(0);
                GetAvailableTasks()[Constants.TaskName.Move].SetPriorityScore(5);
                ((Move) GetAvailableTasks()[Constants.TaskName.Move])
                    .SetCanRun(true);
                selectedTaskName = Constants.TaskName.Move;
            }
            //HandleTaskChange(selectedTaskName);
        }

        protected override void GoalSwitchBehavior()
        {
            
        }

        /**
     ***************Flee Behavior CODE**********************************
    **/
        public bool IsFleeing()
        {
            return isFleeing;
        }

        public Vector2 GetFleePosition()
        {
            return fleePosition;
        }

        public Vector2 SetFleePosition(bool isFacingRight)
        {
            if (!isFleeing)
            {
                //Reset the flee timer to 0 if the fleeing is not yet enabled 
                ResetFleeTimer();
            }

            //Fleeing is active
            isFleeing = true;
            
            //  If an aggressive character is near enough, change fleeBehavior direction/position
            var hasAggressiveCharacterAhead =
                SpriteFinder.HasAggressiveCharacterAhead(
                    _character.GetCharacterCollider(), 
                    fleeTriggerDistance, 
                    _character.IsCharacterFacingRight());
            if (hasAggressiveCharacterAhead)
            {
                if (isFacingRight)
                {
                    fleePosition = new Vector2(
                        _character.GetCharacterCollider().bounds.center.x - fleeDistance, 
                        _character.GetCharacterCollider().bounds.center.y);
                }
                else
                {
                    fleePosition = new Vector2(
                        _character.GetCharacterCollider().bounds.center.x + fleeDistance, 
                        _character.GetCharacterCollider().bounds.center.y);
                }
            }
            else //if the cause of flee is not ahead, we can assume it's coming from behind
            {
                if (isFacingRight)
                {
                    fleePosition = new Vector2(
                        _character.GetCharacterCollider().bounds.center.x + fleeDistance, 
                        _character.GetCharacterCollider().bounds.center.y);
                }
                else
                {
                    fleePosition = new Vector2(
                        _character.GetCharacterCollider().bounds.center.x - fleeDistance, 
                        _character.GetCharacterCollider().bounds.center.y);
                }
            }

            return fleePosition;
        }

        public float ResetFleeTimer()
        {
            return fleeTimer = 0;
        }

        public float GetFleeTimer()
        {
            return fleeTimer;
        }

        public float GetFleeTriggerDistance()
        {
            return fleeTriggerDistance;
        }

        public float GetVanishTriggerDistance()
        {
            return vanishTriggerDistance;
        }

        public float GetFleeDistance()
        {
            return fleeDistance;
        }

        public float GetFleeDuration()
        {
            return fleeDuration;
        }

        public void MaybeIncrementFleeTimer()
        {
            if (isFleeing)
            {
                fleeTimer++;
            }
        }

        public void EndFlee()
        {
            isFleeing = false;
            fleeTimer = 0;
            _animator.SetBool(Constants.MovingFast, false);
        }

        private bool ShouldVanish()
        {
            if (//GetFleeTimer() >= GetFleeDuration() && 
                     (SpriteFinder.HasAggressiveCharacterAhead(
                          _character.GetCharacterCollider(), 15, _character.IsCharacterFacingRight())
                      || SpriteFinder.HasAggressiveCharacterBehind(
                          _character.GetCharacterCollider(), 15, _character.IsCharacterFacingRight())))
            {
                return shouldVanish = true;
            }
            return shouldVanish = false;
        }

        public void Vanish()
        {
            //change layer order to be behind all other characters
            //add code to only enable this if the background environment is clear
            EndFlee();
            _animator.SetTrigger(Constants.Vanish);
            spriteRenderer.sortingOrder = 9;
            health.DisableCompletely();
            //ToDo : add code to delete the game object at the end of vanish
        }

        public void PivotEscape()
        {

        }

        private bool ShouldFlee()
        {
            if (_character.IsCharacterDocile()
                && (SpriteFinder.HasAggressiveCharacterAhead(
                        _character.GetCharacterCollider(),
                        30, 
                        _character.IsCharacterFacingRight())
                    || SpriteFinder.HasAggressiveCharacterBehind(
                        _character.GetCharacterCollider(),
                        30, 
                        _character.IsCharacterFacingRight())))
            {
                return true;
            }
            return false;

            /*
             //Second First priority of movement control is if the character should fleeBehavior
        else if (!IsFleeing() && Personality.IsDocile(GetCurrentDemeanor()) 
                  && (SpriteFinder.HasAggressiveCharacterAhead(boxCollider, aggroDistance/2, IsFacingRight()) 
                  || SpriteFinder.HasAggressiveCharacterBehind(boxCollider, aggroDistance/4, IsFacingRight())))
        {
            if (showDebugCode)
            {
                Debug.Log(boxCollider.name+": initiating fleeing");
            }
            SetFleePosition();
            
        } //Second First priority of movement control is if the character should fleeBehavior
        else if (IsFleeing() && Personality.IsDocile(GetCurrentDemeanor()))
        {
            if (showDebugCode)
            {
                Debug.Log(boxCollider.name+": is fleeing");
            }
            ShouldStopMoving(false);
            UnFreezeRigidbody();
            
            
            if (SpriteFinder.HasAggressiveCharacterAhead(boxCollider, aggroDistance/2, IsFacingRight())
                && SpriteFinder.HasAggressiveCharacterBehind(boxCollider, aggroDistance/2, IsFacingRight()))
            {
                ShouldStopMoving(true);
                fleeBehavior.Vanish();
                
                /#2#/ToDo implement this so the character can attack if surrounded
               //Character is cornered and fleeing, they should change to aggressive
               character.CharacterCurrentDemeanor(Demeanor.EnemyDemeanor.Aggressive);
               fleeBehavior.EndFlee();#2#
            }
            else if (SpriteFinder.HasAggressiveCharacterAhead(boxCollider, aggroDistance/2, IsFacingRight()))
            {
                //does this allow ping pong behavior if enemies are positioned just outside of distance given?
                SetFleePosition();
            }
            else if (IsEnemyRunningIntoTheGround() || IsEnemyRunningTowardsACliff())
            {
                //should run off into the distance if they've been chased to the end of the ground
                ShouldStopMoving(true);
                fleeBehavior.Vanish();
            }
            else if (fleeBehavior.GetFleeTimer() >= fleeBehavior.GetFleeDuration() 
                     && (SpriteFinder.HasAggressiveCharacterAhead(boxCollider, aggroDistance/4, IsFacingRight())
                          || SpriteFinder.HasAggressiveCharacterBehind(boxCollider, aggroDistance/4, IsFacingRight())))
            {
                fleeBehavior.Vanish();
            }
            else if (fleeBehavior.GetFleeTimer() >= fleeBehavior.GetFleeDuration() 
                     && !(SpriteFinder.HasAggressiveCharacterAhead(boxCollider, aggroDistance/4, IsFacingRight())
                          || SpriteFinder.HasAggressiveCharacterBehind(boxCollider, aggroDistance/4, IsFacingRight())))
            {
                //Flee has gone on long enough, may stop fleeing
                //fleeBehavior.EndFlee();
            }
            else
            {
                ShouldStopMoving(false);
            }

            //Todo add code to prevent player from stopping character when they aren't pushing against (landed next to them etc)
            //How to prevent player from pushing enemy while they are fleeing??
            
        }*/
        }
    }
}