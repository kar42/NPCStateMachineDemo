using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts
{
    /**
     * Task class defines the individual tasks that can carry out small units of behavioral driven functionality.
     * It will instantiate and manage one or more states,
     * which can be used to trigger animations for the enemy.
     * <p>Each Update: </p>
     * The current state will be determined
     * The state behavior will be handled by each individual task (e.g. move, movement override)
     */
    public abstract class Task
    {
        private Constants.TaskName _taskName;
        protected List<State> _states;
        protected Animator _animator;
        protected Character _character;
        protected AbstractEnemy _enemy;
        protected Vector2 _goalPosition;
        protected float _taskCooldown = 20.0f;
        private int _priorityScore = 0;
        private int animatorTriggerInt;
        private State _currentState;
        private bool _animationInProgress;

        public Task(Animator animator, Character character)
        {
            _animator = animator;
            _character = character;
            _enemy = character.GetComponent<AbstractEnemy>();
        }

        public void InitializeTask(Constants.TaskName taskName)
        {
            _taskName = taskName;
            _states = InitializeStates();
        }
        
        public abstract List<State> InitializeStates();

        /**
         * This method will execute the task behavior
         */
        public void PerformTask(Vector2 goalPosition)
        {
            //Figure out which state to apply, e.g. running or walking
            _currentState = DetermineState();
            
            //track whether the state has it's animation in progress
            _animationInProgress = _currentState.IsAnimationInProgress();
            
            //Perform state behavior
            HandleStates(goalPosition);
        }
        
        public virtual void TaskSwitchBehavior()
        {
            //this method will be overridden as needed
            //to allow specific behavior to happen when tasks or goals are changed
        }
        
        /**
         * Each task will determine which state should be used,
         * if more than one might apply
         */
        public abstract State DetermineState();
        

        /**
         * Handle individual animation states (state machine)
         */
        public abstract void HandleStates(Vector2 goalPosition);

        virtual public void IncrementTaskCooldown()
        {
            _taskCooldown += Time.deltaTime;
        }

        protected void ResetTaskCooldown()
        {
            _taskCooldown = 0f;
        }

        virtual public bool IsOnCooldown()
        {
            return false;
        }

        public int GetPriorityScore()
        {
            return _priorityScore;
        }

        public void SetPriorityScore(int newPriorityScore)
        {
            _priorityScore = newPriorityScore;
        }

        public Constants.StateName GetCurrentStateName()
        {
            return _currentState.GetStateName();
        }

        public Animator GetAnimator()
        {
            return _animator;
        }

        public bool IsAnimationInProgress()
        {
            if (_currentState == null)
            {
                return false;
            }
            return _currentState.IsAnimationInProgress();
        }

        public Character GetCharacter()
        {
            return _character;
        }

        abstract public Constants.TaskName GetTaskName();

        public void ResetTaskScore()
        {
            SetPriorityScore(0);
        }

    }
}