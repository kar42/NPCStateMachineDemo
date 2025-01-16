using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target; // The target object that camera will follow
    public Vector2 offset; // The constant offset between the camera and the target
    public float smoothSpeed = 0.125f; // Smoothing factor for the camera's movement

    void LateUpdate()
    {
        if (target == null)
            return;

        // Determine the desired position of the camera
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);

        // Smoothly interpolate between the camera's current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}