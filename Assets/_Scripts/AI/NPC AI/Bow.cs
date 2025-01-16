using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [SerializeField] Archer archer;
    [SerializeField] public bool attackUp;
    [SerializeField] public bool attackDown;

    public Transform firePoint;
    public GameObject arrowPrefab;

    public void Shoot()
    {
        /*//Debug.Log("Shot an arrow");
        if (attackUp)
        {
            AudioManager.instance.PlaySound("ArrowShoot");
            Quaternion arrowRotate;// = Quaternion.Euler(0, 0, 40);
            if (archer.IsFacingRight())
            {
                arrowRotate = Quaternion.Euler(0, 0, 40);
            }
            else
            {
                arrowRotate = Quaternion.Euler(0, 0, 140);
            }
            Instantiate(arrowPrefab, firePoint.position, arrowRotate);
            return;
        }
        else if(attackDown)
        {
            AudioManager.instance.PlaySound("ArrowShoot");
            Quaternion arrowRotate;// = Quaternion.Euler(0, 0, 40);
            if (archer.IsFacingRight())
            {
                arrowRotate = Quaternion.Euler(0, 0, 320);
            }
            else
            {
                arrowRotate = Quaternion.Euler(0, 0, 220);
            }
            Instantiate(arrowPrefab, firePoint.position, arrowRotate);
            return;
        } else
        {

            AudioManager.instance.PlaySound("ArrowShoot");
            Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        }*/
    }
}
