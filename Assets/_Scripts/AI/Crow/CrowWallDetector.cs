using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowWallDetector : MonoBehaviour
{
    public bool isCollidingWithEnv = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //print("CrowWallDetector: " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Enviornment"))
        {
            isCollidingWithEnv = true;
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enviornment"))
        {
            isCollidingWithEnv = false;
        }
    }
}
