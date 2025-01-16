using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpearRetrieveAnimTrigger : MonoBehaviour
{

    [FormerlySerializedAs("PlayerVariables")]
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;

    
    [SerializeField] private PlayerController player;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "spear" && playerVariables.retrieveSpearPressed && playerVariables.isMoving)
        {
            //pass the name of the gameobject i.e. SpearRetrieveSW, SpearRetrieveSE etc.
            print(gameObject.name);
            player.animator.SetTrigger(gameObject.name);
            
        }
    }
}
