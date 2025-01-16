using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using SupportScripts;
using UnityEngine;

public class ChaseTarget : Goal
{
    [Header("Target Info")]
    [SerializeField] private Transform currentTarget;
    [Header("Decision Info")]
    [SerializeField] private bool inRangeForMeleeAttack;
    [SerializeField] private bool inRangeForDistanceAttack;
    [SerializeField] private Transform playerSpriteTransform;
    [SerializeField] private Transform playerTransform;
    
    protected override void Initialize()
    {
        //Default initialization of the goal
        playerSpriteTransform = GameObject.Find("Sprite").transform;
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
    }

    protected override List<Constants.TaskName> DefaultGoalTaskNames()
    {
        //Chase Target will consist of all the tasks needed
        // to locate, follow and engage a target willingly,
        // with the exception of movement overrides
        return new List<Constants.TaskName>
        {
            Constants.TaskName.Idle,
            Constants.TaskName.Move,
            Constants.TaskName.Attack,
            Constants.TaskName.RangedAttack,
            Constants.TaskName.Heal,
            Constants.TaskName.Block,
            Constants.TaskName.MovementOverride
        };
    }
    
    protected override bool CheckIfGoalIsAvailable()
    {
        //Update current target position, if any
        currentTarget = GetCurrentTargetTransform();
        //Only aggressive characters should have targets, 
        // and goal should only activate if a suitable target is near
        return _character.IsCharacterAggressive() && HasTargetInRange();
    }
    
    protected override Vector2 CalculateGoalPosition()
    {
        //goal position is the closest point on the target collider
        // to the enemy's attack point
        return GetCurrentTargetCollider()
            .ClosestPoint(enemy.GetAttackPointPosition());
    }

    protected override void CalculateTaskPriorities()
    {
        //Determine if which types of attack are available, if any
        inRangeForMeleeAttack = CloseEnoughForMeleeAttack();
        inRangeForDistanceAttack = CloseEnoughForRangedAttack();
        ResetTaskScores();
        //Always give movement override highest priority
        if (_character.HasMovementOverride()){
            GetAvailableTasks()[Constants.TaskName.MovementOverride].SetPriorityScore(9);
        }
        
        //blocking attacks is the highest combat priority
        else if (ShouldBlock())
        {
            GetAvailableTasks()[Constants.TaskName.Block].SetPriorityScore(8);
        } 
        //if character can heal, prioritize that
        else if (ShouldHeal())
        {
            GetAvailableTasks()[Constants.TaskName.Heal].SetPriorityScore(7);
        }
        //if close enough for a physical attack, do it
        else if (inRangeForMeleeAttack)
        {
            GetAvailableTasks()[Constants.TaskName.Attack].SetPriorityScore(6);
        }
        //if close enough for a ranged attack, do it
        else if (enemy.HasRangedAttack() && inRangeForDistanceAttack) //can check if on cooldown to factor this in
        {
            GetAvailableTasks()[Constants.TaskName.RangedAttack].SetPriorityScore(5);
        }
        //not close enough to attack, so chase them
        else if (HasTargetInRange() && !ShouldStopRunningTowardsTarget())
        {
            GetAvailableTasks()[Constants.TaskName.Move].SetPriorityScore(5);
            ((Move) GetAvailableTasks()[Constants.TaskName.Move])
                .SetCanRun(true);
        }
        else
        {
            GetAvailableTasks()[Constants.TaskName.Idle].SetPriorityScore(1);
        }
    }

    protected override void GoalSwitchBehavior()
    {
        base.GoalSwitchBehavior();
    }

    private void ResetTaskScores()
    {
        foreach (var task in GetAvailableTasks().Values)
        {
            task.ResetTaskScore();
        }
    }
    
    private bool HasTargetInRange(float buffer=0)
    {
        //this is checking strictly the distance from the target to this character's attack point
        //if a buffer is provided, the check will ignore distances between 0 and the buffer
        return IsTargetWithinAggroDistance(buffer);
    }

    private bool ShouldStopRunningTowardsTarget()
    {
        //if the character doesn't have a ranged attack, and the target is above them
        if (!enemy.HasRangedAttack() 
            && SpriteFinder.IsEnemyRunningTowardsAnUnreachableTarget(
                GetCurrentTargetCollider(),
                _character.GetCharacterCollider()))
        {
            return true;
        }
        //if the character is blocked by another character (when facing the goal)
        if (SpriteFinder.IsEnemyRunningIntoAnotherCharacter(
                _character.GetCharacterCollider(),
                _character.IsCharacterFacingRight(),
                true,
                true)
            && SpriteFinder.IsColliderFacingXPosition(
                _character.GetCharacterCollider(),
                _character.IsCharacterFacingRight(),
                goalPosition.x))
        {
            return true;
        }

        return false;
    }

    private float DistanceFromTargetToAttackPoint()
    {
        float targetNearestXPosition = GetCurrentTargetCollider()
            .ClosestPoint(enemy.GetAttackPointPosition()).x;
        return Math.Abs(targetNearestXPosition - enemy.GetAttackPointPosition().x);
    }

    protected bool IsTargetWithinAggroDistance(float buffer)
    {
        if (GetCurrentTargetCollider() == null)
        {
            return false;
        }
        float distance = Vector2.Distance(
            GetCurrentTargetCollider().ClosestPoint(enemy.GetAttackPointPosition()), 
            enemy.GetAttackPointPosition());
        if (distance < enemy.GetAggroDistance())
        {
            if (buffer != 0)
            {
                return distance > buffer;
            }
            return true;
        }
        return false;
    }
    
    public Collider2D GetCurrentTargetCollider()
    {
        if (currentTarget == null)
        {
            return null;
        }
        if (IsPlayer(currentTarget.name))
        {
            return playerSpriteTransform.GetComponent<BoxCollider2D>();
        } 
        return currentTarget.GetComponent<BoxCollider2D>();
    }

    protected Collider2D GetCurrentTargetHitBox()
    {
        //todo fix for player?
        return currentTarget.Find("HitBox").GetComponent<BoxCollider2D>();
    }

    public Health GetCurrentTargetHealth()
    {
        if (IsPlayer(currentTarget.name))
        {
            return playerTransform.GetComponent<Health>();
        } 
        return currentTarget.GetComponent<Health>();
    }

    public Character GetCurrentTargetCharacter()
    {
        if (IsPlayer(currentTarget.name))
        {
            return playerTransform.GetComponent<Character>();
        } 
        return currentTarget.GetComponent<Character>();
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    protected String GetCurrentTargetName()
    {
        return currentTarget.name;
    }

    private bool IsPlayer(string name)
    { 
        return name == "Player" || name == "Sprite";
    }
    
    protected Transform GetCurrentTargetTransform()
    {
        
        //If character has been hit, attack whoever hit them
        if (health.aggressorCharacter != null 
            && !health.aggressorCharacter.GetComponent<Health>().dead)
        {
            return health.aggressorCharacter.GetComponent<Transform>();
        }
        //If a character is an aggressive animal,
        // then attack anything that comes close if not same (e.g wolf pack)
        else if ( enemy.IsAnimalEnemy(transform.name)
                  && SpriteFinder.HasNearbyCharacter(
                      _character.GetCharacterCollider(), 
                      enemy.GetAggroDistance()))
        {
            return SpriteFinder
                .GetNearestCharacterCollider(
                    _character.GetCharacterCollider(), 
                    enemy.GetAggroDistance())
                .GetComponentInParent<Transform>();
        }
        //humanoid characters will currentTarget the player
        else if ( enemy.IsHumanoidEnemy(transform.name) 
                  && IsPositionWithinDistance(
                      playerSpriteTransform.position,
                      enemy.GetAttackPointPosition(), 
                      enemy.GetAggroDistance()) )
        { 
            return currentTarget;
        }
        else
        {
            return playerSpriteTransform;
        }
    }

    private bool IsPositionWithinDistance(Vector2 scoutPosition, Vector2 positionToCheck, float maxDistanceToCheck)
    {
        float distance = Vector2.Distance(
            scoutPosition, 
            positionToCheck);
        if (distance < maxDistanceToCheck)
        {
            return true;
        }
        return false;
    }
    
    
    /**
     * This will indicate that the enemy should attack based on their attack point position
     * or if the target is between the enemy collider and their attack point
     * This should be used to gauge closeness for attack point combat melee purposes only.. Might be refactored?
     */
    protected bool CloseEnoughForMeleeAttack(bool checkYPosition = true)
    {
        if (IsCurrentTargetInFrontOfMe()) 
        {//this checks between character and attack point only
            return true;
        }

        if (checkYPosition)
        {
            // Check if target's collider is in the vertical space to attack
            // (if it overlaps owning character's attack point Y position)
            var targetIsWithinYRange =
                GetCurrentTargetCollider()
                    .OverlapPoint(new Vector2(
                        GetCurrentTargetCollider().bounds.center.x, 
                        enemy.GetAttackPointPosition().y));
            if (!targetIsWithinYRange)
            {
                return false;
            }
        }
        
        float targetNearestXPosition = GetCurrentTargetCollider()
            .ClosestPoint(enemy.GetAttackPointPosition()).x;
        float distance = Math.Abs(targetNearestXPosition - enemy.GetAttackPointPosition().x);

        Debug.DrawLine( enemy.GetAttackPointPosition(),
            new Vector2(enemy.GetAttackPointPosition().x,  enemy.GetAttackPointPosition().y), Color.white);
        if (distance < enemy.GetAttackPointRange())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool IsCurrentTargetInFrontOfMe()
    {
        Collider2D colliderInFrontOfMe = SpriteFinder.ScoutAroundCollider(
            _character.GetCharacterCollider(), enemy.GetAttackPointPosition().x);
        if (colliderInFrontOfMe != null && colliderInFrontOfMe == GetCurrentTargetCollider() )
        {
            return true;
        }
        return false;
    }

    protected bool CloseEnoughForRangedAttack()
    {
        if (!enemy.HasRangedAttack() || IsCurrentTargetInFrontOfMe() )
        {
            //Not in range if target doesn't have a ranegd attack or is too close or is not grounded
            return false;
        }
        Debug.DrawLine(GetCurrentTargetCollider()
            .ClosestPoint(
                enemy.GetAttackPointPosition()),enemy.GetAttackPointPosition(), Color.white);
        
        float distance = Math.Abs(
            Vector2.Distance(
                GetCurrentTargetCollider()
                    .ClosestPoint(enemy.GetBoxColliderCenterPoint()), enemy.GetBoxColliderCenterPoint()));

        var rangedAttackMinDistance = enemy.GetRangedAttackMinDistance();
        var rangedAttackMaxDistance = enemy.GetRangedAttackMaxDistance();
        //if target is close, but inaccessible for a normal attack
        //or if target is within normal cast range, then cast
        if (distance >= rangedAttackMinDistance && distance <= rangedAttackMinDistance+rangedAttackMaxDistance) 
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool ShouldBlock()
    {
        if (!_character.GetAllowBlocking())
        {
            return false;
        }
        if (_character.IsDefenceWarningActivated() 
            && !GetTaskByName(Constants.TaskName.Block).IsOnCooldown())
        {
            return true;
        }
        if (GetTaskByName(Constants.TaskName.Block).IsAnimationInProgress())
        {
            return true;
        }
        return false;
    }
    
    public bool ShouldHeal()
    {
        if (!_character.GetCanHeal())
        {
            return false;
        }
        if (GetTaskByName(Constants.TaskName.Heal).IsAnimationInProgress())
        {
            return true;
        }
        if (GetTaskByName(Constants.TaskName.Heal).IsOnCooldown())
        {
            return false;
        }
        return health.currentHealth <= health.healthLevelRequiredToHeal;
    }
}