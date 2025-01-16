using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace SupportScripts
{
    public class Move : Task
    {
        private State _walkState;
        private State _runState;
        private bool _canRun;
        private float _speed;
        protected float nextWaypointDistance = 3f;
        protected int currentWayPoint = 0;
        protected bool reachedEndOfPath = false;
        private Seeker seeker;
        protected Path path;
        private Rigidbody2D _rb2d;

        public Move(Animator animator, Character character) : base(animator, character)
        {
            seeker = character.GetComponent<Seeker>();
            _rb2d = character.GetComponent<Rigidbody2D>();
            _walkState = new State(
                GetAnimator(),
                Constants.StateName.Walk, 
                Constants.Moving);
            _runState = new State(
                GetAnimator(),
                Constants.StateName.Run, 
                Constants.MovingFast);
        }
        
        public override List<State> InitializeStates()
        {
            return new List<State>()
            {
                _walkState,
                _runState
            };
        }

        public override void TaskSwitchBehavior()
        {
            _walkState.DisableState();
            _runState.DisableState();
            _rb2d.velocity = new Vector2(0, 0);
        }

        public override State DetermineState()
        {
            if (!_canRun)
            {
                _speed = _character.GetWalkSpeed();
                return _walkState;
            }
            _speed = _character.GetRunSpeed();
            return _runState;
        }

        //Core Task method to trigger State behavior
        public override void HandleStates(Vector2 goalPosition)
        {
            //Update the current goalPosition
            _goalPosition = goalPosition;
            
            //Determine path to goal position
            UpdatePath();
            
            //Move character along path towards goal position
            MoveCharacter();
        }

        public override Constants.TaskName GetTaskName()
        {
            return Constants.TaskName.Move;
        }

        private void UpdatePath()
        {
            if(seeker.IsDone())
            {
                seeker.StartPath(
                    _character.GetCharacterUpperFacingPosition(),
                    _goalPosition,
                    OnPathComplete);
            }
        }

        private void OnPathComplete(Path p)
        {
            if (!p.error)
            {
                path = p;
                currentWayPoint = 0;
            }
        }

        private Vector2 CharacterFacePosition()
        {
            return SpriteFinder.GetColliderFacingCenterPosition(
                _character.GetCharacterCollider(),
                _character.IsCharacterFacingRight());
        }

        private void MoveCharacter()
        {
            if (path == null)
            {
                return;
            }
            try
            {
                _walkState.EnableState();
                if (_canRun)
                {
                    _runState.EnableState();
                }
                else
                {
                    _runState.DisableState();
                }
                UpdateDirection();
                var currentWayPointOnPath = path.vectorPath[currentWayPoint];
                float distance = Vector2.Distance(_character.GetCharacterCollider().ClosestPoint(currentWayPointOnPath), currentWayPointOnPath);
                Debug.DrawLine(_character.GetCharacterCollider().ClosestPoint(currentWayPointOnPath), currentWayPointOnPath, Color.cyan);

                if (distance < nextWaypointDistance)
                {
                    currentWayPoint++;
                    currentWayPointOnPath = path.vectorPath[currentWayPoint];
                }
                Vector2 direction = ((Vector2)currentWayPointOnPath - _character.GetCharacterUpperFacingPosition()).normalized;
                _rb2d.velocity = new Vector2(direction.x * _speed, _rb2d.velocity.y);

                
            }
            catch (ArgumentOutOfRangeException e)
            { // Suppressing ArgumentOutOfRangeException that occurs when currentWayPoint index is out of bounds (on patrol point path complete)
                Debug.Log("ArgumentOutOfRangeException occurred in Sorcerer -- Move()");
            }
            catch (Exception e)
            {
                Debug.Log("Exception occurred in "+_character.name+" -- Move()");
                throw e;
            }
        }

        public void SetCanRun(bool canRun)
        {
            _canRun = canRun;
        }

        public bool GetCanRun()
        {
            return _canRun;
        }

        private void UpdateDirection()
        {
            bool isFacingTheGoalPosition = SpriteFinder
                .IsColliderFacingXPosition(
                    _character.GetCharacterCollider(),
                    _character.IsCharacterFacingRight(),
                    _goalPosition.x);

            if (!isFacingTheGoalPosition)
            {
                _character.transform.Rotate(0f, 180f, 0f);
                _character.SetCharacterFacingRight(!_character.IsCharacterFacingRight());
            }
        }
    }
}