using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVariables : MonoBehaviour
{
    public bool isWalking,
        isGrounded,
        isJumping,
        hasUsedStandardJump,
        isWallJumping,
        isGrabbing,
        isDead,
        isBlocking,
        canMove,
        hasUsedAirDash,
        isMoving,
        retrieveSpearPressed,
        isSpearThrown,
        isAttacking,
        isSpearControlButtonHeld,
        isClimbingLadder,
        isInMultiplayerMode,
        isPlayer2;
        
        

    public float xVelocity, yVelocity;
    
    public static bool[] isHitting = new bool[3];
}
