using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using SupportScripts;
using UnityEngine;

public class Patrol : Goal
{
    [Header("Patrol")]
    [SerializeField] private float scoutDistance = 300f;
    [SerializeField] private float patrolDistance = 100f;
    [SerializeField] private bool shouldPatrol = false;
    [SerializeField] private bool isPatrolling = false;
    [SerializeField] private float distancePatrolLeft;
    [SerializeField] private float distancePatrolRight;
    [SerializeField] private bool patrolRight = true;
    [SerializeField] private float patrolPauseDuration = 4f;
    [SerializeField] private bool _patrolPause = false;
    [SerializeField] private float _patrolWaitTimer = 0;
    private Vector2 _patrolPointLeft;
    private Vector2 _patrolPointRight;
    
    protected override void Initialize()
    {
        //Default initialization of the Patrol goal
        shouldPatrol = true;
        patrolRight = true;
        patrolDistance = 100f;
        SetPatrolPoints();
        MaybeUpdatePatrolPoints();
    }

    protected override List<Constants.TaskName> DefaultGoalTaskNames()
    {
        //Patrolling consists of Idle and Move task behavior
        //All goals need to contain the ability to handle movement overrides
        //...and maybe death too?
        return new List<Constants.TaskName>
        {
            Constants.TaskName.Idle,
            Constants.TaskName.Move,
            Constants.TaskName.MovementOverride
        };
    }
    
    protected override bool CheckIfGoalIsAvailable()
    {
        //Patrol goal is Available on a character by character basis (defaults to true)
        return _character.GetAllowPatrol();
    }
    
    protected override Vector2 CalculateGoalPosition()
    {
        //Goal position must be up to date for pathfinding:
        //First check if the current patrol positions needs to change
        MaybeUpdatePatrolPoints();
        //Then check if the current
        MaybeUpdatePatrolDirection();
        if (PatrolRight())
        {
            return PatrolPointRight();
        }
        else
        {
            return PatrolPointLeft();
        }
    }

    protected override void CalculateTaskPriorities()
    {
        Constants.TaskName selectedTaskName;
        if (_character.HasMovementOverride()){
            GetAvailableTasks()[Constants.TaskName.Move].SetPriorityScore(1);
            GetAvailableTasks()[Constants.TaskName.Idle].SetPriorityScore(5);
            GetAvailableTasks()[Constants.TaskName.MovementOverride].SetPriorityScore(9);
            selectedTaskName = Constants.TaskName.MovementOverride;
        }
        else if (_patrolPause && PatrolWait())
        {
            GetAvailableTasks()[Constants.TaskName.Move].SetPriorityScore(1);
            GetAvailableTasks()[Constants.TaskName.Idle].SetPriorityScore(5);
            selectedTaskName = Constants.TaskName.Idle;
            GetAvailableTasks()[Constants.TaskName.MovementOverride].SetPriorityScore(0);
        }
        else
        {
            GetAvailableTasks()[Constants.TaskName.Move].SetPriorityScore(5);
            ((Move) GetAvailableTasks()[Constants.TaskName.Move])
                .SetCanRun(false);
            selectedTaskName = Constants.TaskName.Move;
            GetAvailableTasks()[Constants.TaskName.Idle].SetPriorityScore(1);
            GetAvailableTasks()[Constants.TaskName.MovementOverride].SetPriorityScore(0);
        }
    }

    protected override void GoalSwitchBehavior()
    {
        base.GoalSwitchBehavior();
        _patrolPause = true;
        _patrolWaitTimer = 0.0f;
    }

    //** STEERING AND PERCEPTION BEHAVIOR **//
    public void MaybeUpdatePatrolDirection()
    {
        if (patrolRight)
        {
            //set checking position to be 3f (same as nextwaypoint distance in Move task)
            var scoutPointX = _character.GetCharacterFacingXPosition() + 3f;
            //Check if enemy should stop moving in current direction
            AttemptPatrolDirectionSwap(_patrolPointRight,scoutPointX);
            //tracking - update and set the distance to the currently defined right patrol position
            distancePatrolRight = Vector2.Distance(_character.GetCharacterFacingPosition(), _patrolPointRight);
        }
        else if (!patrolRight)
        { 
            //same as above but for the left side
            var scoutPointX = _character.GetCharacterFacingXPosition() - 3f;
            AttemptPatrolDirectionSwap(_patrolPointLeft,scoutPointX);
            distancePatrolLeft = Vector2.Distance(_character.GetCharacterFacingPosition(), _patrolPointLeft);
        }
    }

    private void AttemptPatrolDirectionSwap(Vector2 patrolPointToCheck, float scoutPointX)
    {
        //check for a relevant collider ahead (from their highest collider point)
        var objectAhead = SpriteFinder.ScoutAroundColliderTop(
            _character.GetCharacterCollider(), scoutPointX, ShouldIgnoreCharacter());
        if (objectAhead != null)
        {
            //Some object is in the patrol path, turn around immediately
            SwapPatrolDirection();
            return;
        }
        if (SpriteFinder.IsEnemyRunningIntoAnotherCharacter(
                _character.GetCharacterCollider(),
                _character.IsCharacterFacingRight(),
                true) )
        {
            //change direction if another character of different type is in the way
            //except if they are docile or neutral (eg fox or elk)
            SwapPatrolDirection();
            return;
        }
        //check how far from enemy to given patrol point
        var distanceToPatrolPoint = Vector2.Distance(
            _character.GetCharacterCollider().ClosestPoint(patrolPointToCheck), patrolPointToCheck);
        if (distanceToPatrolPoint <= 15)
        {
            //if distance is within range of patrol point destination, turn around
            SwapPatrolDirection();
            return;
        }
    }

    public void MaybeUpdatePatrolPoints()
    {
        float distance = Vector2.Distance(_patrolPointLeft, _patrolPointRight);
        var yChange = (SpriteFinder.GetColliderCenterHeadPosition(_character.GetCharacterCollider()).y - _patrolPointLeft.y);
        if (yChange > 10f || yChange < -10f)
        {
            //if the collider is on enough of a slope,
            // then recalculate their patrol points until they are on flat ground
            SetPatrolPoints();
        }
        else if (SpriteFinder.IsEnemyRunningIntoTheGround(_character.GetCharacterCollider(), _character.IsCharacterFacingRight()))
        {
            //if there is wall or ground in front of them, go the other way
            SetPatrolPoints();
        }
        else if (distance < 20)
        {
            //if total patrol path length is too short, try again
            //todo add stalling behavior e.g. if they're stuck let them stay in idle
            SetPatrolPoints();
        }
    }

    public void SetPatrolPoints(bool checkTopAndBottom=false)
    {
        //Set left patrol point
        float leftPatrolPointX = _character.GetCharacterCollider().bounds.center.x - patrolDistance;
        _patrolPointLeft = SetPatrolPoint(leftPatrolPointX, checkTopAndBottom);
        
        //Set right patrol point 
        float rightPatrolPointX = _character.GetCharacterCollider().bounds.center.x + patrolDistance;
        _patrolPointRight = SetPatrolPoint(rightPatrolPointX, checkTopAndBottom);

    }

    private Vector2 SetPatrolPoint(float xPoint, bool checkTopAndBottom)
    {
        //Patrol point should be at the height of the character's head to avoid floating collisions
        float yPoint = _character.GetCharacterCenterHeadPosition().y;
        //Set patrol point based on given x and the top of the character's collider
        Vector2 patrolPointToSet = new Vector2(
            xPoint, 
            yPoint);
        //Check if collision is detected in projected patrol path
        Collider2D colliderInWayOfPatrolPoint;
        if (checkTopAndBottom)
        {
            colliderInWayOfPatrolPoint = SpriteFinder
                .ScoutAroundCollider( _character.GetCharacterCollider(), xPoint, false);
        }
        else
        {
            colliderInWayOfPatrolPoint = SpriteFinder
                .ScoutAroundColliderTop( _character.GetCharacterCollider(), xPoint, false);
        }
        //If a collider is in the path, adjust patrol point to stop there
        if (colliderInWayOfPatrolPoint != null)
        {
            var closestXPoint = colliderInWayOfPatrolPoint
                .ClosestPoint(patrolPointToSet).x;
            patrolPointToSet = new Vector2(closestXPoint,  yPoint);
        }
        return patrolPointToSet;
    }

    private bool ShouldIgnoreCharacter()
    {
        return _character.IsCharacterDocileOrNeutral();
    }

    public bool SwapPatrolDirection()
    {
        if (!_patrolPause)
        {
            //only change the patrol direction if they are not on a patrol pause
            _patrolPause = true;
            patrolRight = !patrolRight;
        }
        return patrolRight;
    }

    public bool PatrolWait()
    {
        if (_patrolPause)
        {
            _patrolWaitTimer += Time.deltaTime;
            if (_patrolWaitTimer < patrolPauseDuration)
            {
                _animator.SetBool(Constants.Moving, false);
                rb2d.velocity = new Vector2(0, rb2d.velocity.y);
                return true;
            }
            else
            {
                _patrolPause = false;
                _patrolWaitTimer = 0;
                return false;
            }
        }
        return false;
    }
    
    public bool PatrolRight()
    {
        return patrolRight;
    }

    public Vector2 PatrolPointRight()
    {
        return _patrolPointRight;
    }

    public Vector2 PatrolPointLeft()
    {
        return _patrolPointLeft;
    }
    
}