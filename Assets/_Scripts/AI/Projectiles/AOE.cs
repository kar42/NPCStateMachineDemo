using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;

public class AOE : MonoBehaviour
{
    private float aoeDamage = 1;
    private BoxCollider2D _boxCollider;
    private AbstractEnemy _enemy;

    void Start()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }
    
    
    void DamageCharacters ()
    {
        List<Collider2D> intersectingCharacters = new List<Collider2D>();
        ContactFilter2D contactFilter = new ContactFilter2D();
        Physics2D.OverlapCollider(_boxCollider, contactFilter, intersectingCharacters);
        foreach (var collider in intersectingCharacters)
        {
            //var enemyHealth = collider.GetComponent<Health>();
            Health enemyHealth;
            if (SpriteFinder.IsPlayer(collider.name))
            { //this could be handled better..
                enemyHealth = collider.GetComponentInParent<Health>();
            }
            else
            {
                enemyHealth = collider.GetComponent<Health>();
            }
            if (enemyHealth != null && !enemyHealth.dead)
            {
                //Debug.Log("AOE Hit : " + hitInfo.name);
                enemyHealth.TakeDamage(aoeDamage, _enemy.EnemyCharacter());
                //enemyHealth.Knockback();
            }
        }
    }

    void OnTriggerEnter2D (Collider2D hitInfo)
    {
        //Debug.Log("AOE Hit : " + hitInfo.name);
        /*if (hitInfo.gameObject.layer != Constants.ProjectileLayerNum 
            && hitInfo.gameObject.layer != Constants.AttackPointLayerNum)
        {
            //AudioManager.instance.PlaySound("ArrowHit");
            var enemyHealth = hitInfo.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.dead)
            {
                enemyHealth.TakeDamage(aoeDamage);
                //enemyHealth.Knockback();
            }
        }*/
    }

    void DestroyAOEPrefab()
    {
        Destroy(gameObject);
    }

    public void SetEnemy(AbstractEnemy enemy)
    {
        _enemy = enemy;
    }
}
