using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public class rotating_around_object_code : MonoBehaviour
    {
        public Vector2 turn;
 
        public float sensitivity = .5f;
    
        void Start()

        {

            Cursor.lockState = CursorLockMode.Locked;

        }
    
        // Update is called once per frame
        void Update()
        {
            Vector2 spearThrowPointPostition = transform.position;
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 direction = mousePosition - spearThrowPointPostition;
            //transform.right = direction;

            turn.x += mousePosition.x * sensitivity;

            turn.y += mousePosition.y * sensitivity;
        
            transform.localRotation = Quaternion.Euler(-turn.x, turn.y, 0);
        
            //float angle = Vector2.Angle();
            //Debug.Log("direction = " + direction);
        }
    
    

        public Vector3 deltaMove;

        public float speed = 1;

    }
}