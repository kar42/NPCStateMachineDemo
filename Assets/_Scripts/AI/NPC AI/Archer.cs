using UnityEngine;
using System.Collections;
using Pathfinding;
using System;
using System.Text.RegularExpressions;

public class Archer : AbstractEnemy
{
    [Header("Archer Specifics")]
    [SerializeField] bool attackStance;
    [SerializeField] bool attackUp;
    [SerializeField] bool attackDown;
    private Bow bow;


    // Use this for initialization
    void Start()
    {
        SharedStart();
        bow = gameObject.GetComponent<Bow>();
        attackPointCollider = bow.firePoint.GetComponent<BoxCollider2D>();
        character.SetCharacterFacingRight(true);
        manaRegenAmount = 10;
        hasRangedAttack = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStart();
        UpdateFinish();
    }

    override protected void UpdateStart()
    {
        SharedUpdate();
        animator.SetFloat(Constants.AirSpeedY, rb2d.velocity.y);
    }

    override protected void UpdateFinish()
    {
    }

    override public Vector2 GetAttackPointPosition()
    {
        return bow.firePoint.position;
    }

    public override float GetAttackPointRange()
    {
        return 0;
    }

    override protected Collider2D GetAttackPointCollider()
    {
        return attackPointCollider;
    }

    private bool WillArrowHit()
    {
        return false;
    }

    private Constants.BowAttackDirection SetAttackDirection()
    {
        /*Constants.BowAttackDirection attackDirection;
        var targetFirePosition = new Vector2(GetCurrentTargetCollider().bounds.center.x, GetAttackPointPosition().y);
        RaycastHit2D raycastHitStraight = Physics2D.Linecast(GetAttackPointCollider().ClosestPoint(targetFirePosition), targetFirePosition);
        if (raycastHitStraight.collider != null )//&& IsCurrentTarget(raycastHitStraight.collider.name))
        {
            //the target is directly ahead of the archer attack point
            attackDown = false;
            bow.attackDown = false;
            attackUp = false;
            bow.attackUp = false;
            attackDirection = Constants.BowAttackDirection.AttackStraight;
        }
        else if (GetCurrentTargetCollider().bounds.center.y > GetAttackPointPosition().y)
        {
            //the target is higher than the archer attack point
            attackUp = true;
            bow.attackUp = true;
            attackDown = false;
            bow.attackDown = false;
            attackDirection = Constants.BowAttackDirection.AttackUp;
        }
        else if (GetCurrentTargetCollider().bounds.center.y < GetAttackPointPosition().y)
        {
            //the target is lower than the archer attack point
            //ToDo check if target is on the same level (reachable).. This should probably only happen if target is at archers feet
            attackDown = true;
            bow.attackDown = true;
            attackUp = false;
            bow.attackUp = false;
            attackDirection = Constants.BowAttackDirection.AttackDown;
        } 
        else
        {
            //When could this hit?
            attackDirection = Constants.BowAttackDirection.AttackStraight;
        }
        return attackDirection;*/
        return Constants.BowAttackDirection.AttackUp;
    }
}
