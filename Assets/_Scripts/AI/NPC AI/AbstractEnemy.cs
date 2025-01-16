using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class AbstractEnemy : MonoBehaviour
{
    [Header("Debug Settings")] [SerializeField]
    protected bool showDebugCode;

    [Header("Enemy Settings")] [SerializeField]
    private bool mindFogEnabled = false;
    [SerializeField] private Collider2D targetCollider;

    [Header("Characteristics")]
    [SerializeField] protected bool enemyEnabled = true;
    [SerializeField] protected float aggroDistance = 80f;
    [SerializeField] protected float attackDistance = 0f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackDamage = 1f;
    [SerializeField] protected bool hasRangedAttack = false;
    [SerializeField] protected float rangedAttackMinDistance = 30f;
    [SerializeField] protected float rangedAttackMaxDistance = 50f;

    [Header("Mana Regen")] [SerializeField]
    public int manaRegenAmount = 5;

    protected Character character;
    protected BoxCollider2D boxCollider;
    protected BoxCollider2D attackPointCollider;
    protected Animator animator;
    protected Rigidbody2D rb2d;
    protected EnemyHealth health;
    private GoalManager _myGoalManager;

    /**
     ***************MUST IMPLEMENT IN EACH ENEMY CLASS**********************************
    **/
    abstract public Vector2 GetAttackPointPosition();

    abstract public float GetAttackPointRange();
    abstract protected Collider2D GetAttackPointCollider();

    abstract protected void UpdateStart();
    abstract protected void UpdateFinish();

    public delegate void EnemyDeath();

    public static event Action<AbstractEnemy> OnEnemyDeath;
    private bool hasDied = false;

    private bool isDestructionCoroutineStarted = false;

    /**
     ***************SHARED INITIALIZATION & UPDATE CODE**********************************
    **/
    protected void SharedStart()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        health = gameObject.GetComponent<EnemyHealth>();
        character = gameObject.GetComponent<Character>();
        boxCollider = GetComponent<BoxCollider2D>();
        // Z axis is set to > 5000, when spawned this code resets it
        transform.position = new Vector2(transform.position.x, transform.position.y);

        //Goal Manager initialization
        _myGoalManager = gameObject.AddComponent<GoalManager>();
        Constants.GoalType[] defaultGoals;
        //The goals order below is important, last taking highest priority
        if (character.IsCharacterDocile())
        {
            //fox is docile and has different goals
            defaultGoals = new Constants.GoalType[]
            {
                Constants.GoalType.Patrol,
                Constants.GoalType.Flee,
                Constants.GoalType.MindFog,
                Constants.GoalType.Die
            };
        }
        else
        {
            //neutral (e.g. elk) and humanoid use these default goals
            defaultGoals = new Constants.GoalType[]
            {
                Constants.GoalType.Patrol,
                Constants.GoalType.ChaseTarget,
                Constants.GoalType.MindFog,
                Constants.GoalType.Die
            };
        }
        _myGoalManager.InitializeGoalManager(animator, character, defaultGoals);
    }

    protected void SharedUpdate()
    {
        if (health.IsDead() && !hasDied && IsHumanoidEnemy(transform.name))
        {
            //start timer to destroy game object
            hasDied = true;
            OnEnemyDeath?.Invoke(this);
            StartCoroutine(DestroyAfterDelay(20f));
            isDestructionCoroutineStarted = true;
        }

        // check if there are external factors causing an override to any goal driven behavior
        character.CheckForMovementOverride();

        //for tracking and debugging
        targetCollider = _myGoalManager.MaybeGetTargetCollider();

        //Every frame, check and act on enemy goals
        _myGoalManager.ProcessGoals();
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }


    /**
     *************** ENEMY SPECIFIC IDENTIFICATION **********************************
    **/
    protected bool IsEnemy(string name)
    {
        //overload this method to take collider and check layer?
        foreach (var namePattern in Constants.EnemyNamePatterns)
        {
            MatchCollection namePatternMatch = namePattern.Matches(name);
            if (namePatternMatch.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsAnimalEnemy(string name)
    {
        foreach (var namePattern in Constants.AnimalEnemyNamePatterns)
        {
            MatchCollection namePatternMatch = namePattern.Matches(name);
            if (namePatternMatch.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsHumanoidEnemy(string name)
    {
        foreach (var namePattern in Constants.HumanoidEnemyNamePatterns)
        {
            MatchCollection namePatternMatch = namePattern.Matches(name);
            if (namePatternMatch.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    /**
     ***************GENERAL STATUS**********************************
    **/
    public float EnemyHealth()
    {
        return health.currentHealth;
    }

    public bool IsEnemyDead()
    {
        return health.IsDead();
    }

    public Personality.EnemyPersonality GetCurrentDemeanor()
    {
        return character.CharacterCurrentDemeanor();
    }

    public void SetCurrentDemeanor(Personality.EnemyPersonality newPersonality)
    {
        character.SetCurrentDemeanor(newPersonality);
    }

    public Character EnemyCharacter()
    {
        return character;
    }

    public Vector2 GetBoxColliderCenterPoint()
    {
        return boxCollider.bounds.center;
    }

    public bool IsDebugEnabled()
    {
        return showDebugCode;
    }


    /**
     ***************AGGRO AND ATTACK SUPPORT CODE**********************************
    **/
    public float GetAggroDistance()
    {
        return aggroDistance;
    }

    //This is called by each enemy during their relevant attack animations (strike frame)
    protected virtual void DamageEnemy()
    {
        try
        {
            var targetHealth = _myGoalManager.MaybeGetTargetHealth();
            targetHealth.TakeDamage(attackDamage, character);
            _myGoalManager.MaybeGetTargetCharacter().SetDefenceWarning(true);
        }
        catch (Exception e)
        {
            Debug.Log("Exception occurred in " + character.name + " -- DamageEnemy()");
        }
        
    }

    protected Collider2D GetCurrentTargetCollider()
    {
        return _myGoalManager.MaybeGetTargetCollider();
    }

    public bool HasRangedAttack()
    {
        return hasRangedAttack;
    }

    public bool SetHasRangedAttack(bool newValue)
    {
        return hasRangedAttack = newValue;
    }

    public float GetRangedAttackMinDistance()
    {
        return rangedAttackMinDistance;
    }

    public float GetRangedAttackMaxDistance()
    {
        return rangedAttackMaxDistance;
    }


    /**
     ***************EXPRESSION BOX SUPPORT CODE**********************************
    **/
    //todo remove after updating expressionbox
    public bool HasTargetInRange()
    {
        if (!Personality.IsAggressive(GetCurrentDemeanor()))
        {
            return false;
        }

        return IsTargetWithinAggroDistance();
    }

    protected bool IsTargetWithinAggroDistance()
    {
        if (GetCurrentTargetCollider() == null)
        {
            return false;
        }

        float distance = Vector2.Distance(
            GetCurrentTargetCollider().ClosestPoint(GetAttackPointPosition()),
            GetAttackPointPosition());
        if (distance < aggroDistance)
        {
            return true;
        }

        return false;
    }


    /**
     *************** MIND FOG SUPPORT CODE**********************************
    **/
    public void EnableMindFog()
    {
        mindFogEnabled = true;
    }

    public void DisableMindFog()
    {
        mindFogEnabled = false;
    }

    public bool IsMindFogEnabled()
    {
        return mindFogEnabled;
    }
}