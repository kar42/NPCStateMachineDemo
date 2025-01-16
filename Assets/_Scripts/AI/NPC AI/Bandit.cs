using UnityEngine;
using System.Collections;
using Pathfinding;
using System;

public class Bandit : AbstractEnemy {

    [Header("Bandit Specifics")]
    [SerializeField] Transform attackPoint;


    // Use this for initialization
    void Start () {
        SharedStart();
        attackPointCollider = attackPoint.GetComponent<BoxCollider2D>();
        character.SetCharacterFacingRight(false);
        manaRegenAmount = 10;
    }

    // Update is called once per frame
    void Update()
    {
        SharedUpdate();
    }

    protected override void UpdateStart()
    {

        SharedUpdate();
        animator.SetFloat(Constants.AirSpeedY, rb2d.velocity.y);
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
