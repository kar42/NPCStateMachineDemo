using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLedgeGrab : MonoBehaviour
{
    [FormerlySerializedAs("PlayerVariables")]
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    private bool greenBox, redBox;
    [SerializeField] public PlayerController playerController;
    [SerializeField] public LayerMask grabPointLayer;
    [SerializeField] public float grabPointX;
    [SerializeField] public float grabPointY;
    public float redOffsetX, redOffsetY, redXSize, redYSize, greenOffsetX, greenOffsetY, greenXSize, greenYSize;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Animator animator;
    private float startingGrav;
    private PlayerWallDetector wallSensorR2;
    private PlayerWallDetector wallSensorL2;
    private PlayerWallDetector ledgeSensorL1;
    private PlayerWallDetector ledgeSensorR1;


    private GameObject closestGrabPoint;
    

    public float playerXOffest = 10f;
    public float playerYOffset = 12.4f;
    
    public float ledgeGrabOffsetX = 10f;
    public float ledgeGrabOffsetY = 12.4f;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startingGrav = rb.gravityScale;
        animator = transform.Find("SquashAndStretchAnchor/Sprite").GetComponent<Animator>();
        playerCollider = transform.Find("SquashAndStretchAnchor/Sprite").GetComponent<Collider2D>();
        wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<PlayerWallDetector>();
        wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<PlayerWallDetector>();
        ledgeSensorL1 = transform.Find("LedgeSensor_L1").GetComponent<PlayerWallDetector>();
        ledgeSensorR1 = transform.Find("LedgeSensor_R1").GetComponent<PlayerWallDetector>();
    }

    // Update is called once per frame
    void Update()
    {
        
        closestGrabPoint = ClosestGrabPoint(playerCollider.transform.position, range: 30);

        if (closestGrabPoint != null)
        {
            if (playerVariables.isGrabbing)
            {
                animator.SetBool("LedgeGrab", true);
            }
        
            //greenBox = Physics2D.OverlapBox(new Vector2(transform.position.x + (greenOffsetX * transform.localScale.x),
            //   transform.position.y + greenOffsetY), new Vector2(greenXSize, greenYSize), 0f, groundMask);
        
            //redBox = Physics2D.OverlapBox(new Vector2(transform.position.x + (redOffsetX * transform.localScale.x),
            //transform.position.y + redOffsetY), new Vector2(redXSize, redYSize), 0f, groundMask);

        
        
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("LedgeGrab") && ((ledgeSensorL1.State() && !wallSensorL2.State()) || (ledgeSensorR1.State() && !wallSensorR2.State())) && !playerVariables.isGrabbing && playerVariables.isJumping)
            {
                playerVariables.isGrabbing = true;
            }
            else
            {
                playerVariables.isGrabbing = false;
            
            }
        
            /*if (PlayerVariables.isJumping)
            {
                PlayerVariables.isGrabbing = false;
            }*/

            if (playerVariables.isGrabbing)
            {
                rb.velocity = new Vector2(0, 0);
                rb.gravityScale = 0f;
            }
        }
        
    }
    
    public void MovetoGrabPoint()
    {
        playerVariables.isJumping = false;
        //transform.position = new Vector2(transform.position.x + (playerXOffest * transform.localScale.x), transform.position.y + playerYOffset );
        transform.position = new Vector2(
            closestGrabPoint.transform.position.x + ledgeGrabOffsetX,
            closestGrabPoint.transform.position.y + ledgeGrabOffsetY
            );
        
        
    }

    private GameObject ClosestGrabPoint(Vector2 origin, float range)
    {
        float closest = 50; //add your max range herea
        GameObject closestObject = null;
        
        var list = new List<GameObject>();
        
        var found = Physics2D.OverlapCircleAll(origin, range,layerMask: grabPointLayer);

        foreach(var currentGameObject in found)  //list of gameObjects to search through
        {
            if (currentGameObject.tag == "LedgeGrabPoint")
            {
                float dist = Vector2.Distance(currentGameObject.transform.position, transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    closestObject = currentGameObject.gameObject;
                }
            }
        }
        return closestObject;
    }

    /*private GameObject ClosestGrabPoint(Vector2 origin, float range)
    {
        var list = new List<GameObject>();
        Collider[] found = Physics.OverlapSphere(origin, range);
 
        foreach(var collider in found)
            list.Add(collider.gameObject);
   
        return ClosestGrabPoint(origin, list);
    }
    
    private GameObject ClosestGrabPoint(Vector3 origin, IEnumerable<GameObject> gameObjects)
    {
        GameObject closest = null;
        float closestSqrDist = 0f;
 
        foreach(var gameObject in gameObjects) {
            float sqrDist = (gameObject.transform.position - origin).sqrMagnitude; //sqrMagnitude because it's faster to calculate than magnitude
 
            if (!closest || sqrDist < closestSqrDist && gameObject.transform.tag == "LedgeGrabPoint") {
                closest = gameObject;
                closestSqrDist = sqrDist;
            }
        }
 
        return closest;
    }*/

    public void ChangePos()
    {
        transform.position = new Vector2(transform.position.x + (playerXOffest * playerController.facingDirection), transform.position.y + playerYOffset );
        rb.gravityScale = startingGrav;
        playerVariables.isGrabbing = false;
        animator.SetBool("LedgeGrab", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (redOffsetX * transform.localScale.x),
            transform.position.y + redOffsetY), new Vector2(redXSize, redYSize));
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (greenOffsetX * transform.localScale.x),
            transform.position.y + greenOffsetY), new Vector2(greenXSize, greenYSize));
        
    }
}
