using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class AttackSystem : MonoBehaviour
{
    [Header("Input Manager")]
    [SerializeField] public GameObject inputManager;
    
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] Transform attackPoint;
    [SerializeField] public float attackRange = 0.5f;
    [SerializeField] public LayerMask enemyLayers;

    
    [SerializeField] public SpearRotation spearRotation;
    public float cooldownTime = 0.5f;
    private float nextFireTime = 0f;
    public  bool hasAttackQueued = false;
    private float lastClickedTime = 0;
    private float maxComboDelay = 1;
    
    
    public int nextAttack = 0;
    //managed on update, whether any of the attack animations are in progress
    public bool isAnyAttackingAnimationInProgress;
    //This parameter is controlled by the animation only, to ensure the end of an animation sequence can be identified
    //Using the animator property we can tell when an animation was meant to "end"
    public bool isAnimationStillAttacking = false;
    
    private float spearChargetimer;
    private bool spearChargetimerOn;

    private float timeSinceAttack;

    private PlayerController _playerController;
    
    //public PlayerInputActions playerControls;
    private InputAction attack;
    private InputAction up;
    
    private int currentAttackCount = 0;
    
    private bool upButtonPressed = false;
    private bool downButtonPressed = false;
    private bool jumpButtonPressed = false;

    private bool isSpearCharging = false;
    
    public bool isAttacking = false;
    private float defenceWarningCountdown = 0f;
    
    private PlayerInput playerInput;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        playerInput = inputManager.GetComponent<PlayerInput>();
    }

    void Start()
    {
        //_playerController = GetComponent<PlayerController>();
    }

    void SpearThrow()
    {
        if (isSpearCharging)
        {
            
            ChangePlayerFacingDirection();
            /*_playerController.facingDirection
            if ()
            {
                
            }*/
                
            GameObject.Find("Player/SpearThrowPoint/AimAssistArrow").SetActive(true);
            //var spearRotation = GameObject.Find("Player/SpearThrowPoint/AimAssistArrow").GetComponent<SpearRotation>();
            if (spearRotation.lockedPosition == Vector2.zero)
            {
                spearRotation.lockedPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                //var tmpScreenPos = Camera.main.WorldToScreenPoint(Mouse.current.position.ReadValue() *3);
            }
            //print("spearRotation.lockedPosition: " + spearRotation.lockedPosition);
        }
        else
        {
            GameObject.Find("Player/SpearThrowPoint/AimAssistArrow").SetActive(false);
            spearRotation.lockedPosition = Vector2.zero;
        }

        if (playerVariables.isSpearThrown)
        {
            hasAttackQueued = false;
        }

        SpearChargeTimer();

        //DisablePlayerMovementUntilAnimationEnd("SpearCharge_Release");
    }

    

    private void ChangePlayerFacingDirection()
    {
        //logic to check if player should change facing direction while charging
        Vector2 spearThrowPointPosition = GameObject.Find("SpearThrowPoint").transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        float throwDirection = mousePosition.x - spearThrowPointPosition.x;
        //print("throw direction: " + throwDirection);
        //print("facing direction: " + _playerController.facingDirection);
        // _playerController.ChangeFacingDirection();

        // if facing direction is opposite direction to throw direction then change direction
        if ((throwDirection > 0 && _playerController.facingDirection < 0)
            || (throwDirection < 0 && _playerController.facingDirection > 0))
        {
            _playerController.ChangeFacingDirection();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (_playerController.isActiveCharacter)
        {

            SpearThrow();

            //not working???????????????????????????????????????????? why???????????
            //DisablePlayerMovementUntilAnimationEnd("MeleeSpear1");
           // DisablePlayerMovementUntilAnimationEnd("MeleeSpear2");
           // DisablePlayerMovementUntilAnimationEnd("MeleeSpear3");
            

            if (!playerVariables.isSpearThrown)
            {
                if (!isAnyAttackingAnimationInProgress)
                {
                    //fresh start seems to be needed to make sure attacks don't loop continually as they are booleans.
                    //Using the animator property we can tell when an animation was meant to "end"
                    animator.SetBool("hit2", false);
                    animator.SetBool("hit1", false);
                    animator.SetBool("hit3", false);
                    //This is for enemies to know when to defend
                    SetPlayerIsAttacking(false);
                }

                isAnyAttackingAnimationInProgress = animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1")
                                                    || animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear2")
                                                    || animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear3");

                
                
                if (!isAnyAttackingAnimationInProgress && hasAttackQueued)
                {
                    if (nextAttack == 0)
                    {
                        nextAttack++;
                    }

                    //nextAttack++;
                    //print("next attack " + nextAttack);
                    if (nextAttack == 1) //&& animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1") 
                    {
                        //nextAttack = 0;

                        animator.SetBool("hit1", true);
                        animator.SetBool("hit2", false);
                        animator.SetBool("hit3", false);
                        hasAttackQueued = false;
                        nextAttack++;
                        //playerMovement.SetPlayerIsAttacking(true);
                    }
                    else if (nextAttack == 2) //&& animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1") 
                    {
                        //nextAttack = 0;

                        animator.SetBool("hit1", false);
                        animator.SetBool("hit2", true);
                        animator.SetBool("hit3", false);
                        hasAttackQueued = false;
                        nextAttack++;
                        //playerMovement.SetPlayerIsAttacking(true);
                    }
                    else if (nextAttack == 3) //&& animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1") 
                    {
                        //nextAttack = 0;

                        animator.SetBool("hit1", false);
                        animator.SetBool("hit2", false);
                        animator.SetBool("hit3", true);
                        hasAttackQueued = false;
                        nextAttack = 0;
                        //playerMovement.SetPlayerIsAttacking(true);
                    }

                    //This is for enemies to know when to defend
                    SetPlayerIsAttacking(true);
                }
                else if (!isAnyAttackingAnimationInProgress && !hasAttackQueued)
                {
                    nextAttack = 0;
                }
                else if (isAnyAttackingAnimationInProgress && hasAttackQueued)
                {
                }
                else if (isAnyAttackingAnimationInProgress && !hasAttackQueued)
                {
                }

                if (nextAttack > 3)
                {
                    //is this still needed?
                    //When would this be true if next attack only increments at 1 and resets to 0 at 2
                    nextAttack = 1;
                    hasAttackQueued = true;
                    //isAnimationStillAttacking = false;
                }


                /*
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.2f &&
                    animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1"))
                {
                    print("WE HERE");
                    animator.SetBool("hit1", false);
                }
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.2f &&
                    animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear2"))
                {
                    animator.SetBool("hit2", false);
                }*/

                //if (Time.time - lastClickedTime > maxComboDelay)
                //{
                //    noOfClicks = 0;
                //}
            }

        }
    }

    public void DisableAttack()
    {
        attack.Disable();
    }

    public void EnableAttack()
    {
        attack.Enable();
    }
    
    

    public void DisableAttacks()
    {
        animator.SetBool("hit1", false);
        animator.SetBool("hit2", false);
        animator.SetBool("hit3", false);
    }
    
    /******************************** Attack ****************************************/
    private void AttackStart()
    {
        //if (Time.time > nextFireTime)
        //{
            //lastClickedTime = Time.time;
            if (playerVariables.isSpearThrown)
            {
                playerVariables.retrieveSpearPressed = true;
                _playerController.PlayerSpearThrow.ThrowRetrieveSpear();
            }
            else
            {
                spearChargetimerOn = true;
                //Debug.Log("AttackStart");
                //hasAttackQueued = true;
            }
            
            //}
    }

    private void ChargeAttack()
    {
        
        animator.SetBool("ChargingSpearAttack", true);
    }
    

    private void AttackEnd()
    {
        if (spearChargetimer < 0.3&& !playerVariables.isSpearThrown 
                                  && !playerVariables.retrieveSpearPressed 
                                  && !playerVariables.retrieveSpearPressed)
        {
            if (playerVariables.isSpearThrown == false)
            {
                hasAttackQueued = true;
                //Debug.Log("is less than two");
                Attack();
            }
            
        }
        else
        {
            //Debug.Log("is more than two");
            
        }
        spearChargetimerOn = false;
        
        
    }

    private void OnEnable()
    {

        attack = playerInput.actions["Attack"];
        attack.Enable();
        attack.started += _ => AttackStart();
        attack.performed += _ => ChargeAttack();
        attack.canceled += _ => AttackEnd();
        
        upButtonPressed = false;
        up = playerInput.actions["UpButton"];
        up.Enable();
        up.started += _ => upButtonPressed = true;
        up.performed += _ => upButtonPressed = true;
        up.canceled += _ => upButtonPressed = false;
    }

    private void OnDisable()
    {
        attack.Disable();
    }
    
    private void Attack()
    {
        spearChargetimerOn = true;
        //animator.SetBool("hit1", true);

        //nextAttack++;
        
        /*if (nextAttack == 0)
        {
            //animator.SetBool("hit1", true);
            nextAttack++;
        }
        else */
        


        /*if (noOfClicks == 1 && animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1"))
        {
            animator.SetBool("hit1", true);
            //playerMovement.SetPlayerIsAttacking(true);
        }
        
        noOfClicks = Mathf.Clamp(noOfClicks, 0, 2);

        if (noOfClicks == 2 && animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1"))
        {
            animator.SetBool("hit2", true);
        }
        */

        /*if (noOfClicks == 1)
        {
            animator.SetBool("hit1", true);
            //playerMovement.SetPlayerIsAttacking(true);
        }
        
        
        noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);

        if (noOfClicks >= 2 && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.2f &&
            animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear1"))
        {
            
            //animator.SetBool("hit1", false);
            animator.SetBool("hit2", true);
        }*/

        /*if (noOfClicks >= 3 && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.2 &&
            animator.GetCurrentAnimatorStateInfo(0).IsName("MeleeSpear2"))
        {
            
            animator.SetBool("hit2", false);
            animator.SetBool("hit3", true);
        }*/





    }
    
    void SpearChargeTimer()
    {
        if (spearChargetimerOn == true)
        {
            spearChargetimer += Time.deltaTime;
            if (spearChargetimer > 0.2 && isSpearCharging == false)
            {
                isSpearCharging = true;
                animator.SetTrigger("ChargingSpearAttackStart");
                _playerController.move.Disable();
                
            }
        }
        else
        {
            spearChargetimer = 0;
            isSpearCharging = false;
            animator.SetBool("ChargingSpearAttack", isSpearCharging);
            _playerController.move.Enable();
        }
    }
    
    
    private void AttackOLD()
    {
        spearChargetimerOn = true;
        
        //Up Attack
        if (_playerController.upButtonPressed && !_playerController.isDodging && !_playerController.isLedgeGrabbing && !_playerController.isLedgeClimbing && !_playerController.isCrouching && _playerController.IsGrounded() && timeSinceAttack > 0.2f)
        {
            _playerController.animator.SetTrigger("UpAttack");
            

            // Reset timer
            timeSinceAttack = 0.0f;

            // Disable movement 
            //m_disableMovementTimer = 0.35f;
        }
        if (_playerController.downButtonPressed && !_playerController.isLedgeGrabbing && !_playerController.isLedgeClimbing && !_playerController.IsGrounded())
        {
            AirSlam();
        }
        else if (_playerController.upButtonPressed && !_playerController.isDodging && !_playerController.isLedgeGrabbing && !_playerController.isLedgeClimbing && !_playerController.isCrouching && !_playerController.IsGrounded() && timeSinceAttack > 0.2f)
        {
            AttackAirUp();

        }
        else if (!_playerController.isDodging && !_playerController.isLedgeGrabbing && !_playerController.isLedgeClimbing && !_playerController.isCrouching && !_playerController.IsGrounded() && timeSinceAttack > 0.2f)
        {
            AirAttack();

        }
        else if(timeSinceAttack > 0.5f)
        {
            currentAttackCount++;

            // Loop back to one after third attack
            if (currentAttackCount > 2)
                currentAttackCount = 1;

            // Reset Attack combo if time since last attack is too large
            if (timeSinceAttack > 1.0f)
                currentAttackCount = 1;

            //if moving then play moving attack animation
            if(_playerController.horizontalInput != 0)
            {
                // Call one of three attack animations "Attack1", "Attack2", "Attack3"
                _playerController.animator.SetTrigger("AttackMoving" + currentAttackCount);
                SetPlayerIsAttacking(true);
            }
            else
            {
                // Call one of three attack animations "Attack1", "Attack2", "Attack3"
                _playerController.animator.SetTrigger("Attack" + currentAttackCount);
                SetPlayerIsAttacking(true);
            }
            
            
            // detect enemies in range of attack
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, _playerController.enemyLayers);
            //Added as event on sprite attacks to sync damage with attack animation
            //playerCombat.Attack();
            
        }
        // Reset timer
        timeSinceAttack = 0.0f;
    }

    private void AttackAirUp()
    {
        Debug.Log("Air attack up");
        _playerController.animator.SetTrigger("AirAttackUp");
    }

    private void AirSlam()
    {
        Debug.Log("Air Slam Function entered");
        _playerController.animator.SetTrigger("AttackAirSlam");
        _playerController.rb.velocity = new Vector2(0.0f, -_playerController.PlayerJump.m_jumpForce);
        _playerController.m_disableMovementTimer = 0.8f;
    }

    private void AirAttack()
    {
        //Debug.Log("AirAttack Function entered");
        _playerController.animator.SetTrigger("AirAttack");
    }
    
    public bool isPlayerAttacking() {
        return isAttacking; 
    }

    public bool SetPlayerIsAttacking(bool isAttackingNewVal)
    {

        isAttacking = isAttackingNewVal;
        playerVariables.isAttacking = isAttacking;
        return isAttacking;
    }
    
    //this will need to be moved into another class for each enemy
    public void PlayerAttackingWarning()
    {
        if (isPlayerAttacking() && defenceWarningCountdown < 5f)
        {
            defenceWarningCountdown++;
        }
        else if (isPlayerAttacking() && defenceWarningCountdown >= 5f)
        {
            SetPlayerIsAttacking(false);
            defenceWarningCountdown = 0;
        }
    }
}

