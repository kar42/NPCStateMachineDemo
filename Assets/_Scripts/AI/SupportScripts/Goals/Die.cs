using System.Collections.Generic;
using UnityEngine;

namespace SupportScripts.Goals
{
    public class Die: Goal
    {
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool deathCompleted = false;
        private Health _health;

        protected override void Initialize()
        {
            _health = gameObject.GetComponent<Health>();
        }

        protected override List<Constants.TaskName> DefaultGoalTaskNames()
        {
            return new List<Constants.TaskName>
            {
                Constants.TaskName.Die
            };
        }

        protected override bool CheckIfGoalIsAvailable()
        {
            return isDead = _health.IsDead();
        }

        protected override Vector2 CalculateGoalPosition()
        {
            return _character.GetCharacterCenterFootPosition();
        }

        protected override void CalculateTaskPriorities()
        {
            GetAvailableTasks()[Constants.TaskName.Die].SetPriorityScore(10);
            currentTaskName = Constants.TaskName.Die;
        }

        protected override void GoalSwitchBehavior()
        {
        }
    }
}