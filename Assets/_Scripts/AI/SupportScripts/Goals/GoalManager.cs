using System;
using System.Collections.Generic;
using System.Linq;
using SupportScripts;
using SupportScripts.Goals;
using UnityEngine;

/**
 * This class creates a LinkedList of Goal classes called myGoals.
 * <p>
 * Goals will be treated in a LIFO manner,
 * the first node is the least prioritized goal for the enemy,
 * and the last node in the linked list is the most important goal.
 * </p>
 */
public class GoalManager: MonoBehaviour
{
    [SerializeField] protected Goal activeGoal;
    [SerializeField] protected Vector2 goalPosition;
    [SerializeField] protected Constants.TaskName activeTask;
    [SerializeField] protected Constants.StateName activeState;
    [SerializeField] protected LinkedList<Goal> myGoals;
    protected Animator _animator;
    protected Character _character;
    private Dictionary<Constants.TaskName, Task> taskPool;
    
    /**
     * AbstractEnemy's SharedStart calls this method to
     * initialize the core info and ordered list of goals.
     * It is called once per enemy, after they are added to the game scene.
     */
    public void InitializeGoalManager(Animator animator, Character character, Constants.GoalType[] defaultGoals)
    {
        //initialize mindfog animator separately as it is a sub-object of the enemy
        var mindFogAnimator = transform.Find("MindFog").GetComponent<Animator>();
        
        //each enemy has one generic task pool that the different goals can use
        taskPool = new Dictionary<Constants.TaskName, Task>
        {
            {Constants.TaskName.Idle, new Idle(animator, character)},
            {Constants.TaskName.Move, new Move(animator, character)},
            {Constants.TaskName.Vanish, new Vanish(animator, character)},
            {Constants.TaskName.Attack, new Attack(animator, character)},
            {Constants.TaskName.RangedAttack, new RangedAttack(animator, character)},
            {Constants.TaskName.Block, new Block(animator, character)},
            {Constants.TaskName.Heal, new Heal(animator, character)},
            {Constants.TaskName.MovementOverride, new MovementOverride(animator, character)},
            {Constants.TaskName.Die, new Death(animator, character)},
            {Constants.TaskName.MindFog, new MindFogEnabled(mindFogAnimator, character)}
        };
        
        //create the linked list of goals to manage
        myGoals = new LinkedList<Goal>();
        var gameObject = animator.gameObject;
        //populate the linked list based on the passed in (ordered) default goal list
        foreach (var goalType in defaultGoals)
        {
            //Instantiate each goal before adding them to the list
            switch (goalType)
            {
                case Constants.GoalType.Patrol:
                    Patrol patrol = gameObject.AddComponent<Patrol>();
                    patrol.InitializeGoal(Constants.GoalType.Patrol, animator, character, taskPool);
                    activeGoal = patrol;
                    AddGoal(patrol);
                    continue;
                case Constants.GoalType.Flee:
                    Flee fleeGoal = gameObject.AddComponent<Flee>();
                    fleeGoal.InitializeGoal(Constants.GoalType.Flee, animator, character, taskPool);
                    AddGoal(fleeGoal);
                    continue;
                case Constants.GoalType.ChaseTarget:
                    ChaseTarget chaseTargetGoal = gameObject.AddComponent<ChaseTarget>();
                    chaseTargetGoal.InitializeGoal(Constants.GoalType.ChaseTarget, animator, character, taskPool);
                    AddGoal(chaseTargetGoal);
                    continue;
                case Constants.GoalType.MindFog:
                    var mindFogGameObject = transform.Find("MindFog")
                        .gameObject;
                    MindFog mindFog = mindFogGameObject
                        .AddComponent<MindFog>();
                    mindFog.InitializeGoal(Constants.GoalType.MindFog, mindFogAnimator, character, taskPool);
                    AddGoal(mindFog);
                    //todo should this be added dynamically?
                    continue;
                case Constants.GoalType.Die: 
                    Die death = gameObject.AddComponent<Die>();
                    death.InitializeGoal(Constants.GoalType.Die, animator, character, taskPool);
                    AddGoal(death);
                    //todo should this be added dynamically?
                    continue;
                default:
                    //throw new NotImplementedException("Goal Type Not Recognized: "+goalType);
                    continue;
                
            }
        }
    }

    /**
     * This is called from each AbstractEnemy's SharedUpdate method,
     * so it is called every frame.
     */
    public void ProcessGoals()
    {
        //From END of linked list, check which goal is available
        activeGoal = GetBestAvailableGoal();
        
        //Process the selected goal and process its tasks
        activeGoal.ProcessTasks();
       
        //Track info about the goal for debugging and demo
        goalPosition = activeGoal.GetGoalPosition();
        activeTask = activeGoal.GetCurrentTask().GetTaskName();
        activeState = activeGoal.GetCurrentTask().GetCurrentStateName();
    }

    /**
     * This will find which item from the end of the list is available
     * and it will disable the current goal if it has changed.
     */
    private Goal GetBestAvailableGoal()
    {
        Goal mostRelevantGoal = GetAvailableGoal(myGoals.Last);
        if (mostRelevantGoal != activeGoal)
        {
            activeGoal.DisableGoalBehavior();
        }
        return mostRelevantGoal;
    }

    /**
     * This will traverse the linked list in reverse order recursively
     * until an available goal is found.
     * Goal availability is defined per goal.
     */
    private Goal GetAvailableGoal(LinkedListNode<Goal> goalToCheck)
    {
        //recursive call to find goal that is available, traversing backwards
        if (goalToCheck.Value.IsGoalAvailable())
        {
            //if the goal is available, return that value
            return goalToCheck.Value;
        }
        //todo add a check if current node is first? but patrol is always on anyway..
        //if the goal is not available, check this method again with the previous value
        return GetAvailableGoal(goalToCheck.Previous);
    }

    /**
     * Adds a goal to the end of the linked list
     */
    private void AddGoal(Goal goalToAdd)
    {
        if (myGoals == null)
        {
            myGoals.AddFirst(goalToAdd);
        }
        else
        {
            myGoals.AddLast(goalToAdd);
        }
    }

    /**
     *************** PUBLIC ACCESS CODE FOR ENEMIES *************** 
    **/
    
    public Collider2D MaybeGetTargetCollider()
    {
        if (activeGoal.GetGoalType() == Constants.GoalType.ChaseTarget)
        {
            return ((ChaseTarget)activeGoal).GetCurrentTargetCollider();
        } 
        else if (activeGoal.GetGoalType() == Constants.GoalType.MindFog)
        {
            return ((MindFog)activeGoal).GetCurrentTargetCollider();
        }

        return null;
    }

    public Health MaybeGetTargetHealth()
    {
        if (activeGoal.GetGoalType() == Constants.GoalType.ChaseTarget)
        {
            return ((ChaseTarget)activeGoal).GetCurrentTargetHealth();
        } 
        else if (activeGoal.GetGoalType() == Constants.GoalType.MindFog)
        {
            return ((MindFog)activeGoal).GetCurrentTargetHealth();
        }

        return null;
    }

    public Character MaybeGetTargetCharacter()
    {
        if (activeGoal.GetGoalType() == Constants.GoalType.ChaseTarget)
        {
            return ((ChaseTarget)activeGoal).GetCurrentTargetCharacter();
        } 
        else if (activeGoal.GetGoalType() == Constants.GoalType.MindFog)
        {
            return ((MindFog)activeGoal).GetCurrentTargetCharacter();
        }

        return null;
    }

    public Transform MaybeGetTargetTransform()
    {
        if (activeGoal.GetGoalType() == Constants.GoalType.ChaseTarget)
        {
            return ((ChaseTarget)activeGoal).GetCurrentTarget();
        } 
        else if (activeGoal.GetGoalType() == Constants.GoalType.MindFog)
        {
            return ((MindFog)activeGoal).GetCurrentTarget();
        }

        return null;
    }

}