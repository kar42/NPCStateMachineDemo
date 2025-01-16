using System;
using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] Transform attackPoint;
    [SerializeField] Transform chargeAttackPoint;
    [SerializeField]  public float attackRange = 10f;
    [SerializeField]  public float chargeAttackRange = 0.7f;
    [SerializeField]  public LayerMask enemyLayers;
    [SerializeField]  public LayerMask smashableLayer;
    [SerializeField]  public int attackDamage = 3;


    public bool successfulAttack = false;

    private void Block()
    {
        throw new NotImplementedException();
    }

    public void Attack()
    {
        // detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        //Damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            AbstractEnemy enemyClass = enemy.GetComponent<AbstractEnemy>();
            if (enemyClass != null && !enemyClass.GetComponent<Health>().IsDead())
            {
                Character enemyCharacter = enemy.GetComponent<Character>();
                enemyCharacter.SetDefenceWarning(true);
                if (enemyCharacter.IsBlockingDefenceActive())
                {
                    //Debug.Log(Time.deltaTime+" PlayerCombat was knocked back by: " + enemy);
                    playerController.GetComponent<Health>().Knockback();
                }
                else
                {
                    //Debug.Log(Time.deltaTime+" PlayerCombat We hit an enemy: " + enemy);
                    enemy.GetComponent<EnemyHealth>()
                        .TakeDamage(attackDamage, playerController.GetCharacter(),true);
                }
            }
            ScreenShake.Instance.ShakeCamera(15f, .2f);
        }
    }

    public void ChargeAttack()
    {
        Debug.Log("chargeattack: ");
        // detect enemies in range of attack
        Collider2D[] hitSmashable = Physics2D.OverlapCircleAll(chargeAttackPoint.position, chargeAttackRange, smashableLayer);
        //Damage them
        foreach (Collider2D smashable in hitSmashable)
        {
            Debug.Log("PlayerCombat: We hit a smashable: " + smashable);
            
            //Destroy the smashable object
            Destroy(smashable.gameObject);
            ScreenShake.Instance.ShakeCamera(15f, .2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        //visually see attack range 
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
