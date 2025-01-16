using UnityEngine;
using System.Collections;
using Pathfinding;
using System;

public class Knight : AbstractEnemy
{

    [Header("Knight Specifics")]
    [SerializeField] Transform attackPoint;



    // Use this for initialization
    void Start()
    {
        SharedStart();
        attackPointCollider = attackPoint.GetComponent<BoxCollider2D>();
        character.SetCharacterFacingRight(true);
    }

    // Update is called once per frame
    void Update()
    {
        SharedUpdate();
    }

    protected override void UpdateStart()
    {

        SharedUpdate();
        animator.SetFloat("AirSpeedY", rb2d.velocity.y);
    }

    override protected void UpdateFinish()
    {
    }

    public override Vector2 GetAttackPointPosition()
    {
        return attackPoint.position;
    }

    public override float GetAttackPointRange()
    {
        return attackRange;
    }

    override protected Collider2D GetAttackPointCollider()
    {
        return attackPointCollider;
    }
}
