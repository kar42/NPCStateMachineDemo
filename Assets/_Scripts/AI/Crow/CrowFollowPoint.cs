using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowFollowPoint : MonoBehaviour
{

    public bool isCollidingWithEnv = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //print("CrowFollowPoint: " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Enviornment"))
        {
            isCollidingWithEnv = true;
        }
        else
        {
            isCollidingWithEnv = false;
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        //print("CrowFollowPoint: " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Enviornment"))
        {
            isCollidingWithEnv = false;
        }
    }
}
