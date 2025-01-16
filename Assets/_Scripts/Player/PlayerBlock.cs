using System;
using UnityEngine;

public class PlayerBlock : MonoBehaviour
{
    
    public PlayerController playerController;
    
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    private void Update()
    {
        
    }

    public void BlockAttackStart()
    {
        if (playerController.isActiveCharacter)
        {
            playerVariables.isBlocking = true;
            playerController.animator.SetTrigger("BlockStart");
            playerController.animator.SetBool("isBlocking", playerVariables.isBlocking);
            playerController.rb.velocity = new Vector2(0, playerController.rb.velocity.y);
            playerVariables.canMove = false;
            //Enable the block collider
            GameObject.Find("BlockBox").GetComponent<Collider2D>().enabled = true;
        }
    }

    public void BlockAttackExecuting()
    {
        
    }

    public void BlockAttackEnd()
    {
        if (playerController.isActiveCharacter)
        {
            playerVariables.isBlocking = false;
            //PlayerVariables.canMove = true;
            playerController.animator.SetTrigger("NotBlockingAnymore");
            //Enable the block collider
            GameObject.Find("BlockBox").GetComponent<Collider2D>().enabled = false;
        }
    }
}