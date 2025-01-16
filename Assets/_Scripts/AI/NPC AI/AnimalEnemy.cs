using UnityEngine;
using System.Collections;
using Pathfinding;
using System;
using SupportScripts;

public class AnimalEnemy : AbstractEnemy
{
    [Header("Animal Specifics")]
    [SerializeField] Transform attackPoint;
    [SerializeField] float runSpeed = 40;


    // Use this for initialization
    void Start()
    {
        SharedStart();
        attackPointCollider = attackPoint.GetComponent<BoxCollider2D>();
        character.SetCharacterFacingRight(true);
        manaRegenAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        SharedUpdate();

    }

    protected override void UpdateStart()
    {
        SharedUpdate();
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
}
