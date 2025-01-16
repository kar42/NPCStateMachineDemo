using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] PlayerController playerController;
    public bool PickupItem(GameObject obj)
    {
        switch (obj.tag)
        {
            case Constants.TAG_HEALTHPICKUP:
                playerHealth.Heal(1);
                
                return true;
            default:
                Debug.LogWarning("Pickup not recognised, Add a tag to the pickup object and create a switch case");
                return false;
        }
    }
}
