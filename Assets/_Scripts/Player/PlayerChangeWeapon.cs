using UnityEngine;

public class PlayerChangeWeapon : MonoBehaviour
{
    
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    public PlayerController playerController;
    private int CurrentWeapon = 0;
    public int SPEAR = 0;
    public int SPEARTHROWN = 1;
    public int SWORD = 2;
    public int GRAPPLE = 3;
    public bool isSpearEquipped = false;
    private bool isSwordEquipped = false;
    private bool isGrappleEquipped = false;

    public void ChangeToSpearLayer()
    {
        playerController.animator.SetLayerWeight(SPEARTHROWN, 0);
        playerController.animator.SetLayerWeight(SPEAR, 1);
    }

    public void ChangeToSpearThrownLayer()
    {
        playerController.animator.SetLayerWeight(SPEAR,0);
        playerController.animator.SetLayerWeight(SPEARTHROWN, 1);
    }
    
    
    public void ChangeWeapon()
    {
        //spear was thrown
        if(CurrentWeapon == SPEAR && playerVariables.isSpearThrown)
        {
            //playerController.animator.SetLayerWeight(SPEAR,0);
            //playerController.animator.SetLayerWeight(SPEARTHROWN, 1);
        } 
        else
        {
            ChangeToSpearLayer();
        }
    }
}