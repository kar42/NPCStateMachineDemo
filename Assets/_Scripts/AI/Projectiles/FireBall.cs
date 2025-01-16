using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using SupportScripts;
using UnityEditor.Rendering;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    private float damage = 1;
    private float duration = 5;
    private CircleCollider2D _circleCollider;
    private AbstractEnemy _castingEnemy;
    private Collider2D _targetCollider;
    protected Seeker seeker;
    protected Path path;
    protected float speed = 40f;
    protected float nextWaypointDistance = 3f;
    protected int currentWayPoint = 0;
    protected float timeSinceCast;
    public Rigidbody2D rb2d;
    private Animator animator;
    private bool continueActions;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        _circleCollider = GetComponent<CircleCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        InvokeRepeating("UpdatePath", 0f, .5f);
        timeSinceCast = 0.0f;
        animator = GetComponent<Animator>();
        continueActions = true;
    }
    
    void UpdatePath()
    {
        if(seeker.IsDone() && continueActions)
        {
            seeker.StartPath(_circleCollider.bounds.center, _targetCollider.ClosestPoint(_circleCollider.bounds.center),
                OnPathComplete);

        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (continueActions) {
            timeSinceCast += Time.deltaTime;
            if (timeSinceCast <= duration)
            {
                Move();
            }
            else
            {
                DestroyPrefab();
            }
        }
    }
    
    private void Move()
    {
        try
        {
            var currentWayPointOnPath = path.vectorPath[currentWayPoint];
            Vector2 direction = ((Vector2)currentWayPointOnPath - (Vector2)_circleCollider.bounds.center).normalized;
            rb2d.velocity = new Vector2(direction.x * speed, direction.y* speed);

            float distance = Vector2.Distance(_circleCollider.ClosestPoint(currentWayPointOnPath), currentWayPointOnPath);
            Debug.DrawLine(_circleCollider.ClosestPoint(currentWayPointOnPath), currentWayPointOnPath, Color.cyan);
            if (distance < nextWaypointDistance)
            {
                currentWayPoint++;
            }
        }
        catch (ArgumentOutOfRangeException e)
        { 
            //Debug.Log("ArgumentOutOfRangeException occurred in Fireball -- Move()");
        }
        catch (Exception e)
        {
            //Debug.Log("Exception occurred in Fireball -- Move()");
            //Debug.Log(e.StackTrace);
        }

    }


    void DamageCharacters ()
    {
        List<Collider2D> intersectingCharacters = new List<Collider2D>();
        ContactFilter2D contactFilter = new ContactFilter2D();
        Physics2D.OverlapCollider(_circleCollider, contactFilter, intersectingCharacters);
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
                enemyHealth.TakeDamage(damage, _castingEnemy.EnemyCharacter());
                //enemyHealth.Knockback();
            }
        }
    }

    void DestroyPrefab()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D (Collider2D hitInfo)
    {
        if (continueActions)
        {
            if (SpriteFinder.IsSpear(hitInfo.name))
            {
                animator.SetTrigger("Destroy");
                rb2d.velocity = new Vector2(0, 0);
                continueActions = false;
            }
            else if (hitInfo.name == _targetCollider.name)
            {
                //this could be handled better..
                var enemyHealth = hitInfo.GetComponentInParent<Health>();
                enemyHealth.TakeDamage(damage, null);
                //AudioManager.instance.PlaySound("ArrowHit");

                Destroy(gameObject);
            }
        }
    }

    public FireBall SetEnemy(AbstractEnemy enemy)
    {
        _castingEnemy = enemy;
        return this;
    }

    public FireBall SetTarget(Collider2D target)
    {
        _targetCollider = target;
        return this;
    }
}
