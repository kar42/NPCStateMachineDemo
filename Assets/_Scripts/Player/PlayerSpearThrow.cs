using System;
using Pathfinding;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class PlayerSpearThrow : MonoBehaviour
{
    
    [FormerlySerializedAs("PlayerVariables")]
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    [Header("Attack")]
    [SerializeField] public PlayerController playerController;
    [SerializeField] public AttackSystem attackSystem;

    [SerializeField] private Transform attackPoint;
    [SerializeField] public float attackRange = 0.5f;
    [SerializeField] public int attackDamage = 3;
    [SerializeField] public float attackRate = 2;
    [SerializeField] public float nextAttackTime = 0;

    [Header("Spear Throw")]
    [SerializeField] public float spearRetrievalSpeed;

    [SerializeField] public float triggerSpearRetrieveAnimationDistance;
    [SerializeField] public float closestDistanceFromSpearToPlayer;

    [SerializeField] public float spearRetrievalSpinIntensity;
    public Path path;
    public int currentWayPoint = 0;
    public Seeker seeker;
    public GameObject spear;
    public bool reachedEndOfPath = false;
    public float nextWaypointDistance = 0.1f;
    private bool isSpearRetrieving = false;
    private bool alreadyTriggeredSpearCallback = false;
    private float spearChargetimer;
    private bool spearChargetimerOn;
    private bool isSpearCharging = false;

    public float timeSinceAttack { get; set; }

    private Vector2 direction;

    public bool retrieveSpearpressed;
    private void Awake()
    {
        InvokeRepeating("UpdatePath", 0f, .1f);
        playerVariables.retrieveSpearPressed = false;
    }

    private void FixedUpdate()
    {
    }

    private void Update()
    {
        retrieveSpearpressed = playerVariables.retrieveSpearPressed;
        
        if (playerVariables.retrieveSpearPressed && spear != null)
        {
            
            //Debug.Log(spear.GetComponent<Rigidbody2D>().velocity);
            //Debug.Log("Got eeeeeeeeeeeeeeeeeeeeeeeeeeeeeee");
            //Debug.Log("path: " + path);
            if (path == null)
            {
                print("path was null");
                return;
            }
            //check if were at the end of the path
            if (currentWayPoint >= path.vectorPath.Count)
            {
                reachedEndOfPath = true;
                playerVariables.retrieveSpearPressed = false ;

            }
            else
            {
                reachedEndOfPath = false;
            }
            
            MoveSpear();
        }
    }

    public void ThrowRetrieveSpear()
    {
        //is spear already thrown?
        if(playerVariables.isSpearThrown)
        {
            spear = GameObject.Find("Spear(Clone)");
            
            Rigidbody2D spearRB = spear.GetComponent<Rigidbody2D>();
            Animator spearAnim = spear.GetComponentInChildren<Animator>();
            BoxCollider2D spearBoxCollider = spear.GetComponent<BoxCollider2D>();
            spearAnim.SetBool("spearstuck", false);
            
            //Unfreeze any characters that spear was attached to
            var spearTip = spear.GetComponentInChildren<SpearTip>();
            spearTip.ClearCharacterFreeze();
            
            if (spear.GetComponentInParent<Transform>().tag == "Liftable")
            {
                //reapply physics to spear
                spearRB.isKinematic = false;
                //spear.GetComponentInParent<Rigidbody2D>().isKinematic = true;
                //detatch spear from whatever it is stuck into
                //spearRB.transform.parent = null;
                //re-enable box collider on spear
                spearRB.transform.parent = null;
                spearBoxCollider.enabled = true;
                spearBoxCollider.isTrigger = true;
            }
            else
            {
                //MonoBehaviour.print("we got set");
                //reapply physics to spear
                spearRB.isKinematic = false;
                //detatch spear from whatever it is stuck into
                spearRB.transform.parent = null;
                //re-enable box collider on spear
                spearBoxCollider.enabled = true;
                spearBoxCollider.isTrigger = true;
            }

            seeker = spear.GetComponentInChildren<Seeker>();

            playerVariables.retrieveSpearPressed = true;
            //BoxCollider2D speartipBox = GameObject.Find("Spear/SpearTip").GetComponent<BoxCollider2D>();
            //speartipBox.isTrigger = false;
            //spearRB.GetComponentInChildren<BoxCollider2D>().enabled = false;
            //Debug.Log("Throw Spear Retrieved executed");
            //spearThrown = false;
            RotateSpear();
            //BoxCollider2D speartipBox = GameObject.Find("Spear(Clone)/SpearTip").GetComponent<BoxCollider2D>();
            //speartipBox.isTrigger = false;
        }
        else
        {

            Debug.Log("Throw Spear executed");
            //playerController.animator.SetTrigger("SpearThrow");
            AudioManager.instance.PlaySound("SpearThrow_sfx");
            playerVariables.isSpearThrown = true;
            //spearRB.GetComponentInChildren<BoxCollider2D>().enabled = false;
            //BoxCollider2D speartipBox = GameObject.Find("Spear/SpearTip").GetComponent<BoxCollider2D>();
            //speartipBox.isTrigger = true;
            ChangePlayerThrowDirection();
        }
    }

    private void ChangePlayerThrowDirection()
    {
        Vector2 spearThrowPointPosition = GameObject.Find("SpearThrowPoint").transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        float throwDirection = mousePosition.x - spearThrowPointPosition.x;

        //Change the facing direction of the player if they are throwing the spear behind them
        if (throwDirection < 0 && playerController.facingDirection == 1)
        {
            playerController.ChangeFacingDirection();
        }
        else if (throwDirection > 0 && playerController.facingDirection == -1)
        {
            playerController.ChangeFacingDirection();
        }
    }
    
    public void MoveSpear()
    {
        //m_animator.SetBool("Moving", true);

        //Debug.Log("Archer Moving");
        //move 
        Rigidbody2D spearRB = spear.GetComponent<Rigidbody2D>();
        try
        {
            direction = ((Vector2)path.vectorPath[currentWayPoint] - spearRB.position).normalized;
        }
        catch (ArgumentOutOfRangeException e)
        {
            
            //Debug.Log("ArgumentOutOfRangeException OCCURRED");
            direction = ((Vector2)path.vectorPath[currentWayPoint-1] - spearRB.position).normalized;
        }
        //Debug.Log(direction.x);
        //Vector2 force = direction * m_speed * Time.deltaTime;
        //Debug.Log(force);
        //m_body2d.AddForce(force);
        //Debug.Log(force);
       
        spearRB.gravityScale = 0;
        var velocity = new Vector2(direction.x * spearRetrievalSpeed, direction.y * spearRetrievalSpeed);
        spearRB.MovePosition(spearRB.position + velocity * Time.fixedDeltaTime);
        //set animator parameters run to true when movement is not 0
        //m_animator.SetBool("Run", m_body2d.velocity.x != 0);

        float distance = 0f;
        try
        {
            distance = Vector2.Distance(spear.transform.position, path.vectorPath[currentWayPoint]);
        }
        catch (ArgumentOutOfRangeException e)
        {
            //Debug.Log("ArgumentOutOfRangeException OCCURRED");
            distance = Vector2.Distance(spear.transform.position, path.vectorPath[currentWayPoint-1]);
        }


        if (distance < nextWaypointDistance)
        {
            //Debug.Log(distance);
            currentWayPoint++;
        }

        //destory the spear object if retrieveSpearPressed and is close enough
        if (playerVariables.retrieveSpearPressed)
        {
            float distanceFromPlayer = Vector2.Distance(spearRB.position, playerController.rb.position);
            //Debug.Log("spearRB.position: " + spearRB.position);
           // Debug.Log("rb.position: " + playerController.rb.position);
            //Debug.Log("distance from player: " + distanceFromPlayer);
            //Debug.Log("triggerSpearRetrieveAnimationDistance: " + triggerSpearRetrieveAnimationDistance);

            //Trigger Spear Callback animation and rotate player to face spear and align spear orientation correctly
            if ((distanceFromPlayer < triggerSpearRetrieveAnimationDistance) && !alreadyTriggeredSpearCallback)
            {
                //Debug.Log("close enough to player and not triggered callback before");
                alreadyTriggeredSpearCallback = true;
                playerController.animator.SetTrigger("SpearCallbackEnd");

                //If the player is 
                bool isPlayerFacingOppositeDirection_Right = (((spearRB.position.x < playerController.rb.position.x - 2 || spearRB.position.x < playerController.rb.position.x + 2)) && playerController.facingDirection == 1);
                bool isPlayerFacingOppositeDirection_Left = (((spearRB.position.x > playerController.rb.position.x - 2 || spearRB.position.x > playerController.rb.position.x + 2)) && playerController.facingDirection == -1);

                //Debug.Log("isPlayerFacingOppositeDirection_Right: " + isPlayerFacingOppositeDirection_Right);
                //Debug.Log("isPlayerFacingOppositeDirection_Left: " + isPlayerFacingOppositeDirection_Left);
                if (isPlayerFacingOppositeDirection_Right || isPlayerFacingOppositeDirection_Left) //|| isPlayerFacingOppositeDirection_Left)
                {
                    playerController.ChangeFacingDirection();
                }
                RotateSpear();
            }

            if (distanceFromPlayer < closestDistanceFromSpearToPlayer)
            {
               // Debug.Log("Spear Retrieval End");
                Object.Destroy(spear);
                playerVariables.retrieveSpearPressed = false;
                playerVariables.isSpearThrown = false;
                alreadyTriggeredSpearCallback = false;
                AudioManager.instance.PlaySound("SpearRetrievalEnd_sfx");
                currentWayPoint = 0;
            }
        }
    }

    private void RotateSpear()
    {
        Rigidbody2D spearRB = spear.GetComponent<Rigidbody2D>();
        /*float spearSpeed = spearRB.velocity.magnitude;
        float spearAngularSpeed = spearRB.angularVelocity;*/

        if(playerController.facingDirection == 1)
        {
            spearRB.AddTorque(spearRetrievalSpinIntensity);
        } else if(playerController.facingDirection == -1)
        {
            spearRB.AddTorque(-spearRetrievalSpinIntensity);
        }
        
    }

    public bool IsSpearThrown(bool spearThrownValue)
    {
        playerVariables.isSpearThrown = spearThrownValue;
        return playerVariables.isSpearThrown;
    }
    
    
    void UpdatePath()
    {
        //Debug.Log(seeker);
        //Debug.Log(spear);
        /*if (spear != null && seeker != null)
        {*/
        //Debug.Log("got here");
        if (seeker != null && seeker.IsDone())
        {
            seeker.StartPath(spear.transform.position, playerController.rb.position, OnPathComplete);
                
        }
        //}
    }
    
    

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }
    
}