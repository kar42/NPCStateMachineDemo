using System;
using System.Collections.Generic;
using System.Linq;
using SupportScripts;
using UnityEngine;

/**
 * Goal class defines the core behavior of the behavioral driven functionality.
 * It will guide the AI Enemy Characters to carry out a primary directive.
 * It defines a group of tasks that can work together to achieve any given primary directive.
 * <p>Each Update: </p>
 * The goal position for path finding will be re-calculated.
 * It will calculate the priority of each available task,
 * and will act on the task with the highest PriorityScore.
 */
public abstract class Goal : MonoBehaviour
{
    [Header("GoalDrivenBehavior")] 
    [SerializeField] protected Constants.GoalType goalName;
    [SerializeField] protected bool isGoalAvailable = false;
    [SerializeField] protected Vector2 goalPosition;
    [SerializeField] protected Character _character;
    protected Constants.TaskName currentTaskName;
    protected Constants.StateName currentStateName;
    protected Dictionary<Constants.TaskName, Task> goalTaskDictionary;
    private List<Constants.TaskName> defaultGoalTaskNames;
    private BoxCollider2D boxCollider;
    protected Animator _animator;
    protected Rigidbody2D rb2d;
    protected AbstractEnemy enemy;
    protected SpriteRenderer spriteRenderer;
    protected EnemyHealth health;
    private Dictionary<Constants.TaskName, Task> _availableTasks;

    abstract protected void Initialize();
    abstract protected List<Constants.TaskName> DefaultGoalTaskNames();
    abstract protected bool CheckIfGoalIsAvailable();
    abstract protected Vector2 CalculateGoalPosition();
    abstract protected void CalculateTaskPriorities();

    /**
     * Initializes the important components for the function of goal behavior.
     * This is called from the goal initialization inside GoalManager.
     */
    public void InitializeGoal(
        Constants.GoalType goalType, 
        Animator animator, 
        Character character,
        Dictionary<Constants.TaskName, Task> taskPool)
    {
        //stores what kind of goal it is (based on Constants.GoalType)
        goalName = goalType;
        
        //store and filter the task pool for the goal
        _availableTasks = taskPool;
        defaultGoalTaskNames = DefaultGoalTaskNames();
        goalTaskDictionary = FilteredGoalTasks();
        
        //store enemy info
        enemy = GetComponent<AbstractEnemy>();
        boxCollider = GetComponent<BoxCollider2D>();
        _animator = animator;
        rb2d = GetComponent<Rigidbody2D>();
        _character = GetComponent<Character>();
        health = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        //Goal specific initialization and availability check
        Initialize();
        IsGoalAvailable();
    }

    /**
     * Updates and checks for the selected task, based on task specific priority scores
     */
    public void ProcessTasks()
    {
        //increment task cooldowns
        IncrementTaskCooldowns();
        
        //determine where enemy should be to perform goal
        goalPosition = CalculateGoalPosition();
        
        //determine which task for the goal has the highest priority
        CalculateTaskPriorities();
        
        //if any change in task occurred, handle as needed
        HandleTaskChange(GetCurrentTask().GetTaskName());
        
        // task with highest priority score will be performed
        GetCurrentTask().PerformTask(goalPosition);
        
        //track current state name
        currentStateName = GetCurrentTask().GetCurrentStateName();

    }

    private void IncrementTaskCooldowns()
    {
        //if task requires a cooldown, it will be updated here
        foreach (var task in goalTaskDictionary.Values)
        {
            task.IncrementTaskCooldown();
        }
    }

    /**
     * What kind of goal it is (types are defined by Constants.GoalType)
     */
    public virtual Constants.GoalType GetGoalType()
    {
        return goalName;
    }

    /**
     * All tasks available in the task pool, mapped by their name
     */
    protected Dictionary<Constants.TaskName, Task> GetAvailableTasks()
    {
        return _availableTasks;
    }

    /**
     * Returns the highest prioritized task out of tasks defined for the goal
     */
    public Task GetCurrentTask()
    {
        return goalTaskDictionary.Values
            .OrderByDescending(t =>
                t.GetPriorityScore())
            .FirstOrDefault();
    }

    /**
     * Check for a task in the goal-defined task pool by name
     */
    public Task GetTaskByName(Constants.TaskName taskName)
    {
        return goalTaskDictionary[taskName];
    }

    /**
     * Filters the full task pool to only those 
     */
    public Dictionary<Constants.TaskName, Task> FilteredGoalTasks()
    {
        return _availableTasks
            .Where(taskMapPair => defaultGoalTaskNames.Contains(taskMapPair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    /**
     * call configuration to calculate if goal is available
     * and set local variable for tracking and debugging
     */
    public bool IsGoalAvailable()
    {
        return isGoalAvailable = CheckIfGoalIsAvailable();
    }

    /*
     * If a goal has changed, call this behavior to handle process changes
     */
    public void DisableGoalBehavior()
    {
        GoalSwitchBehavior();
    }

    /**
     * For the current goal task, handle switching behavior
     */
    virtual protected void GoalSwitchBehavior()
    {
        GetTaskByName(currentTaskName)
            .TaskSwitchBehavior();
    }

    /**
     * Checks and updates the current task based on a given task name.
     */
    protected void HandleTaskChange(Constants.TaskName newTaskName)
    {
        //If the given name is different from the current task
        if (currentTaskName != newTaskName)
        {
            //task switch behavior will be called before updating the current task
            GetTaskByName(currentTaskName)
                .TaskSwitchBehavior();
            currentTaskName = newTaskName;
        }
    }

    /**
     * Returns the calculated goal position
     */
    public Vector2 GetGoalPosition()
    {
        return goalPosition;
    }
}