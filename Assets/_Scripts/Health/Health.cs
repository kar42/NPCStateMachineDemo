using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;

public abstract class Health : MonoBehaviour
{
    
    [SerializeField] public float startingHealth;
    //Toggle this to allow all characters to take damage
    [SerializeField] protected bool allowDamage = true;
    public bool dead = false;
    public bool deathCompleted = false;
    public float currentHealth;
    protected Character character;
    [SerializeField] public Character aggressorCharacter;
    [SerializeField] public float healthLevelRequiredToHeal=4f;
    
    

    public void TakeDamage(float damage, Character attackingCharacter, bool allowKnockBack = false)
    {
        //this will track the given character as the attacker, if they are not dead
        // if no attacking character is given, then it will select the closest character via raycast
        aggressorCharacter = MaybeGetNearestCharacter(attackingCharacter);
        
        if (!IsDead())
        {
            //Debug.Log("Enemy Current Health: " + currentHealth);
            if (character.IsBlockingDefenceActive()
                || character.GetMovementOverride() == Constants.EnemyMovementOverride.KnockBack)
            {
                //Prevent character injury when blocking or being knocked back
                return;
            }

            Hurt();
            
            if (allowDamage)
            {
                currentHealth = Mathf.Clamp(currentHealth - damage, 0, startingHealth);
            }

            if (currentHealth <= 0)
            {
                Death();
            }
        }
    }


    private Health GetCharacterHealth(Character character)
    {
        if (character == null)
        {
            return null;
        }

        return character.GetComponent<Health>();
    }

    public void Heal(float healAmount)
    {
        currentHealth = currentHealth + healAmount;
    }

    abstract protected void Hurt();

    abstract protected void Death();
    
    abstract public void DisableCompletely();

    
    public virtual Character MaybeGetNearestCharacter(Character attackingCharacter)
    {
        //If a supplying character is not given, assume the nearest character did it
        if (attackingCharacter == null)
        {
            attackingCharacter = SpriteFinder.GetNearestCharacter(
                character.GetCharacterCollider(), 100);
        } 
        //otherwise you know who did it
        //but make sure they aren't dead before assigning them
        else if (GetCharacterHealth(attackingCharacter).IsDead())
        {
            return null;
        }
        return attackingCharacter;
    }
    
    virtual public void Knockback()
    {

    }
    
    virtual public void FreezeCharacter(float damage)
    {

    }
    
    virtual public void ClearFreeze(float damage = 0)
    {
    }
    
    virtual public void ClearMovementOverrides()
    {
    }

    public bool IsDead() //todo rename to ShouldBeDead
    {
        return dead || currentHealth <= 0;
    }

    public void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            dead = true;
        }
    }

    public void SetDeathCompleted(bool deathComplete)
    {
        deathCompleted = deathComplete;
    }
}
