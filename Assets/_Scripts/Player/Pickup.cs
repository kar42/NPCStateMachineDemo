using System;
using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveTime = 0f;
    [SerializeField] private bool moveUp = true;
    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    private void Update()
    {
        if (moveUp)
         {
             rb.velocity = Vector2.up*10f;
         } else if (!moveUp)
         {
             rb.velocity = Vector2.down*10f;
         }
         if (moveTime > 1)
         {
             moveUp = !moveUp;
             moveTime = 0;
         } else
         {
             moveTime += Time.deltaTime;
         }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (SpriteFinder.IsPlayer(collision))
        {
            PlayerManager manager = collision.GetComponentInParent<PlayerManager>();
            if(manager)
            {
                bool pickedUp = manager.PickupItem(gameObject);
                if (pickedUp)
                {
                    RemoveItem();
                }
            }
        }
        
    }

    private void RemoveItem()
    {
        Destroy(gameObject);
        AudioManager.instance.PlaySound("HealthPickup_sfx");
    }
}
