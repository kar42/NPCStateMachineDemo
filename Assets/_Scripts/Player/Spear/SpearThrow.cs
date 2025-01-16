using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SpearThrow : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] CinemachineVirtualCamera spearCamera;
    public Transform firePoint;
    public GameObject spearPrefab;
    

    public void Shoot()
    {
        //Debug.Log("Shot an arrow");
        player.PlayerSpearThrow.IsSpearThrown(true);
        var firePosition = new Vector2(firePoint.position.x, firePoint.position.y);
        var spear = Instantiate(spearPrefab, firePosition, firePoint.rotation);
        //GameObject.Find("Spear(Clone)").GetComponent<ParticleSystem>().Stop();
        //spearCamera.Follow = spear.transform;
    }
    
}
