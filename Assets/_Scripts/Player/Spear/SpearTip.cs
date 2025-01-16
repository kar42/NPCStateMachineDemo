using System;
using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;

public class SpearTip : MonoBehaviour
{

    public Rigidbody2D spearRB;
    public Animator animator;
    public BoxCollider2D spearBoxCollider;

    public BoxCollider2D spearTipBoxCollider;
    public float damage = 1;
    public bool hasStruck = false;
    public string struckCharacterName = "";
    public Health struckCharacterHealth;

    void Awake()
    {
        GetComponent<ParticleSystem>().Stop();
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        //Debug.Log("Spear TIP: " + hitInfo.name);
        Collider2D collider2D = hitInfo.GetComponent<Collider2D>();
        //print(collider2D.transform.tag);
        if (collider2D != null && !collider2D.isTrigger)
        {
            //play the impact particles
            if (collider2D.transform.tag == "Killable")
            {
                GetComponent<ParticleSystem>().Play();
            }

            
            
            if (collider2D.transform.tag != "player")
            {
                MaybeAttachSpearToCollider(collider2D);
            }
        }
        //Destroy(gameObject);
    }

    private void MaybeAttachSpearToCollider(Collider2D collider2D)
    {
        //If smashable, smash and continue flight.. check against solid backgrounds?
        if (collider2D.transform.tag == "smashable")
        {
            var particles = GetComponent<ParticleSystem>();
            particles.Play();
            //spearRB.transform.parent = null;
            //spearRB.mass = 10;
            //Destroy the smashable object
            Destroy(collider2D.gameObject);
            ScreenShake.Instance.ShakeCamera(15f, .2f);
        }
        else if (collider2D.transform.tag == "Shield")
        {
            hasStruck = true;
            spearRB.transform.parent = collider2D.transform;
            // stop physics control
            spearRB.velocity = new Vector2(0,0);
            spearRB.isKinematic = true;
            animator.SetBool("spearstuck", true);
            animator.SetTrigger("spearstucktrig");
            //Play sound when spear sticks into something
            AudioManager.instance.PlaySound("SpearStuck_sfx");
            ScreenShake.Instance.ShakeCamera(15f, .2f);
        }
        //attach spear to environment or dynamic platforms
        else if (!hasStruck
                  && (collider2D.transform.tag == "Enviornment"
                      || collider2D.transform.tag == "DynamicPlatform"))
        {
            // stop physics control but do not set parent to prevent weird scale resizing
            hasStruck = true;
            spearRB.velocity = new Vector2(0,0);
            spearRB.isKinematic = true;
            animator.SetBool("spearstuck", true);
            animator.SetTrigger("spearstucktrig");
            AudioManager.instance.PlaySound("SpearStuck_sfx");
            ScreenShake.Instance.ShakeCamera(10f, .2f);
        }
        //attach to player or ground colliders
        else if ( ! hasStruck 
             && (collider2D.transform.tag == "Liftable"
             || (collider2D.transform.tag != "Enviornment" 
                && collider2D.transform.tag != "DynamicPlatform") ) )
        {
            
            // stop physics control
            spearRB.velocity = new Vector2(0,0);
            spearRB.isKinematic = true;
            animator.SetBool("spearstuck", true);
            animator.SetTrigger("spearstucktrig");
            //Debug.Log("Spear Hit Object: "+hitInfo.name);
            if (SpriteFinder.HasCharacterComponent(collider2D) && ! SpriteFinder.IsPlayer(collider2D))
            {
                //Play sound when spear sticks into something
                AudioManager.instance.PlaySound("SpearStuck_sfx");
                EnemyHealth enemyHealth = collider2D.GetComponent<EnemyHealth>();
                if (enemyHealth != null && !enemyHealth.IsDead())
                {
                    spearRB.transform.parent = collider2D.transform;
                    hasStruck = true;
                    struckCharacterName = enemyHealth.name;
                    struckCharacterHealth = enemyHealth;
                    enemyHealth.TakeDamage(damage,null);
                    enemyHealth.FreezeCharacter(0);
                    ScreenShake.Instance.ShakeCamera(15f, .2f);
                }
            }
            else
            {
                hasStruck = true;
                spearRB.transform.parent = collider2D.transform;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.transform.tag == "Shield")
        { //allow spear to fall to ground if enemy stops shielding
            DropSpear();
        }
        else if (struckCharacterHealth != null && struckCharacterName == hitInfo.name)
        { //this does not seem to trigger if player is too close..
            EnemyHealth enemyHealth = hitInfo.GetComponent<EnemyHealth>();
            if (SpriteFinder.HasCharacterComponent(hitInfo) && enemyHealth != null)
            {//this triggers when the spear is returned,
             //or when the enemy dies with spear still engaged
                ClearCharacterFreeze();
            }
        }
    }

    public void DropSpear()
    {
        //print("spear dropping");
        spearRB.isKinematic = false;
        //allow the spear to fall on the ground
        spearRB.velocity = new Vector2(0,-30);
    }

    public void ClearCharacterFreeze()
    {
        if (struckCharacterHealth != null)
        {
            //add hitBox check, ensure it checks parent collider
            hasStruck = false;
            if (struckCharacterHealth.IsDead())
            {
                struckCharacterHealth.ClearFreeze();
                DropSpear();
            }
            else
            {
                struckCharacterHealth.ClearFreeze();
            }
            struckCharacterName = "";
            struckCharacterHealth = null;
        }
    }

    
}