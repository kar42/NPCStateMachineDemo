using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public float speed = 100f;
    public Rigidbody2D rb;
    private float damage = 1;
    private AbstractEnemy hitEnemy;
    private Character hitCharacter;

    void Start()
    {
        //Debug.Log(transform.right);
        //rb.velocity = new Vector2(transform.position.x * speed, transform.position.y);
        rb.velocity = transform.right * speed;
    }
}
