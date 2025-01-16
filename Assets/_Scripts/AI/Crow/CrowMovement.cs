using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Pathfinding;
using SupportScripts;
using UnityEngine.Rendering.Universal;

public class CrowMovement : MonoBehaviour
{
    [Header("Input Manager")]
    [SerializeField] public GameObject inputManager;

    [Header("Movement Options")] [SerializeField]
    float m_speed = 70f;
    private bool canMove = true;
    private bool movingFast;
    private bool movementDisabled;
    private int m_facingDirection = 1;

    [SerializeField] float m_fastSpeed = 100f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Follow Options")] [SerializeField]
    protected Transform target;

    [SerializeField] protected float nextWaypointDistance = 3f;
    [SerializeField] float followDistance = 50f;

    [Header("Mana")] [SerializeField] public ManaBar manaBar;

    [SerializeField] public int mindFogManaCost = 20;
    //[SerializeField] public int mindFogManaCost = 20;

    public bool isFollowing = true;

    // CROW CHARACTERISTICS
    private Rigidbody2D rb;
    public Animator animator;
    private BoxCollider2D boxCollider;
    private float crowMindFogDistance = 100.0f;

    // PATHFINDING VARIABLES
    private Seeker seeker;
    private Path path;
    private int currentWayPoint = 0;
    private bool reachedEndOfPath = false;
    private float followSpeed = 100f;

    private bool m_rolling = false;
    private int m_currentAttack = 0;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private bool m_touchingWall = false;
    private bool m_isWallSliding = false;
    private float wallJumpCooldown;
    private float horizontalInput;
    private float verticalInput;
    private bool summonMorrigan;

    public bool isActiveCharacter = false;

    // MIND FOG CODE
    private bool castMindFog;
    private List<AbstractEnemy> mindFoggedEnemies = new List<AbstractEnemy>();
    private Vector2 mindFogStartPosition;
    private Vector2 mindFogStopPosition;

    private PlayerController _playerController;

    public bool isAutoFlyingToLocation = false;


    private bool previousFrameGrappled = false;

    //private bool isPreviouslyGrappled = false;
    private CrowFollowPoint crowFollowPoint;
    private CrowWallDetector crowWallDetector;

    /**
     *************** INITIALIZATION AND UPDATE CODE **********************************
    **/
    private void Awake()
    {
        
        seeker = GetComponent<Seeker>();
        //these grab refs to rigidbody and animator of charactor
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        //crowControls = new PlayerInputActions();
        movementDisabled = true;
        summonMorrigan = false;
        movingFast = false;
        castMindFog = false;
        _playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        crowFollowPoint = GameObject.Find("CrowFollowPoint").GetComponent<CrowFollowPoint>();
        crowWallDetector = GameObject.Find("CrowWallDetector").GetComponent<CrowWallDetector>();

        //InvokeRepeating("UpdatePath", 0f, .1f);
    }

    private void FixedUpdate()
    {
        if (manaBar.CanUseMana(10) )
        {
            if (!GetComponent<Light2D>().isActiveAndEnabled)
            {
                GetComponent<Light2D>().enabled = true;
            }
        }
        else if(GetComponent<Light2D>().isActiveAndEnabled)
        {
            try
            {
                GetComponent<Light2D>().enabled = false;
            }
            catch(Exception e)
            {
                print("gameobject not found");
            }
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        //print("isautoflying to location: " +  isAutoFlyingToLocation);
        animator.SetFloat("AirSpeedY", rb.velocity.y);
        animator.SetFloat("AirSpeedX", rb.velocity.x);
        //Debug.Log("isFollowing: " + isFollowing);
        //Debug.Log("isActiveCharacter: " + isActiveCharacter);
        if (!isActiveCharacter)
        {
            IsTooFarAway();
        }

        
        if (isAutoFlyingToLocation)
        {
            UpdatePath(_playerController.crowMovePointTransform);
                MoveToMouseClickPosition();
                MoveToMouseClickPosition();
                isFollowing = false;
                castMindFog = true;
        }
        
        else if (isFollowing)
        {
            if (crowFollowPoint.isCollidingWithEnv && crowWallDetector.isCollidingWithEnv)
            {
                rb.velocity = new Vector2(0, 0);
            }
            else
            {
                UpdatePath(target);
                FollowPlayer();
                FollowPlayer();
            }

        }
        else
        {
            if (canMove && isActiveCharacter)
            {
                MoveCrow();
                boxCollider.enabled = true;
            }
            else
            {
                rb.velocity = new Vector2(0, 0);
                boxCollider.enabled = false;
            }
        }

        CastMindFog();
    }


    private void IsTooFarAway()
    {

        Vector2 crowPos = new Vector2(rb.position.x, rb.position.y);
        Vector2 playerPos = new Vector2(_playerController.rb.position.x, _playerController.rb.position.y);
        var distance = Vector2.Distance(crowPos, playerPos);

        //Debug.Log(distance);
        if (distance > 140)
        {
            isAutoFlyingToLocation = false;
            isFollowing = true;
        }
    }


    void UpdatePath(Transform targetToFollow)
    {
        if (seeker.IsDone())
        {
            //print("target to follow position: " + targetToFollow.position);
            seeker.StartPath(rb.position, targetToFollow.position, OnPathComplete);
            /*if (HasTargetInSight())
            {
                m_seeker.StartPath(m_body2d.position, target.position, OnPathComplete);
            }
            else
            {
                //m_seeker.StartPath(m_body2d.position, -m_body2d.position* 100, OnPathComplete);
                if (paceCounter >= 0 && paceCounter < 5)
                {
                    //var test = new Vector3();
                    m_seeker.StartPath(m_body2d.position, target.position, OnPathComplete);
                    paceCounter++;
                }
                else
                {
                    m_seeker.StartPath(m_body2d.position, -target.position, OnPathComplete);
                    paceCounter--;
                }
            }*/
            //UpdateDirection();
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }

    /**
     *************** DIRECTION AND ORIENTATION **********************************
    **/
    private bool IsTooCloseToPlayer()
    {
        float distanceFromPlayer = Vector2.Distance(target.position, rb.position);
        //Debug.Log("distance: " + distanceFromPlayer);
        //Debug.Log("Distance " + distance);
        if (distanceFromPlayer < followDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsCloseEnoughToPlayer()
    {
        float distanceFromPlayer = Vector2.Distance(target.position, rb.position);
        //Debug.Log("Distance " + distance);
        if (distanceFromPlayer > followDistance)
        {
            return false;
        }

        if (distanceFromPlayer < followDistance)
        {
            return true;
        }
        else
        {
            return true;
        }
    }

    private bool IsAtMousePosition()
    {

        float distanceFromMousePosition =
            Vector2.Distance(_playerController.crowMovePointTransform.position, rb.position);
        //Debug.Log("Distance " + distance);
        if (distanceFromMousePosition > 1)
        {
            return false;
        }

        if (distanceFromMousePosition <= 1)
        {
            return true;
        }
        else
        {
            return true;
        }
    }

    private void ChangeFacingDirection()
    {
        //Changes the current target so the facing driection will be looking towards it 
        var currentTarget = new Vector2();
        if (isAutoFlyingToLocation) currentTarget = _playerController.crowMovePointTransform.position;
        else currentTarget = target.transform.position;

        //if player is moving right
        if (currentTarget.x - rb.transform.position.x > 0.01)
        {
            //flip transform image to right
            transform.localScale = new Vector3(0.5f, 0.5f, 1);
            m_facingDirection = 1;
        }
        //if player is moving left
        else if (currentTarget.x - rb.transform.position.x < 0.01)
        {
            transform.localScale = new Vector3(-0.5f, 0.5f, 1);
            m_facingDirection = -1;
        }
    }

    private Vector2 PositionOfGroundBeneathCrow()
    {
        var downwardScoutPositionX = boxCollider.bounds.center.x;
        var downwardScoutPositionY = (boxCollider.bounds.center.y - boxCollider.bounds.extents.y) - 2000;

        var genericDownwardDirection = new Vector2(downwardScoutPositionX, downwardScoutPositionY);

        var rayCastsBeneathCrow =
            Physics2D.LinecastAll(boxCollider.ClosestPoint(genericDownwardDirection), genericDownwardDirection);

        foreach (RaycastHit2D rayCast in rayCastsBeneathCrow)
        {
            if (rayCast.transform)
            {
                return rayCast.point;
            }
        }

        //when would ground not be found? if this is returned, nothing should happen?
        return transform.position;
    }

    private bool IsIdle()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down,
            0.1f, groundLayer);
        return raycastHit.collider != null;
    }



    /**
     *************** PASSIVE MOVEMENT **********************************
    **/

    private void FollowPlayer()
    {

        if (path == null)
        {
            return;
        }

        //check if were at the end of the path
        if (currentWayPoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
        }
        else
        {
            reachedEndOfPath = false;
        }

        //if(target.position)
        if (IsCloseEnoughToPlayer() && IsTooCloseToPlayer())
        {
            //Debug.Log("IsCloseEnoughToPlayer && IsTooCloseToPlayer(");
            //Flutter();
            //MoveAway();
        }

        if (!IsCloseEnoughToPlayer())
        {
            AutoMove();
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }
    }

    private void Flutter()
    {
        Vector2 direction = ((Vector2) path.vectorPath[currentWayPoint] - rb.position).normalized;
        rb.velocity = new Vector2(5 * m_speed, 5 * m_speed);
        //set animator parameters run to true when movement is not 0
        //m_animator.SetBool("Run", m_body2d.velocity.x != 0);

        ChangeFacingDirection();


        if (rb.velocity.x != 0)
        {
            animator.SetBool("Moving", true);
        }
        else if (rb.velocity.x == 0)
        {
            animator.SetBool("Moving", false);
        }
    }

    private void MoveToMouseClickPosition()
    {

        if (path == null)
        {
            return;
        }

        //check if were at the end of the path
        if (currentWayPoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
        }
        else
        {
            reachedEndOfPath = false;
        }

        if (!IsAtMousePosition())
        {
            AutoMove();
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
            isAutoFlyingToLocation = false;
        }
    }

    private void FreezeOrFollow()
    {
        isFollowing = !isFollowing;
    }

    private void MoveAway()
    {
        Vector2 direction = ((Vector2) path.vectorPath[currentWayPoint] - rb.position).normalized;
        rb.velocity = new Vector2(-direction.x * m_speed, -direction.y * m_speed);
        //set animator parameters run to true when movement is not 0
        //m_animator.SetBool("Run", m_body2d.velocity.x != 0);

        ChangeFacingDirection();


        if (rb.velocity.x != 0)
        {
            animator.SetBool("Moving", true);
        }
        else if (rb.velocity.x == 0)
        {
            animator.SetBool("Moving", false);
        }

    }

    private void AutoMove()
    {
        //m_animator.SetBool("Moving", true);

        Vector2 direction = new Vector2();
        //move 
        try
        {
            direction = ((Vector2) path.vectorPath[currentWayPoint] - rb.position).normalized;
        }
        catch (ArgumentOutOfRangeException e)
        {
            //print("failed")
        }
        //if crow is close enough to player slow it down else speed it up

        //print("distance: "+ Vector2.Distance(rb.position,crowFollowPoint.transform.position));
        var velocity = new Vector2();

        if (Vector2.Distance(rb.position, crowFollowPoint.transform.position) < 15)
        {
            //print("slow!!!!");
            velocity = new Vector2(direction.x * m_speed, direction.y * m_speed);
        }
        else
        {
            //print("FAST!!!!");
            velocity = new Vector2(direction.x * m_fastSpeed, direction.y * m_fastSpeed);
        }

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        //set animator parameters run to true when movement is not 0
        //m_animator.SetBool("Run", m_body2d.velocity.x != 0);

        var distance = 0f;
        
        try
        {
            distance = Vector2.Distance(rb.position, path.vectorPath[currentWayPoint]);
        }
        catch (ArgumentOutOfRangeException e)
        {
            //print("failed")
            //supressed
        }

        if (distance < nextWaypointDistance)
        {
            currentWayPoint++;
        }

        ChangeFacingDirection();


        if (rb.velocity.x != 0)
        {
            animator.SetBool("Moving", true);
        }
        else if (rb.velocity.x == 0)
        {
            animator.SetBool("Moving", false);
        }
    }




    /**
     *************** PLAYER CONTROLLED MOVEMENT **********************************
    **/
    private void MoveCrow()
    {
        //SummonMorrigan();

        //if (summonMorrigan)
        //{
        if (!movingFast)
        {
            MoveCrow(m_speed);
        }
        else
        {
            MoveCrow(m_fastSpeed);
        }

        /*
        //Death Test
        if (Input.GetKeyDown("e"))
        {
            m_animator.SetTrigger("Death");
        }
        */
        //}
    }

    private void MoveCrow(float speed)
    {
        if (speed == m_fastSpeed)
            animator.SetBool("MovingFast", true);
        else
            animator.SetBool("MovingFast", false);

        //quickly move the player left or right
        rb.velocity = new Vector2(horizontalInput * speed, verticalInput * speed);

        //if player is moving right
        if (horizontalInput > 0.01f)
        {
            //flip transform image to right
            transform.localScale = new Vector3(0.5f, 0.5f, 1);
            m_facingDirection = 1;
        }
        //if player is moving left
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-0.5f, 0.5f, 1);
            m_facingDirection = -1;
        }

    }



    /**
     *************** MIND FOG CODE **********************************
    **/
    private void CastMindFog()
    {
        /*if (isFollowing)
        {
            castMindFog = false;
            //Does putting this here have any impact on mindfog if user transitions right back to player
            return;ddd
        }*/

        //Maybe Cast Mind Fog
        if (castMindFog)
        {
            var scoutPointGround = new Vector2(transform.position.x, transform.position.y - crowMindFogDistance);
            //scan for all enemies beneath crow
            var enemiesBeneathMe = SpriteFinder
                .ScoutForCharacterCollidersBetweenGivenPoints(
                    transform.position,
                    scoutPointGround,
                    true
                );
            if (enemiesBeneathMe == null)
            {
                return;
            }
                /*Physics2D.LinecastAll(boxCollider.ClosestPoint(PositionOfGroundBeneathCrow()),
                PositionOfGroundBeneathCrow());*/
            Debug.DrawLine(transform.position, scoutPointGround,
                Color.magenta);

            foreach (Collider2D enemyHit in enemiesBeneathMe)
            {
                //any returned enemies should have mindfog enabled
                //Debug.Log("Collider Detected : "+ enemyHit.collider.name);
                var enemy = enemyHit.GetComponent<AbstractEnemy>();
                if (enemy != null && !enemy.IsMindFogEnabled())
                {
                    Debug.Log("Casting Mind Fog on : " + enemyHit.name);
                    enemy.EnableMindFog();
                    mindFoggedEnemies.Add(enemy);
                }
            }
            castMindFog = false;
        }
        else
        {
            //Do we even need a list of mindfogged enemies? 
            // MindFog class will handle cooldown and disabling MindFog
            if (mindFoggedEnemies.Count >= 1)
            {
                //clear list of mindfogged enemies
                mindFoggedEnemies.RemoveRange(0, mindFoggedEnemies.Count - 1);
            }
            //maybe reset the start and stop positions for mindfog?
        }
        //Do we need another else so that the stacking of mindcast victims does not reset cooldown?
    }

    private void StartMindFog()
    {
        if (manaBar.TryUseMana(mindFogManaCost))
        {
            if (!isFollowing)
            {
                //Debug.Log("Starting Mind Fog");
                castMindFog = true;
                mindFogStartPosition = transform.position;
                //enable animation
            }
        }
    }

    private void StopMindFog()
    {
        /*if (!isFollowing) //If crow switches back to player, it should be auto disabled?
        {*/
        //Debug.Log("Stopping Mind Fog");
        castMindFog = false; //do we want that to be the case, or have it on a timer?
        mindFogStopPosition = transform.position;
        //disable animation
        /*}*/
    }
}
