using System.Collections;
using System.Collections.Generic;
using SupportScripts;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimEvents : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    // References to effect prefabs. These are set in the inspector
    [Header("Effects")]
    public GameObject runStopDust;
    public GameObject jumpDust;
    public GameObject landingDust;
    public GameObject dodgeDust;
    public GameObject wallSlideDust;
    public GameObject wallJumpDust;
    public GameObject airSlamDust;
    public GameObject parryEffect;
    public PlayerLedgeGrab ledgeGrab;
    
    [Header("Player")]
    public PlayerController player;
    public PlayerCombat playerCombat;
    public AttackSystem attackSystem;
    private Character playerCharacter;

    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = AudioManager.instance;
        playerCharacter = player.GetComponent<Character>();
    }


    // Animation Events
    // These functions are called inside the animation files
    void AE_resetDodge()
    {
        player.ResetDodging();
        float dustXOffset = 0.6f;
        float dustYOffset = 0.078125f;
        //player.SpawnDustEffect(runStopDust, dustXOffset, dustYOffset);
        //Debug.Log("AE_resetDodge");
    }

    void AE_blockStart()
    {
        playerVariables.canMove = false;
    }
    

    void AE_resetWallSlide()
    {
        audioManager.StopSound("WallSlide_sfx");
    }

    void AE_setPositionToClimbPosition()
    {
        player.SetPositionToClimbPosition();
    }

    void AE_runStop()
    {
        audioManager.StopSound("Footstep1_sfx");
        float dustXOffset = 0.6f;
        float dustYOffset = 0.078125f;
        //player.SpawnDustEffect(runStopDust, dustXOffset, dustYOffset);
        //Debug.Log("AE_runStop");
    }

    void AE_footstep1()
    {
        audioManager.PlaySound("Footstep1_sfx");
    }
    void AE_footstep2()
    {
        //audioManager.PlaySound("Footstep2");
    }

    public void AE_Jump()
    {
        audioManager.StopSound("Footstep1_sfx");
        audioManager.PlaySound("Jump_sfx");
        AE_resetWallSlide();

        if (!player.IsWallSliding())
        {
            float dustYOffset = 0.078125f;
            //player.SpawnDustEffect(jumpDust, 0.0f, dustYOffset);
        }
        else
        {
            //player.SpawnDustEffect(wallJumpDust);
        }
    }

    void AE_Landing()
    {
        audioManager.StopSound("Footstep1_sfx");
        audioManager.PlaySound("Landing_sfx");
        float dustYOffset = 0.078125f;
        //player.SpawnDustEffect(landingDust, 0.0f, dustYOffset);
    }

    void AE_Throw()
    {
        audioManager.PlaySound("Jump_sfx");
    }

    void AE_Parry()
    {
        audioManager.PlaySound("Parry_sfx");
        float xOffset = 0.1875f;
        float yOffset = 0.25f;
        //player.SpawnDustEffect(parryEffect, xOffset, yOffset);
        player.DisableMovement(0.5f);
    }

    void AE_ParryStance()
    {
        audioManager.PlaySound("DrawSword_sfx");
    }

    void AE_AttackAirSlam()
    {
        audioManager.PlaySound("DrawSword_sfx");
    }

    void AE_AttackAirLanding()
    {
        audioManager.PlaySound("AirSlamLanding");
        float dustYOffset = 0.078125f;
        player.SpawnDustEffect(airSlamDust, 0.0f, dustYOffset);
        player.DisableMovement(0.5f);
    }

    void AE_Hurt()
    {
        //audioManager.PlaySound("PlayerHurt_sfx");
    }

    void AE_Death()
    {
        audioManager.PlaySound("PlayerDeath_sfx");
    }

    void AE_SwordAttack()
    {
        audioManager.PlaySound("SwordAttack_sfx");
    }

    void AE_SheathSword()
    {
        audioManager.PlaySound("SheathSword_sfx");
    }

    void AE_Dodge()
    {
        audioManager.PlaySound("Dodge_sfx");
        float dustYOffset = 0.078125f;
        player.SpawnDustEffect(dodgeDust, 0.0f, dustYOffset);
    }

    void AE_WallSlide()
    {
        //m_audioManager.GetComponent<AudioSource>().loop = true;
        if (!audioManager.IsPlaying("WallSlide_sfx"))
            audioManager.PlaySound("WallSlide_sfx");
        
        float dustXOffset = 0.25f;
        float dustYOffset = 0.25f;
        player.SpawnDustEffect(wallSlideDust, dustXOffset, dustYOffset);
    }

    void AE_LedgeGrab()
    {
        audioManager.PlaySound("LedgeGrab_sfx");
    }

    void AE_LedgeClimb()
    {
        //audioManager.PlaySound("RunStop_sfx");
    }

    void AE_Fall()
    {
        audioManager.StopSound("Footstep1_sfx");
        //audioManager.PlaySound("RunStop_sfx");
    }

    void AE_AirAttackSpear()
    {
        audioManager.PlaySound("SpearMelee1_sfx");
    }

    void AE_Melee1Spear()
    {
        audioManager.PlaySound("SpearMelee1_sfx");
        player.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
        player.move.Disable();
        playerVariables.canMove = false;
        
    }

    void AE_Melee2Spear()
    {
        audioManager.PlaySound("SpearMelee2_sfx");
        player.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
        player.move.Disable();
        playerVariables.canMove = false;
        
    }
    
    void AE_Melee3Spear()
    {
        audioManager.PlaySound("SpearMelee2_sfx");
        player.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
        player.move.Disable();
        playerVariables.canMove = false;
    }

    void AE_DamageEnemy()
    {
        playerCombat.Attack();
    }
    
    void AE_ChargeSpearRelease()
    {
        //playerCombat.Attack();
        //playerCombat.ChargeAttack();
        //player.PlayerSpearThrow.ThrowRetrieveSpear();
        audioManager.PlaySound("ChargeSpearRelease_sfx");
        
    }

    void AE_ChargeSpearReleaseEnd()
    {
        player.animator.SetLayerWeight(player.ChangeWeapon.SPEAR,0);
        player.animator.SetLayerWeight(player.ChangeWeapon.SPEARTHROWN,1);
        //player.AttackSystem.DisableAttack();
        //playerController.animator.SetLayerWeight(SPEARTHROWN, 1);
    }
    
    void AE_StartAttack()
    {
        attackSystem.isAnimationStillAttacking = true;
        //attackSystem.hasClicked = false;
    }

    void AE_EndAttack()
    {
        player.move.Enable();
        attackSystem.isAnimationStillAttacking = false;
        playerVariables.canMove = true;

    }
    void AE_EnterLedgeGrab()
    {
        //print("Enter ledge grab");
        player.move.Disable();
        player.jump.Disable();
        ledgeGrab.MovetoGrabPoint();
    }
    
    void AE_ExitLedgeGrab()
    {
        //print("Exit ledge grab");
        player.move.Enable();
        player.jump.Enable();
        ledgeGrab.ChangePos();
    }

    void AE_SpearBlockImpact()
    {
        ScreenShake.Instance.ShakeCamera(15f, .2f);
    }

    void AE_BlockAttackEnd()
    {
        playerVariables.canMove = true;
    }
    
    void AE_KnockbackSpeedUpdate(float knockbackSpeed)
    {
        player.SetAnimationSpeed(knockbackSpeed);
    }

    void AE_KnockbackExit()
    {
        playerCharacter.SetMovementOverride(Constants.EnemyMovementOverride.None);
    }
    
    
    

    //used to prevent stuttering in wallslide animation
    /*void AE_setFalseCanPerformSlideStartAnimation()
    {
        Debug.Log("got asdasd");
        player.setCanPerformSlideStartAnimation(false);
    }

    void AE_setTrueCanPerformSlideStartAnimation()
    {
        player.setCanPerformSlideStartAnimation(true);
    }*/
}
