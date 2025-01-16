using UnityEngine;

namespace SupportScripts
{
    /**
     * State class defines a single state that an enemy can be in.
     * This aligns to a specific animation.
     * The animation may either be triggered or enabled/disabled.
     * States are defined in and owned by individual tasks
     * and would typically be used in the owning task's "HandleStates()" method
     */
    public class State
    {
        private Constants.StateName _stateName;
        private int _animatorStateID;
        private Animator _animator;

        public State(Animator animator, Constants.StateName stateName, int animatorStateID)
        {
            _animator = animator;
            _stateName = stateName;
            _animatorStateID = animatorStateID;

        }
        
        public  int AnimatorTriggerInt()
        {
            return _animatorStateID;
        }

        public bool IsAnimationInProgress()
        {
            return _animator.GetCurrentAnimatorStateInfo(0)
                .IsName(_stateName.ToString());
        }

        public void TriggerState()
        {
            _animator.SetTrigger(_animatorStateID);
        }
        
        public void EnableState()
        {
            _animator.SetBool(_animatorStateID,true);
        }

        public void DisableState()
        {
            _animator.SetBool(_animatorStateID,false);
        }

        public string ToString()
        {
            return _stateName.ToString();
        }

        protected internal Constants.StateName GetStateName()
        {
            return _stateName;
        }
    }
}