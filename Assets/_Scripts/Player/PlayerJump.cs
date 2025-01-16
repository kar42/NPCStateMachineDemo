using UnityEngine;
using UnityEngine.Serialization;

public class PlayerJump : MonoBehaviour
{
    [FormerlySerializedAs("PlayerVariables")]
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    [Header("Jump Quality")]
    [SerializeField] public float fallMultiplier = 2.5f;
    [SerializeField] public PlayerController playerController;
    [SerializeField] public float lowJumpMultiplier = 2f;
    public float m_jumpForce = 7.5f;
    private float wallJumpCooldown;
    public bool jumpButtonPressed = false;
    [Header("Air Dash Quality")]
    [SerializeField] public float airDashVerticalForce = 5.5f;
    [SerializeField] public float airDashHorizontalForce = 5.5f;
    
    public void Jump()
    {
        if (playerController.isActiveCharacter)
        {
            //air dash logic
            if (playerVariables.hasUsedStandardJump && !playerVariables.hasUsedAirDash && !playerVariables.isWallJumping &&
                !playerController._playerWallJump.isWallSliding)
            {
                AirDash();
                playerVariables.hasUsedStandardJump = false;
            }
            else
            {
                //Debug.Log("hoz " + Time.fixedTime + " wallcheckhit : " + wallCheckHit);

                //standard jump
                if (playerController.IsGrounded()  && !playerController._playerWallJump.isWallSliding)
                {
                    //Debug.Log("we got here000000000000000");
                     playerController.animator.SetTrigger("Jump");
                    playerController.animator.SetBool("Grounded", playerController.IsGrounded());
                    playerController.rb.velocity = Vector2.up * m_jumpForce;
                    //m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                    // squishEffect.GetComponent<Animator>().SetTrigger("stretch");
                    playerController.squishEffect.GetComponent<Animator>().SetTrigger("Jump");
                    
                    playerVariables.hasUsedStandardJump = true;
                    playerVariables.isJumping = true;
                    playerController.playerAnimEvents.AE_Jump();
                }

                if (playerController._playerWallJump.wallCheckHit && !playerController.IsGrounded() &&
                    playerController.horizontalInput != 0 || playerController._playerWallJump.isWallSliding)
                {
                     playerController._playerWallJump.CleanExecuteWallJumpCooldownCouroutine();
                    //Debug.Log("we got here1111111111111111");
                    // flip the player sprite
                    //m_facingDirection = -m_facingDirection;
                    //playerController.ChangeFacingDirection();
                    playerController.rb.velocity =
                        new Vector2(
                            playerController.facingDirection * playerController._playerWallJump.wallJumpHorizontalForce,
                            playerController._playerWallJump.wallJumpVerticalForce);
                    //transform.localScale = new Vector3(m_facingDirection, 1f, 1);

                    //m_body2d.velocity = new Vector2(-horizontalInput * 100 * 0.5f, 100);
                    //Debug.Log("velocity: " + m_body2d.velocity);
                    playerController.animator.SetTrigger("Jump");
                    wallJumpCooldown = 0;
                    playerVariables.isWallJumping = true;
                    playerVariables.isJumping = true;
                }

                else if (!playerController.IsGrounded() &&
                         (playerController._playerWallJump.isWallFalling ||
                          playerController._playerWallJump.isWallSliding) && playerController.horizontalInput == 0)
                {
                    playerController._playerWallJump.CleanExecuteWallJumpCooldownCouroutine();
                    playerController._playerWallJump.isWallSliding = false;
                    //playerController.ChangeFacingDirection();
                    playerController.rb.velocity =
                        new Vector2(
                            playerController.facingDirection * playerController._playerWallJump.wallJumpHorizontalForce,
                            playerController._playerWallJump.wallJumpVerticalForce);
                    //transform.localScale = new Vector3(m_facingDirection, 1f, 1);
                    //Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -m_facingDirection, wallHopForce * wallHopDirection.y);
                    //m_body2d.AddForce(forceToAdd, ForceMode2D.Impulse);
                    playerController.animator.SetTrigger("Jump");
                    wallJumpCooldown = 0;
                    playerVariables.isWallJumping = true;
                    playerVariables.isJumping = true;
                }
                else if (!playerController.IsGrounded() &&
                         (playerController._playerWallJump.isWallFalling ||
                          playerController._playerWallJump.isWallSliding) && playerController.horizontalInput != 0)
                {
                    playerController._playerWallJump.CleanExecuteWallJumpCooldownCouroutine();
                    playerController._playerWallJump.isWallSliding = false;
                    //playerController.ChangeFacingDirection();
                    playerController.transform.Rotate(0f, 180f, 0f);
                    playerController.rb.velocity =
                        new Vector2(
                            playerController.horizontalInput * playerController._playerWallJump.wallJumpHorizontalForce,
                            playerController._playerWallJump.wallJumpVerticalForce);
                    //Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * horizontalInput, wallJumpForce * wallJumpDirection.y);
                    //m_body2d.AddForce(forceToAdd, ForceMode2D.Impulse);
                    playerController.animator.SetTrigger("Jump");
                    wallJumpCooldown = 0;
                    playerVariables.isWallJumping = true;
                    playerVariables.isJumping = true;
                }
            }


            //reset to true after a jump
            //canPerformSlideStartAnimation = true;

            jumpButtonPressed = true;
        }
    }

    private void AirDash()
    {
        playerController.animator.SetTrigger("AirDash");
       
        playerController.rb.velocity = new Vector2(airDashHorizontalForce * playerController.facingDirection, airDashVerticalForce);
        playerVariables.hasUsedAirDash = true;
    }

    public void ImproveJumpQuality()
    {
        //If we are falling increase the the speed at which we fall by factor of fallMultiplier
        if (playerController.rb.velocity.y < 0)
        {
            playerController.rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        //If we are currently jumping and we want to drop immediatly 
        else if (playerController.rb.velocity.y > 0 && !jumpButtonPressed)
        {
            playerController.rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        //Add shake when landing on the ground
        

    }
}