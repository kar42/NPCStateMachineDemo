using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairCursor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Mouse.current.leftButton.isPressed
        Vector2 mouseCursorPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //GetComponent<Rigidbody2D>().MovePosition(mouseCursorPos);
        gameObject.transform.position = mouseCursorPos;
    }
}
