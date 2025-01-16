using System;
using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class PlayerHealth : Health
{
    
    [FormerlySerializedAs("PlayerVariables")]
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    [SerializeField] public float knockbackIntensity = 1f;
    //[SerializeField] public PlayerVariables playerVariables;
    
    public ScreenShake screenShake;
    public Animator m_animator;
    private float maxHealth = 100f;
    private PlayerController player;

    // Start is called before the first frame update
    private void Awake()
    {
        //m_animator = GetComponent<Animator>();
        currentHealth = startingHealth;
        character = GetComponent<Character>();
        player = GetComponent<PlayerController>();

    }
    

    override protected void Hurt()
    {
        m_animator.SetTrigger("Hurt");
        AudioManager.instance.PlaySound("Hurt_sfx");
        ScreenShake.Instance.ShakeCamera(5f, .1f);
        GetComponent<ParticleSystem>().Play();
    }

    public void Heal(int healAmount)
    {
        //Check to make sure we dont give the player more than their max health
        if(currentHealth + healAmount <= maxHealth)
            currentHealth += healAmount;
        else currentHealth = maxHealth;

        Debug.Log("Player Healed");

    }

    override public void Knockback()
    {
        Debug.Log("Player knockback triggered");
        m_animator.SetTrigger("GroundKnockback1");
        character.SetMovementOverride(Constants.EnemyMovementOverride.KnockBack);
        var velocity = player.GetComponent<Rigidbody2D>().velocity;
        //player.GetComponent<Rigidbody2D>().velocity = new Vector2(100 * knockbackIntensity, velocity.y); //this was applying forward knockback
    }

    public bool KnockbackUpdate()
    {
        if (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Knockback1"))
        {
            character.SetMovementOverride(Constants.EnemyMovementOverride.KnockBack);
            if (character.IsCharacterFacingRight())
            {
                character.GetComponent<Rigidbody2D>().velocity = new Vector2(Vector2.left.x * knockbackIntensity * player.GetAnimationSpeed(), character.GetComponent<Rigidbody2D>().velocity.y);
            }
            else
            {
                character.GetComponent<Rigidbody2D>().velocity = new Vector2(Vector2.right.x * knockbackIntensity * player.GetAnimationSpeed(), character.GetComponent<Rigidbody2D>().velocity.y);
            }
            return true;
        }
        else if (!m_animator.GetCurrentAnimatorStateInfo(0).IsName("Knockback1") && character.GetMovementOverride() == Constants.EnemyMovementOverride.KnockBack)
        {
            //todo this should be moved to some general "reset status" type code, to ensure things reset consistently
            //this needs to be set back to none, because any movement override prevents player from taking damage
            character.SetMovementOverride(Constants.EnemyMovementOverride.None);
            return false;
        }

        return false;
    }
    
    override public void FreezeCharacter(float damage)
    {

    }
    
    override public void ClearFreeze(float damage)
    {
    }

    public void KillCharacter()
    {
        Death();
    }
    
    override protected void Death()
    {
        m_animator.SetTrigger("Death");
        dead = true;
        playerVariables.isDead = dead;
    }

    override public void DisableCompletely()
    {
    }

    private void GameOver()
    {
        //throw new NotImplementedException();
    }

    /**
     ***************Save / Load Implementation**********************************
    **/

    public object SaveState()
    {
        return new SaveData()
        {
            currentHealth = this.currentHealth,
            maxHealth = this.maxHealth
        };
    }

    public void LoadState(object state)
    {
        var saveData = (SaveData)state;
        currentHealth = saveData.currentHealth;
        maxHealth = saveData.maxHealth;
    }

    [Serializable]
    private struct SaveData
    {
        public float currentHealth;
        public float maxHealth;
    }
}
