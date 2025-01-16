using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.InputSystem;
using Pathfinding;
using SupportScripts;
using UnityEngine.Serialization;

//[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;

    [Header("Input Manager")]
    [SerializeField] public GameObject inputManager;
    
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    [Header("Animator")]
    [SerializeField] public Animator animator;
    [Header("Layers")]
    [SerializeField] public  LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] public LayerMask enemyLayers;
    [Header("Health")]
    [SerializeField] PlayerHealth playerHealth;
    [Header("Combat")]
    [SerializeField] PlayerCombat playerCombat;
    [Header("Pause Menu")]
    [SerializeField] public PauseMenu pauseMenu;
    
    [SerializeField] public Transform squishEffect;

    [Header("Mana")]
    [SerializeField] public ManaBar manaBar;
    [Header("AnimatorEvents")]
    [SerializeField] public PlayerAnimEvents playerAnimEvents;
    [Header("Player Position")]
    private Vector2 currentPosition;
    [Header("Crow")] [SerializeField] public CrowMovement crowMovement;
    
    public PlayerInput PlayerInput => playerInput;
    
    public float m_dodgeForce = 8.0f;
    public float m_parryKnockbackForce = 4.0f;
    public bool m_noBlood = false;
    public bool m_hideSword = false;
    public float m_rollForce = 6.0f;
    public float m_speed = 4.0f;

    public Rigidbody2D rb;
    
    private BoxCollider2D boxCollider;

    public bool isRolling { get; private set; }

    public bool isSwinging = false;

    private float rollDuration = 8.0f / 14.0f;
    private float rollCurrentTime;
    public float horizontalInput{ get; set; }
    public int facingDirection = 1;
    private bool isFacingRight = true;
    private bool movementDisabled;
    private bool summonMorrigan;


    private SpriteRenderer sr;
    private PlayerWallDetector m_groundSensor;
    private PlayerWallDetector m_wallSensorR1;
    private PlayerWallDetector m_wallSensorR2;
    private PlayerWallDetector m_wallSensorL1;
    private PlayerWallDetector m_wallSensorL2;
    //public bool isMoving { get; private set; } = false; 
    public bool isDead { get; private set; } = false;
    public  bool isDodging { get; set; } = false;
    public bool isLedgeGrabbing { get; set; } = false;
    public bool isLedgeClimbing { get; set; } = false;
    public bool isCrouching { get; set; } = false;
    private Vector3 m_climbPosition;
    public float m_disableMovementTimer = 0.0f;
    private float m_parryTimer = 0.0f;
    private float m_respawnTimer = 0.0f;
    private Vector3 m_respawnPosition = Vector3.zero;
    private float m_gravity;
    public float m_maxSpeed = 4.5f;
    public bool upButtonPressed = false;
    public bool downButtonPressed = false;
    private bool grounded = true;
    public float m_animationSpeed = 1.0f;


    private CharacterController _characterController;
    //public PlayerInputActions playerControls;
    //player controlled actions
    
    public InputAction move;
    //private InputAction attack;
    public InputAction jump;
    private InputAction parry;
    private InputAction roll;
    private InputAction upAttack;
    private InputAction airAttack;
    private InputAction dodge;
    private InputAction throw_attack;
    private InputAction crouch;
    private InputAction ledgeClimb;
    private InputAction ledgeDrop;
    private InputAction up;
    private InputAction down;
    private InputAction switchCharacter;
    public InputAction block;
    private InputAction spearChargeAttack;
    private InputAction switchToSword;
    private InputAction switchToSpear;
    private InputAction switchToGrapple;
    public InputAction moveCrowToPosition;

    bool canPerformSlideStartAnimation = true;

    public bool isActiveCharacter = true;

    private bool previously_grounded = false;


    //switching between weapons

    private bool isFootstepSoundPlaying = false;

    protected Character character;

    public Transform crowMovePointTransform;

    public PlayerWallJump _playerWallJump;
    public PlayerJump _playerJump;
    public PlayerChangeWeapon _changeWeapon;
    public PlayerSpearThrow playerSpearThrow;
    public AttackSystem _attackSystem;
    public PlayerBlock playerBlock;

    [Header("Watched Variables")]
    public bool isJumpingWatch;

    public bool groundedWatch;

    public bool canPlayerMove;

    public bool iswalljumping;

    public bool canRefreshButtons;

    private void Start()
    {
        
    }

    private void Awake()
    {
        //set this at startup to make sure there is no rouge alterations 
        canRefreshButtons = false;
        
        //these grab refs to rigidbody and animator of charactor
        rb = GetComponent<Rigidbody2D>();
        boxCollider = transform.Find("SquashAndStretchAnchor/Sprite").GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        //seeker = spear.GetComponent<Seeker>();
        
        playerInput = inputManager.GetComponent<PlayerInput>();
        
        movementDisabled = false;
        summonMorrigan = false;
        PlayerSpearThrow.timeSinceAttack = 0.0f;
        isRolling = false;

        m_groundSensor = transform.Find("GroundSensor").GetComponent<PlayerWallDetector>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<PlayerWallDetector>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<PlayerWallDetector>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<PlayerWallDetector>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<PlayerWallDetector>();

        //prevent grapple being set when game starts
        //grappler.DisableGrapple();
        ChangeWeapon.isSpearEquipped = true;
        character = GetComponent<Character>();
        character.SetCharacterFacingRight(true);
        AttackSystem.isAttacking = false;
        playerVariables.canMove = true;
    }

    // update runs on every frame
    private void FixedUpdate()
    {
        if (canRefreshButtons)
        {
            RefreshButtons();
        }
        
        /*if (gameObject.transform.parent.gameObject.name != "Player&Crow")
        {
            isActiveCharacter = false;
            crowMovement.isActiveCharacter = true;
            gameObject.SetActive(false);
            crowMovement.isFollowing = false;
        }*/

        playerVariables.isGrounded = IsGrounded();
        playerVariables.xVelocity = rb.velocity.x;
        playerVariables.yVelocity = rb.velocity.y;
        
        canPlayerMove = playerVariables.canMove;
        iswalljumping = playerVariables.isWallJumping;
        if (isActiveCharacter)
        {
            DisablePlayerMovementUntilAnimationEnd();
            /*if (m_disableMovementTimer > 0)
            {
                movementDisabled = true;
            }
            else
            {
                movementDisabled = false;
            }
            
            if (movementDisabled)
            {
                return;
            }*/
            if (playerVariables.isDead)
            {
                return;
            }

            Knockback();
            //knockback behavior will override any other update code besides death.. this should be moved into a different function at some point..
            


            groundedWatch = IsGrounded();
            isJumpingWatch = playerVariables.isJumping;

            LockCrowInPlace();
            ShouldThroughObjectCheck();
            //SpearChargeTimer();
            //AttackSystem.PlayerAttackingWarning();

            currentPosition = rb.position;
            // logic to trigger the wall slide start animation
            WallSlideStart();
            ChangeWeapon.ChangeWeapon();
            grounded = IsGrounded();

            animator.SetBool("canPerformWallSlideStartAnim", canPerformSlideStartAnimation);
            SquashAnimation();

            if (grounded && !previously_grounded)
            {
                playerVariables.isJumping = false;
                playerVariables.hasUsedAirDash = false;
            }

            // Decrease death respawn timer 
            m_respawnTimer -= Time.deltaTime;

            // Increase timer that controls attack combo
            PlayerSpearThrow.timeSinceAttack += Time.deltaTime;

            // Decrease timer that checks if we are in parry stance
            m_parryTimer -= Time.deltaTime;

            // Decrease timer that disables input movement. Used when attacking
            //m_disableMovementTimer -= Time.deltaTime;

            // Respawn Hero if dead
            if (isDead && m_respawnTimer < 0.0f)
                RespawnHero();

            // Increase timer that controls attack combo
            PlayerSpearThrow.timeSinceAttack += Time.deltaTime;

            // Increase timer that checks roll duration
            if (isRolling)
                rollCurrentTime += Time.deltaTime;

            // Disable rolling if timer extends duration
            if (rollCurrentTime > rollDuration)
                isRolling = false;

            if (isDead)
                return;

            Fall();
            PlayerJump.ImproveJumpQuality();
            if (!character.HasMovementOverride())
            {
                Move();
            }
            

            if (isActiveCharacter)
            {
                summonMorrigan = false;
                _playerWallJump.WallJump();
                WallSlide();
                //LedgeGrab();
            }

            





            previously_grounded = grounded;
        }
        
        if (!isActiveCharacter)
        {
            summonMorrigan = true;
            rb.velocity = new Vector2(0, 0);
            animator.SetInteger("AnimState", 0);

        }
    }

    private void Knockback()
    {
        if (playerHealth.KnockbackUpdate())
        {
            return;
        }
        else if (character.GetMovementOverride() != Constants.EnemyMovementOverride.None)
        {
            return;
        }

        //add knockback if player has collided with another character.. Maybe allow this to ignore "Docile" demeanored characters?
        if (SpriteFinder.IsAnyCharacterColliderBelowMyFeet(boxCollider, 1) ||
            SpriteFinder.HasAggressiveCharacterAhead(boxCollider, 1, isFacingRight))
        {
            //it is better to trigger knockback directly, without triggering "hurt" animation from take damage health method
            //If damage on knockback is desired, knockback method could silently deliver damage...
            playerHealth.Knockback(); //.TakeDamage(1, null, true);
            return;
        }
    }

    public void setCanRefreshButtons()
    {
        print("PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPAAAAAAAASAAAAA");
        canRefreshButtons = true;
    }

    private void RefreshButtons()
    {
        /*print("PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPAAAAAAAASAAAAA222");
        print(PlayerInput.currentActionMap.ToString());
        print(PlayerInput.currentActionMap.enabled);
        PlayerInput.DeactivateInput();
        GetComponent<PlayerInput>().DeactivateInput();
        PlayerInput.SwitchCurrentActionMap("UI");
        print(PlayerInput.currentActionMap.ToString());
        print(PlayerInput.currentActionMap.enabled);
        PlayerInput.SwitchCurrentActionMap("Player");
        print(PlayerInput.currentActionMap.ToString());
        print(PlayerInput.currentActionMap.enabled);

        PlayerInput.ActivateInput();
        GetComponent<PlayerInput>().ActivateInput();
        //set to false to only run once
        canRefreshButtons = false;*/
    }

    private void TouchingEnemyReaction()
    {
        if (SpriteFinder.HasAggressiveCharacterAhead(boxCollider, 1, isFacingRight))
        {
            playerHealth.TakeDamage(1, null, true);
        }
    }

    /*private void PlayAnim(string newState)
    {
        if(currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }*/

    private void SquashAnimation()
    {
        //play squash animation
        if (grounded && !previously_grounded)
        {
            squishEffect.GetComponent<Animator>().SetTrigger("Land");
            //TODO Assess later whether this is nessessary
            ScreenShake.Instance.ShakeCamera(8f, .1f);
            //Debug.Log("GROUNDED");
            //AudioManager.instance.PlaySound("Landing");
        }
    }

    private void ShouldThroughObjectCheck()
    {
        // Handle passing through 
        if(downButtonPressed && m_groundSensor.CanPassThroughPlatform())
        {
            m_groundSensor.PassThroughObject();
        }
    }

    private void LockCrowInPlace()
    {
        if (IsCrowTouchingMe())
        {
            FreezeRigidbodyInPlace();
        }
        else
        {
            UnFreezeRigidbody();
        }
    }

    public Character GetCharacter()
    {
        return character;
    }
    
    public void FreezeRigidbodyInPlace()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void UnFreezeRigidbody()
    {
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    protected bool IsCrowTouchingMe()
    { //should check front only - otherwise player contact from behind could interfere?
        var crowCollider = crowMovement.GetComponentInParent<BoxCollider2D>();
        if (!crowCollider.enabled)
        {
            return false;
        }
        bool isTouchingMe = boxCollider.IsTouching(crowCollider);
        //Debug.Log(boxCollider.name + " -> Is Player touching me: " + isTouchingMe);
        return isTouchingMe;
    }

    private void WallSlideStart()
    {
        if (rb.velocity.y > 0 && _playerWallJump.isWallSliding)
        {
            // reimplement later
            //animator.SetTrigger("WallslideStart");
        }
        //Debug.Log("rb.velocity.y: " + rb.velocity.y);
        //Debug.Log("Wallslide: " + m_wallSlide);
    }

    /*private void LedgeGrab()
    {
        // Check if all sensors are setup properly
        if (m_wallSensorR1 && m_wallSensorR2 && m_wallSensorL1 && m_wallSensorL2)
        {
            //Grab Ledge
            // True if either bottom right sensor is colliding and top right sensor is not colliding 
            // OR if bottom left sensor is colliding and top left sensor is not colliding 
            bool shouldGrab = !isLedgeClimbing && !isLedgeGrabbing && ((m_wallSensorR1.State() && !m_wallSensorR2.State()) || (m_wallSensorL1.State() && !m_wallSensorL2.State()));
            if (shouldGrab)
            {
                Vector3 rayStart;
                if (facingDirection == 1)
                    rayStart = m_wallSensorR2.transform.position + new Vector3(0.2f, 0.0f, 0.0f);
                else
                    rayStart = m_wallSensorL2.transform.position - new Vector3(0.2f, 0.0f, 0.0f);

                var hit = Physics2D.Raycast(rayStart, Vector2.down, 1.0f);

                GrabableLedge ledge = null;
                if (hit)
                    ledge = hit.transform.GetComponent<GrabableLedge>();

                if (ledge)
                {
                    isLedgeGrabbing = true;
                    rb.velocity = Vector2.zero;
                    rb.gravityScale = 0;

                    m_climbPosition = ledge.transform.position + new Vector3(ledge.topClimbPosition.x, ledge.topClimbPosition.y, 0);
                    if (facingDirection == 1)
                        transform.position = ledge.transform.position + new Vector3(ledge.leftGrabPosition.x, ledge.leftGrabPosition.y, 0);
                    else
                        transform.position = ledge.transform.position + new Vector3(ledge.rightGrabPosition.x, ledge.rightGrabPosition.y, 0);
                }
                animator.SetBool("LedgeGrab", isLedgeGrabbing);
            }
        }
    }*/

    private void WallSlide()
    {
        // Check if all sensors are setup properly
        if (m_wallSensorR1 && m_wallSensorR2 && m_wallSensorL1 && m_wallSensorL2)
        {
            bool prevWallSlide = _playerWallJump.isWallSliding;
            //Wall Slide
            // True if either both right sensors are colliding and character is facing right
            // OR if both left sensors are colliding and character is facing left

            _playerWallJump.isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State() && (facingDirection == 1 || facingDirection == -1) || (m_wallSensorL1.State() && m_wallSensorL2.State() && facingDirection == -1));
            if (IsGrounded())
            {
                _playerWallJump.isWallSliding = false;
            }
            animator.SetBool("WallSlide", _playerWallJump.isWallSliding);
            //Play wall slide sound
            if (prevWallSlide && !_playerWallJump.isWallSliding)
                AudioManager.instance.StopSound("WallSlide");
        }
    }

    private void Fall()
    {
        //Fall Animation
        animator.SetFloat("AirSpeedY", rb.velocity.y);
        
        animator.SetBool("Grounded", IsGrounded());
    }

    private void DisablePlayerMovementUntilAnimationEnd()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("SpearCharge_Release") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("Block") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear2") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear3") )
        {
            playerVariables.canMove = false;
            
        }
        else
        {
            playerVariables.canMove = true;
        }
    }
    
    private void Move()
    {
        
        if (playerVariables.canMove)
        {
            movementDisabled = summonMorrigan || playerHealth.currentHealth <= 0;
            //horizontalInput = Input.GetAxis("Horizontal");
            Vector2 moveDirection = move.ReadValue<Vector2>();

            horizontalInput = moveDirection.x;
            
            //print("movedirection: " + moveDirection);

            // Check if character is currently moving
            if (Mathf.Abs(horizontalInput) > Mathf.Epsilon && Mathf.Sign(horizontalInput) == facingDirection)
                playerVariables.isMoving = true;
            else
                playerVariables.isMoving = false;

            if (!movementDisabled)
            {
                //set animator parameters run to true when movement is not 0
                if (horizontalInput != 0)
                {
                    animator.SetInteger("AnimState", 1);

                }
                else
                {
                    animator.SetInteger("AnimState", 0);
                }
                
                //Prevents player from turning around while walljumping
                if (!playerVariables.isWallJumping)
                {
                    //if player is moving right
                    if (horizontalInput > 0.01f && !isFacingRight)
                    {
                        //Debug.Log("got to not moving right");
                        //flip transform image to right
                        //transform.localScale = new Vector3(1f, 1f, 1);
                        transform.Rotate(0f, 180f, 0f);
                        facingDirection = 1;
                        isFacingRight = true;
                        character.SetCharacterFacingRight(true);
                    }
                    //if player is moving left
                    else if (horizontalInput < -0.01f && isFacingRight)
                    {
                        //Debug.Log("got to moving right");
                        //transform.localScale = new Vector3(-1f, 1f, 1);
                        transform.Rotate(0f, 180f, 0f);
                        facingDirection = -1;
                        isFacingRight = false;
                        character.SetCharacterFacingRight(false);
                    }
                }
                

                // SlowDownSpeed helps decelerate the characters when stopping
                float SlowDownSpeed = playerVariables.isMoving ? 1.0f : 0.5f;
                // Set movement
                if (!playerVariables.isWallJumping && !isDodging && !isLedgeGrabbing && !isLedgeClimbing && !isCrouching && m_parryTimer < 0.0f)
                {
                    if (isActiveCharacter)
                    {
                        rb.velocity = new Vector2(horizontalInput * m_speed * SlowDownSpeed, rb.velocity.y);
                        //var velocity = new Vector2(horizontalInput * m_speed * SlowDownSpeed, rb.velocity.y);
                        //rb.MovePosition(rb.position + new Vector2(velocity.x,rb.velocity.y) * Time.deltaTime);
                    }
                }

                
            }
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }
        
    }

    private void SwitchCharacter()
    {
        manaBar.TryUseMana(30);
        isActiveCharacter = !isActiveCharacter;
        //TODO this is why the crow doesnt 
        playerInput.currentActionMap.Disable();
        if (rb.velocity.y == 0 && rb.velocity.x != 0)
        {
            rb.velocity = new Vector2(0f, 0f);
        }
    }

    private void CheckIfCanJump()
    {
        if(IsGrounded() && rb.velocity.y <= 0)
        {

        }
    }

    private void ExitJump()
    {
        Debug.Log("got here");
    }

    private void Parry()
    {
        Debug.Log("Parry Function entered");
        // Parry
        // Used when you are in parry stance and something hits you
        if (m_parryTimer > 0.0f)
        {
            animator.SetTrigger("Parry");
            rb.velocity = new Vector2(-facingDirection * m_parryKnockbackForce, rb.velocity.y);
        }

        // Parry Stance
        // Ready to parry in case something hits you
        else
        {
            animator.SetTrigger("ParryStance");
            m_parryTimer = 7.0f / 12.0f;
        }
    }

    public void DamageEnemy()
    {
        playerCombat.Attack();
    }

    private void Block()
    {
        animator.SetTrigger("Block");
        animator.SetBool("IdleBlock", true);

    }

    public bool IsGrounded()
    {
        return character.IsGrounded();
        /*RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        RaycastHit2D raycastHitPlatform = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0,
            Vector2.down, 0.1f, platformLayer);
        return raycastHit.collider != null || raycastHitPlatform.collider != null;*/
    }

    private bool OnWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    void RespawnHero()
    {
        Debug.Log("Player Respawned");
        transform.position = Vector3.zero;
        isDead = false;
        animator.Rebind();
    }

    private float Horizontal
    {
        get
        {
            var keyboard = Keyboard.current;
            var horizontal = 0;
            if (keyboard.dKey.isPressed)
            {
                //Debug.Log("true");

                horizontal = 1;
            } else if(keyboard.aKey.isPressed)
            {
                horizontal = -1;
            }
            return horizontal;
        }
    }

    public PlayerJump PlayerJump
    {
        get { return _playerJump; }
    }

    public PlayerChangeWeapon ChangeWeapon
    {
        get { return _changeWeapon; }
    }

    public PlayerSpearThrow PlayerSpearThrow
    {
        get { return playerSpearThrow; }
    }
    
    public AttackSystem AttackSystem
    {
        get { return _attackSystem; }
    }

    public PlayerBlock PlayerBlock
    {
        get { return playerBlock; }
    }


    private void Roll()
    {
        Debug.Log("Roll Function entered");
    }

    private void Dodge()
    {
        if (isActiveCharacter)
        {
            Debug.Log("Dodge Function entered");
            if (IsGrounded() && !isDodging && !isLedgeGrabbing && !isLedgeClimbing)
            {
                isDodging = true;
                isCrouching = false;
                animator.SetBool("Crouching", false);
                animator.SetTrigger("Dodge");
                rb.velocity = new Vector2(facingDirection * m_dodgeForce, rb.velocity.y);
            }
        }
    }

    private void Crouch()
    {
        Debug.Log("Crouch Function entered");
    }

    private void Throw()
    {
        if (isActiveCharacter)
        {
            Debug.Log("Throw Function entered");
            if (IsGrounded() && !isDodging && !isLedgeGrabbing && !isLedgeClimbing)
            {
                animator.SetTrigger("Throw");

                // Disable movement 
                m_disableMovementTimer = 0.20f;
            }
        }
    }

    private void LedgeClimb()
    {
        Debug.Log("LedgeClimb Function entered");
    }

    private void LedgeFall()
    {
        Debug.Log("LedgeFall Function entered");
    }

    // Called in AE_resetDodge in PrototypeHeroAnimEvents
    public void ResetDodging()
    {
        isDodging = false;
    }

    // Function used to spawn a dust effect
    // All dust effects spawns on the floor
    // dustXoffset controls how far from the player the effects spawns.
    // Default dustXoffset is zero
    public void SpawnDustEffect(GameObject dust, float dustXOffset = 0, float dustYOffset = 0)
    {
        if (dust != null)
        {
            // Set dust spawn position
            Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * facingDirection, dustYOffset, 0.0f);
            GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity) as GameObject;
            // Turn dust in correct X direction
            newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(facingDirection, 1, 1);
        }
    }

    void DisableWallSensors()
    {
        isLedgeGrabbing = false;
        _playerWallJump.isWallSliding = false;
        isLedgeClimbing = false;
        m_wallSensorR1.Disable(0.8f);
        m_wallSensorR2.Disable(0.8f);
        m_wallSensorL1.Disable(0.8f);
        m_wallSensorL2.Disable(0.8f);
        rb.gravityScale = m_gravity;
        animator.SetBool("WallSlide", _playerWallJump.isWallSliding);
        animator.SetBool("LedgeGrab", isLedgeGrabbing);
    }

   

    public void SetPositionToClimbPosition()
    {
        transform.position = m_climbPosition;
        rb.gravityScale = m_gravity;
        m_wallSensorR1.Disable(3.0f / 14.0f);
        m_wallSensorR2.Disable(3.0f / 14.0f);
        m_wallSensorL1.Disable(3.0f / 14.0f);
        m_wallSensorL2.Disable(3.0f / 14.0f);
        isLedgeGrabbing = false;
        isLedgeClimbing = false;
    }

    public bool IsWallSliding()
    {
        return _playerWallJump.isWallSliding;
    }

    public void DisableMovement(float time = 0.0f)
    {
        m_disableMovementTimer = time;
    }

    public void EnableMovement()
    {
        m_disableMovementTimer = 0;
    }
    
    public void SetAnimationSpeed(float speed = 0.0f)
    {
        m_animationSpeed = speed;
    }
    
    public float GetAnimationSpeed()
    {
        return m_animationSpeed;
    }

    public void ChangeFacingDirection()
    {
        transform.Rotate(0f, 180f, 0f);
        character.SetCharacterFacingRight(!isFacingRight);
        isFacingRight = !isFacingRight;
        facingDirection = -facingDirection;
        //StartCoroutine(WallJumpCool)
    }
    
    private void OnEnable()
    {
        move = playerInput.actions["Move"];
        move.Enable();

        

        moveCrowToPosition = playerInput.actions["MoveCrowToPosition"];
        moveCrowToPosition.performed += _ => TryUseMindFog();

        //attack = playerControls.Player.Attack;
        //attack.Enable();
        ///attack.started += _ => AttackStart();
        //attack.performed += _ => ChargeAttack();
        //attack.canceled += _ => AttackEnd();
        //spearChargeAttack = playerControls.Player.SpearChargeAttack;
        //attack.Enable();
        //attack.performed += _ => Attack();

        PlayerJump.jumpButtonPressed = false;
        jump = playerInput.actions["Jump"];
        jump.Enable();
        jump.started += _ => PlayerJump.Jump();
        jump.performed += _ => PlayerJump.jumpButtonPressed = true;
        jump.canceled += _ => PlayerJump.jumpButtonPressed = false;

        //parry = playerControls.Player.Parry;
        //parry.Enable();
        //parry.performed += _ => Parry();

        dodge = playerInput.actions["Dodge"];
        dodge.Enable();
        dodge.performed += _ => Dodge();

        throw_attack = playerInput.actions["Throw"];
        throw_attack.Enable();
        throw_attack.performed += _ => Throw();

        upButtonPressed = false;
        up = playerInput.actions["UpButton"];
        up.Enable();
        up.started += _ => upButtonPressed = true;
        up.performed += _ => upButtonPressed = true;
        up.canceled += _ => upButtonPressed = false;

        downButtonPressed = false;
        down = playerInput.actions["dwn"];
        down.Enable();
        down.started += _ => test();
        down.performed += _ => test();
        down.canceled += _ => test2();
    }

    public void test()
    {
        //print("We got here");
        downButtonPressed = true;
    }

    public void test2()
    {
        //print("We got here");
        downButtonPressed = false;
    }

    public bool IsUpButtonPressed()
    {
        return upButtonPressed;
    }

    public bool IsDownButtonPressed()
    {
        return downButtonPressed;
    }

    private void TryUseMindFog()
    {
        //Only move the crow to mind fog position if they have the mana
        if (manaBar.TryUseMana(10))
        {
            var crowMovePoint = new GameObject("crowMovePoint");
            crowMovePoint.transform.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            //print("crowMovePoint: " + crowMovePoint.transform.position);
            crowMovePointTransform = crowMovePoint.transform;
            crowMovement.isAutoFlyingToLocation = true;
        }
    }

    /***************************** Switch Between Weapons ****************************/

    /******************************** Attack ****************************************/


    //boilerplate code for new input system
    private void OnDisable()
    {
        move.Disable();
        //attack.Disable();
        jump.Disable();
        //parry.Disable();
        up.Disable();
        down.Disable();
        block.Disable();
        moveCrowToPosition.Disable();
    }

}
