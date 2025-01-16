using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] protected AbstractEnemy enemy;
    [SerializeField] protected GameObject itemDrop;
    public ManaBar manaBar;

    protected Animator animator;
    protected Rigidbody2D rb2d;
    protected BoxCollider2D collider;
    
    public delegate void EnemyDeath();
    public static event EnemyDeath OnEnemyDeath;
    
    
    protected void Awake()
    {
        animator = GetComponent<Animator>();
        currentHealth = startingHealth;
        rb2d = enemy.GetComponent<Rigidbody2D>();
        collider = enemy.GetComponent<BoxCollider2D>();
        character = enemy.GetComponent<Character>();
    }

    override protected void Hurt()
    {
        character.SetMovementOverride(Constants.EnemyMovementOverride.Hurt);
    }

    override protected void Death()
    {
        manaBar = GameObject.Find("ManaBar").GetComponent<ManaBar>();
        //Give player appropriate amount of Mana
        manaBar.RegenMana(enemy.manaRegenAmount);
        
        //generate a health potion where they died
        Instantiate(Resources.Load("_Prefabs/Other/healthpotion"), 
            enemy.GetBoxColliderCenterPoint(), Quaternion.Euler(0, 0, 0));
        Debug.Log(enemy.name+" has been killed");
    }
    
    override public void DisableCompletely()
    {
        character.UnFreezeRigidbody();
        character.ResetCharacterColor();
        //Loop through and disable any children colliders
        var boxColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D attachedCollider in boxColliders)
        {
            if (!SpriteFinder.IsSpear(attachedCollider.name))
            {
                attachedCollider.enabled = false;
            }
            else if (SpriteFinder.IsSpearTip(attachedCollider.name))
            {
                //attachedCollider.GetComponent<SpearTip>().DropSpear();
            }
        }
        //disable enemy entirely
        collider.enabled = false;
        rb2d.gravityScale = 0;
        rb2d.velocity = new Vector2(0,0);
        enemy.enabled = false;
    }
    

    public override void Knockback()
    {
        Debug.Log(rb2d.name + " Knockback Method Entered");
        character.SetMovementOverride(Constants.EnemyMovementOverride.KnockBack);
    }
    
    public override void FreezeCharacter(float damage)
    {
        Debug.Log(rb2d.name+" FreezeCharacter Method Entered");
        character.SetMovementOverride(Constants.EnemyMovementOverride.Freeze);
    }
    
    public override void ClearFreeze(float damage = 0)
    {
        Debug.Log(rb2d.name+" ClearFreeze Method Entered");
        character.SetMovementOverride(Constants.EnemyMovementOverride.None);
    }
}
