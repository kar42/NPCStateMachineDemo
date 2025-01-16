using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabController : MonoBehaviour
{
    [Header("Input Manager")]
    [SerializeField] public GameObject inputManager;
    
    public Transform grabDetect;

    public Transform boxHolder;

    public CrowMovement crowMovement;

    public float rayDist;
    
    private PlayerController playerController;
    
    //private PlayerInputActions playerControls;
    private PlayerInput playerInput;
    private InputAction pickUpItem;

    private bool pickUpItemPressed;

    private void Awake()
    {
        playerInput = inputManager.GetComponent<PlayerInput>();
        //playerControls = new PlayerInputActions();
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pickUpItemPressed) // set to a key
        {
            Debug.Log("Got to pickupitem");
        }

        RaycastHit2D grabCheck = Physics2D.Raycast(grabDetect.position, Vector2.down * transform.localScale, rayDist);
        //Debug.L
        if (grabCheck.collider != null && grabCheck.collider.tag == "Liftable")
        {
            var go = grabCheck.collider.gameObject;
            if(pickUpItemPressed) // set to a key
            {
                Debug.Log("Got to pickupitem");
                go.transform.parent = boxHolder;
                go.transform.position = boxHolder.position;
                go.GetComponent<Rigidbody2D>().isKinematic = true;
            }
            else
            {
                go.transform.parent = null;
                go.GetComponent<Rigidbody2D>().isKinematic = false;
            }
        }
    }

    private void OnEnable()
    {
        pickUpItemPressed = false;
        pickUpItem = playerInput.actions["MorriganPickupItem"];
        pickUpItem.Enable();
        pickUpItem.started += _ => pickUpItemPressed = true;
        pickUpItem.performed += _ => pickUpItemPressed = true;
        pickUpItem.canceled += _ => pickUpItemPressed = false;
    }

    //boilerplate code for new input system
    private void OnDisable()
    {
        pickUpItem.Disable();
    }
}
