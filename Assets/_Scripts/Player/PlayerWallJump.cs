using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerWallJump : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    [SerializeField] public PlayerController playerController;
    [Header("Wall Jump")]
    [SerializeField] public float wallJumpTime = 0.2f;

    [SerializeField] public float wallSlideSpeed = 0.3f;
    [SerializeField] public float wallDistance = 0.5f;
    [SerializeField] public float wallJumpVerticalForce = 100f;
    [SerializeField] public float wallJumpHorizontalForce = 100f;
    [SerializeField] public float wallJumpMoveFreezeTimeDuration = 0.5f;
    public bool isWallSliding = false;
    public bool isWallFalling = false;
    public RaycastHit2D wallCheckHit;
    private RaycastHit2D wallCheckHitBehind;
    private bool isWallJumpCoolDownCouroutineRunning = false;
    private Coroutine couroutineRef;
    public float wallJumpCooldown;


    private void Update()
    {
        //Set walljumping to false when grounded
        if (playerVariables.isWallJumping == true && playerController.IsGrounded())
        {
            playerVariables.isWallJumping = false;
        }
    }

    public void WallJump()
    {
        wallCheckHit = Physics2D.Raycast(playerController.transform.position, new Vector2(wallDistance * playerController.facingDirection, 0), wallDistance, playerController.groundLayer);
        wallCheckHitBehind = Physics2D.Raycast(playerController.transform.position, new Vector2(-wallDistance * playerController.facingDirection, 0), -wallDistance, playerController.groundLayer);
        Debug.DrawRay(playerController.transform.position, new Vector2(wallDistance, 0), Color.green);
        Debug.DrawRay(playerController.transform.position, new Vector2(-wallDistance, 0), Color.green);

        //wallRayCastPositionLeft = wallCheckHit;

        //print("grounded: " + playerController.IsGrounded());
       // print("horizontalinput: " + (playerController.horizontalInput != 0));
        //print("wallcheckhit: " + wallCheckHit);
        //print("wallCheckHitBehind: " + wallCheckHitBehind);
        //print("--------------------------");
        

        if ((wallCheckHit || wallCheckHitBehind) && !playerController.IsGrounded() && (playerController.horizontalInput != 0))
        {
            isWallSliding = true;
            isWallFalling = false;
        }
        else if ((wallCheckHit || wallCheckHitBehind) && !playerController.IsGrounded())
        {
            isWallSliding = false;
            isWallFalling = true;
        }
        else
        {
            isWallSliding = false;
            isWallFalling = false;
        }
        
        if (isWallSliding)
        {
            playerController.rb.velocity = new Vector2(playerController.rb.velocity.x, Mathf.Clamp(playerController.rb.velocity.y, wallSlideSpeed, float.MaxValue));
        }
        
    }

    private IEnumerator WallJumpCoolDown()
    {
        playerVariables.canMove = false;
        //playerController.
        yield return new WaitForSeconds(wallJumpMoveFreezeTimeDuration);

        playerVariables.canMove = true;
        isWallJumpCoolDownCouroutineRunning = false;
        
    }

    public void CleanExecuteWallJumpCooldownCouroutine()
    {
        playerController.ChangeFacingDirection();
        
        if (isWallJumpCoolDownCouroutineRunning)
        {
            playerController.StopCoroutine(couroutineRef);
        }
        couroutineRef = playerController.StartCoroutine(WallJumpCoolDown());
        isWallJumpCoolDownCouroutineRunning = true;
    }
}