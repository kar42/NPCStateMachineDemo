using UnityEngine;
using System.Collections;
using Pathfinding;
using System;

public class Sorcerer : AbstractEnemy {

    [Header("Sorcerer Specifics")]
    [SerializeField] Transform attackPoint;
    [SerializeField] Transform HandPosition1;
    public GameObject aoe;
    public GameObject fireBall;



    // Use this for initialization
    void Start () {
        SharedStart();
        attackPointCollider = attackPoint.GetComponent<BoxCollider2D>();
        character.SetCharacterFacingRight(true);
        character.SetAllowBlocking(true);
        character.SetCanHeal(true);
        manaRegenAmount = 50;
        hasRangedAttack = true;
        rangedAttackMinDistance = 30f;
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

    protected override void UpdateFinish()
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

    protected override Collider2D GetAttackPointCollider()
    {
        return attackPointCollider;
    }

    //this method is triggered at a specific cast animation frame for the sorcerer
    public void ReleaseFireball()
    {
        var castPosition = new Vector2(HandPosition1.position.x, HandPosition1.position.y);
        Instantiate(fireBall, castPosition,Quaternion.Euler(0, 0, 0))
            .GetComponent<FireBall>()
            .SetEnemy(this)
            .SetTarget(GetCurrentTargetCollider());

    }
}
