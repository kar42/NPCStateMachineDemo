using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class SpearRotation : MonoBehaviour,IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public Vector2 lockedPosition = Vector2.zero;
    [SerializeField] public float rotateAmountIntensity = 90;
    [SerializeField] public float rotateAmountIntensityAMT1 = 90;
    [SerializeField] public float rotateAmountIntensityAMT2 = -90;

    [SerializeField] public float deadzone = 10;
    [SerializeField] public float rotationSpeed = 10;

    [SerializeField] public Vector2 previousZerod;
    
    [SerializeField] public float dist;
        
    [SerializeField] public float prevdist;
    
    // Update is called once per frame
    void Update()
    {
        
        Vector2 spearThrowPointPostition = transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mousePosition - spearThrowPointPostition;

        var zerod = lockedPosition - mousePosition;
        
        //transform.right = direction;

        dist = Vector2.Distance(mousePosition, spearThrowPointPostition);
        
        //print("direction: " + direction);
        //print("ZEROD: " + zerod.y);
        
        transform.right = direction;
        
        /*if (zerod.y < deadzone && previousZerod != zerod)
        {
            rotateAmountIntensity = rotationSpeed;
        }
        else if (zerod.y > -deadzone && previousZerod != zerod)
        {
            rotateAmountIntensity = -rotationSpeed;
        }
        else
        {
            rotateAmountIntensity = 0;
            zerod = lockedPosition - mousePosition;
        }
        transform.Rotate(0,0,rotateAmountIntensity);

        
        //print(direction);
        //float angle = Vector2.Angle();
        //Debug.Log("direction = " + direction);
        previousZerod = zerod;
        prevdist = dist;*/
    }
    
    private float timeCount;
    private Vector2 deltaValue = Vector2.zero;

    public void OnBeginDrag(PointerEventData data)
    {
        deltaValue = Vector2.zero;
    }

    public void OnDrag(PointerEventData data)
    {
        
        print("ONDRAG");
        print("ONDRAG");
        deltaValue += data.delta;
        if (data.dragging)
        {
            timeCount += Time.deltaTime;
            if (timeCount > 0.5f)
            {
                timeCount = 0.0f;
                Debug.Log("delta: " + deltaValue);
            }
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        deltaValue = Vector2.zero;
    }
}
