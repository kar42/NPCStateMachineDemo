using UnityEngine;

public class ExpressionBox : MonoBehaviour
{
    [Header("ExpressionBox")]

    [SerializeField] private AbstractEnemy _owningEnemy;
    [SerializeField] private Constants.EnemyExpressions currentExpression = Constants.EnemyExpressions.None;
    [SerializeField] private bool displayHealth = false;
    [SerializeField] private bool enemyAlertTriggered = false;
    private Animator animator;
    private BoxCollider2D boxCollider; 
    private Health characterHealth;


    // Start is called before the first frame update
    private void Start()
    {
        characterHealth = GetComponentInParent<Health>();
        _owningEnemy = GetComponentInParent<AbstractEnemy>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponentInParent<BoxCollider2D>();
        currentExpression = Constants.EnemyExpressions.None;
        displayHealth = false;
    }

    // Update is called once per frame
    private void Update()
    {

        if (_owningEnemy != null)
        {
            if (_owningEnemy.IsEnemyDead())
            {
                DisableExpressionBoxContents();
                return;
            }
            switch (currentExpression)
            {
                case (Constants.EnemyExpressions.EnemyAlerted):
                    HandleEnemyAlertedExpression();
                    break;
                case (Constants.EnemyExpressions.DisplayHealth):
                    HandleDisplayHealthExpression();
                    break;
                case (Constants.EnemyExpressions.None):
                default:
                    UpdateEnemyExpressions();
                    break;
            }
        }
    }
    

    private void UpdateEnemyExpressions()
    {  
        if (_owningEnemy.HasTargetInRange() && !enemyAlertTriggered)
        {
            currentExpression = Constants.EnemyExpressions.EnemyAlerted;
        }
        else if (_owningEnemy.HasTargetInRange() && enemyAlertTriggered)
        {
            if (!IsEnemyAlertedAnimationPlaying())
            {
                currentExpression = Constants.EnemyExpressions.DisplayHealth;
                displayHealth = true;
            }
        }
        else
        {
            DisableExpressionBoxContents();
        }
    }

    private void HandleEnemyAlertedExpression()
    {  
        if (_owningEnemy.HasTargetInRange() && !enemyAlertTriggered)
        {
            TriggerEnemyAlertedAnimation();
            enemyAlertTriggered = true;
        }
        else
        {
            currentExpression = Constants.EnemyExpressions.None;
        }
    }

    private void TriggerEnemyAlertedAnimation()
    {
        animator.SetTrigger(Constants.EnemyAlerted);
    }

    private bool IsEnemyAlertedAnimationPlaying()
    {
        return animator.GetCurrentAnimatorStateInfo(0)
            .IsName("EnemyAlerted");
    }
    private void DisableExpressionBoxContents()
    {
        enemyAlertTriggered = false;
        displayHealth = false;
        animator.SetInteger("HealthBarNum", 0);
        currentExpression = Constants.EnemyExpressions.None;
    }
    
    private void HandleDisplayHealthExpression()
    {
        if (_owningEnemy.HasTargetInRange() && displayHealth)
        {
            int currentEnemyHealthScore = DetermineEnemyHealthNumber();
            animator.SetInteger("HealthBarNum", currentEnemyHealthScore);
        }
        if (!_owningEnemy.HasTargetInRange())
        {
            currentExpression = Constants.EnemyExpressions.None;
            displayHealth = false;
            animator.SetInteger("HealthBarNum", 0);
        }
        else
        {
            currentExpression = Constants.EnemyExpressions.None;
        }
    }

    private int DetermineEnemyHealthNumber()
    {
        float currentHealthPercent = characterHealth.currentHealth / characterHealth.startingHealth;
        if (currentHealthPercent > 0.75 && currentHealthPercent <= 1f)
        {
            return 1;
        }

        if (currentHealthPercent > 0.5f && currentHealthPercent < 0.75)
        {
            return 2;
        }

        if (currentHealthPercent > 0.25f && currentHealthPercent < 0.5f)
        {
            return 3;
        }

        if (currentHealthPercent > 0 && currentHealthPercent < 0.25f)
        {
            return 4;
        }

        if (characterHealth.IsDead() || currentHealthPercent <= 0)
        {
            return 5;
        }

        return 0;
    }
}