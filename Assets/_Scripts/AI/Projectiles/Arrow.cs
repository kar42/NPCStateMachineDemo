using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 100f;
    public Rigidbody2D rb;
    private float arrowDamage = 1;

    void Start()
    {
        //Debug.Log(transform.right);
        //rb.velocity = new Vector2(transform.position.x * speed, transform.position.y);
        rb.velocity = transform.right * speed;
    }

    void OnTriggerEnter2D (Collider2D hitInfo)
    {
        //Debug.Log("Arrow Hit Object: " + hitInfo.name);
        if (SpriteFinder.ShouldIgnoreSensorColliders(hitInfo)) 
            //Add any other colliders which should be ignored here
        {
            //AudioManager.instance.PlaySound("ArrowHit");
            Health enemyHealth;
            if (SpriteFinder.IsPlayer(hitInfo.name))
            { //this could be handled better..
                enemyHealth = hitInfo.GetComponentInParent<Health>();
            }
            else
            {
                enemyHealth = hitInfo.GetComponent<Health>();
            }
            if (enemyHealth != null && !enemyHealth.dead)
            {
                enemyHealth.TakeDamage(arrowDamage, null);
                ScreenShake.Instance.ShakeCamera(15f, .2f);
                //enemyHealth.Knockback();
            }
            Destroy(gameObject);
        }
        
    }
}
