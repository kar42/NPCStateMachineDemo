using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerPlayerMovement : MonoBehaviour
{
    //Movement and Jump Variables
    private Rigidbody2D rb;

    private bool CanMove = true;
    private bool CanJump;
    private bool OnGround;

    [SerializeField] private float MovementSpeed;
    [SerializeField] private float JumpSpeed;
    [SerializeField] private float WallSlideSpeed;

    [SerializeField] private float RayLength;
    [SerializeField] private float RayPositionOffset;

    Vector3 RayPositionCenter;
    Vector3 RayPositionLeft;
    Vector3 RayPositionRight;

    RaycastHit2D[] GroundHitsCenter;
    RaycastHit2D[] GroundHitsLeft;
    RaycastHit2D[] GroundHitsRight;

    RaycastHit2D[][] AllRaycastHits = new RaycastHit2D[3][];

    //Wall Jump Variables
    private bool OnWallRight;
    private bool OnWallLeft;

    Vector3 WallRayPositionLeft;
    Vector3 WallRayPositionRight;

    RaycastHit2D[] WallHitsLeft;
    RaycastHit2D[] WallHitsRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Movement();
        Jump();
    }

    private void Movement()
    {
        if (CanMove)
        {
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                rb.velocity = new Vector2(MovementSpeed * Time.fixedDeltaTime, rb.velocity.y);
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                rb.velocity = new Vector2(-MovementSpeed * Time.fixedDeltaTime, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
    }

    private void RaySetup()
    {
        //Ground Check/Standard Jump Ray Setup
        RayPositionCenter = transform.position + new Vector3(0, RayLength * .5f, 0);
        RayPositionLeft = transform.position + new Vector3(-RayPositionOffset, RayLength * .5f, 0);
        RayPositionRight = transform.position + new Vector3(RayPositionOffset, RayLength * .5f, 0);

        GroundHitsCenter = Physics2D.RaycastAll(RayPositionCenter, -Vector2.up, RayLength);
        GroundHitsLeft = Physics2D.RaycastAll(RayPositionLeft, -Vector2.up, RayLength);
        GroundHitsRight = Physics2D.RaycastAll(RayPositionRight, -Vector2.up, RayLength);

        AllRaycastHits[0] = GroundHitsCenter;
        AllRaycastHits[1] = GroundHitsLeft;
        AllRaycastHits[2] = GroundHitsRight;

        OnGround = RayListCheck(AllRaycastHits);
        CanJump = OnGround;

        //Wall Jump/Slide Ray Setup
        WallRayPositionLeft = transform.position + new Vector3(-RayPositionOffset, .5f, 0);
        WallRayPositionRight = transform.position + new Vector3(RayPositionOffset, .5f, 0);

        WallHitsLeft = Physics2D.RaycastAll(WallRayPositionLeft, Vector2.left, RayLength);
        WallHitsRight = Physics2D.RaycastAll(WallRayPositionRight, -Vector2.left, RayLength);

        OnWallLeft = RayCheck(WallHitsLeft);
        OnWallRight = RayCheck(WallHitsRight);

        DrawRays();
    }

    private bool RayCheck(RaycastHit2D[] RayHits)
    {
        foreach (RaycastHit2D hit in RayHits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.tag != "PlayerCollider")
                {
                    return true;
                }
            }

        }
        return false;
    }

    private void Jump()
    {
        RaySetup();

        //Ground Jump
        if (Input.GetKey(KeyCode.Space) && CanJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity = new Vector2(rb.velocity.x, JumpSpeed);
        }

        if (!OnGround)
        {
            //Wall Slide
            if (CanMove)
            {
                if (Input.GetAxisRaw("Horizontal") < 0 && OnWallLeft)
                {
                    rb.velocity = new Vector2(rb.velocity.x, -WallSlideSpeed);
                }
                else if (Input.GetAxisRaw("Horizontal") > 0 && OnWallRight)
                {
                    rb.velocity = new Vector2(rb.velocity.x, -WallSlideSpeed);
                }
            }

            //Wall Jump
            if (Input.GetKeyDown(KeyCode.Space) && OnWallLeft)
            {
                rb.velocity = new Vector2(JumpSpeed * 0.5f, JumpSpeed);
                StartCoroutine(WallJumpCoolDown());
            }

            if (Input.GetKeyDown(KeyCode.Space) && OnWallRight)
            {
                rb.velocity = new Vector2(-JumpSpeed * 0.5f, JumpSpeed);
                StartCoroutine(WallJumpCoolDown());
            }
        }
    }

    IEnumerator WallJumpCoolDown()
    {
        CanMove = false;
        yield return new WaitForSeconds(0.25f);
        CanMove = true;
    }

    private bool RayListCheck(RaycastHit2D[][] RayHits)
    {
        foreach (RaycastHit2D[] HitList in RayHits)
        {
            foreach (RaycastHit2D hit in HitList)
            {
                if (hit.collider != null)
                {
                    if (hit.collider.tag != "PlayerCollider")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void DrawRays()
    {
        //Ground Rays
        Debug.DrawRay(RayPositionCenter, -Vector2.up * RayLength, Color.red);
        Debug.DrawRay(RayPositionLeft, -Vector2.up * RayLength, Color.red);
        Debug.DrawRay(RayPositionRight, -Vector2.up * RayLength, Color.red);

        //Wall Rays
        Debug.DrawRay(WallRayPositionLeft, Vector2.left * RayLength, Color.blue);
        Debug.DrawRay(WallRayPositionRight, -Vector2.left * RayLength, Color.blue);
    }

}
