using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SupportScripts;
using UnityEngine;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Character : MonoBehaviour
{
    [SerializeField] private bool facingRight;
    [SerializeField] private bool allowBlocking = false;
    [SerializeField] private bool isBlockingDefenceActive = false;
    [SerializeField] private bool defenceWarning = false;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool canHeal = false;
    [SerializeField] float healAmount = 2f;
    [SerializeField] private float walkSpeed = 40f;
    [SerializeField] private float runSpeed = 60f;
    [SerializeField] private bool allowPatrol = true;
    [FormerlySerializedAs("coreDemeanor")] [SerializeField] private Personality.EnemyPersonality corePersonality = Personality.EnemyPersonality.Neutral;
    [FormerlySerializedAs("currentDemeanor")] [SerializeField] private Personality.EnemyPersonality currentPersonality = Personality.EnemyPersonality.Neutral;
    private MovementOverride movementOverride;
    private Constants.EnemyMovementOverride movementOverrideName;
    private BoxCollider2D _myCollider;
    private bool isPlayer;
    private Transform transform;
    private Rigidbody2D rb2d;
    private Health health;
    private Animator animator;
    protected LayerMask groundLayer;
    protected LayerMask platformLayer;
    private float defenceWarningCountdown = 0f;
    private float blockingCooldown = 0f;
    private bool isHealing = false;
    private Color originalCharacterColor;
    public Color characterColor;
    private Collider2D[] ignoredColliders;
    
    
    // Start is called before the first frame update
    protected void Awake()
    {
        transform = GetComponent<Transform>();
        //This works assuming the game object holding the character has one main collider only
        if (transform.name == "Player")
        {
            var playerSprite = transform.Find("SquashAndStretchAnchor/Sprite");
            _myCollider = playerSprite.GetComponent<BoxCollider2D>();
            animator = playerSprite.GetComponent<Animator>();
            isPlayer = true;
        }
        else
        {
            _myCollider = GetComponent<BoxCollider2D>();
            animator = GetComponent<Animator>();
            isPlayer = false;
        }
        rb2d = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");
        platformLayer = LayerMask.GetMask("Platform");
        health = GetComponent<Health>();
        //currentDemeanor = coreDemeanor;
        //movementOverride = new MovementOverride(animator, this);
        movementOverrideName = Constants.EnemyMovementOverride.None;
        if (!SpriteFinder.IsPlayer(transform.name))
        {
            originalCharacterColor = GetComponent<SpriteRenderer>().color;
        }

        ignoredColliders = new Collider2D[0];
    }

    /*
     ***************CORE BEHAVIOR**********************************
    */

    /**
     * Character's core demeanor.. Not meant to change, rather is a reference for returning to default behavior.
     * Access currentDemeanor for updated attitude.
     */
    public Personality.EnemyPersonality CharacterCoreDemeanor()
    {
        return corePersonality;
    }
    
    /**
     * Character's current demeanor.. Will be changed as needed to update behavior for enemies
     */
    public Personality.EnemyPersonality CharacterCurrentDemeanor()
    {
        return currentPersonality;
    }
    
    public void SetCurrentDemeanor(Personality.EnemyPersonality newPersonality)
    {
        currentPersonality = newPersonality;
    }
    
    

    /*
     ***************DIRECTIONALITY**********************************
    */
    public bool IsCharacterFacingRight() {
        return facingRight;
    }
    
    public bool SetCharacterFacingRight(bool characterIsFacingRight)
    {
        facingRight = characterIsFacingRight;
        return facingRight;
    }

    public BoxCollider2D GetCharacterCollider()
    {
        return _myCollider;
    }

    public float GetCharacterFacingXPosition()
    {
        return GetCharacterFacingPosition().x;
    }

    public Vector2 GetCharacterFacingPosition()
    {
        float xPosition = _myCollider.bounds.center.x;
        if (facingRight)
        {
            xPosition = xPosition + _myCollider.bounds.extents.x;
        }
        else
        {
            xPosition = xPosition - _myCollider.bounds.extents.x;
        }

        return new Vector2(xPosition, _myCollider.bounds.center.y);
    }

    public Vector2 GetCharacterUpperFacingPosition()
    {
        float xPosition = _myCollider.bounds.center.x;
        if (facingRight)
        {
            xPosition = xPosition + _myCollider.bounds.extents.x;
        }
        else
        {
            xPosition = xPosition - _myCollider.bounds.extents.x;
        }

        return new Vector2(xPosition, _myCollider.bounds.center.y+_myCollider.bounds.extents.y);
    }
    
    public Vector2 GetCharacterCenterFootPosition()
    {
        return new Vector2(_myCollider.bounds.center.x, _myCollider.bounds.center.y - _myCollider.bounds.extents.y);
    }

    public Vector2 GetCharacterRightFootPosition()
    {
        return new Vector2(
            _myCollider.bounds.center.x + (_myCollider.bounds.extents.x-0.1f), 
            _myCollider.bounds.center.y - _myCollider.bounds.extents.y);
    }

    public Vector2 GetCharacterLeftFootPosition()
    {
        return new Vector2(
            _myCollider.bounds.center.x - (_myCollider.bounds.extents.x+0.1f), 
            _myCollider.bounds.center.y - _myCollider.bounds.extents.y);
    }
    
    public Vector2 GetCharacterCenterHeadPosition()
    {
        return new Vector2(_myCollider.bounds.center.x, _myCollider.bounds.center.y + _myCollider.bounds.extents.y);
    }
    
    public Vector2 GetCharacterCenterPosition()
    {
        return _myCollider.bounds.center;
    }
    
    public bool IsGrounded()
    {
        var groundScanPoint = new Vector2(GetCharacterCenterFootPosition().x,GetCharacterCenterFootPosition().y-0.1f);
        var groundCenterCollider = Physics2D.OverlapCircle(groundScanPoint, 0.5f, groundLayer);
        var groundCenterColliderPlatform = Physics2D.OverlapCircle(groundScanPoint, 0.5f, platformLayer);
        Debug.DrawLine(GetCharacterCenterFootPosition(),groundScanPoint,Color.red);
        
        groundScanPoint = new Vector2(GetCharacterLeftFootPosition().x,GetCharacterLeftFootPosition().y-0.1f);
        var groundLeftCollider = Physics2D.OverlapCircle(groundScanPoint, 0.5f, groundLayer);
        var groundLeftColliderPlatform = Physics2D.OverlapCircle(groundScanPoint, 0.5f, platformLayer);
        Debug.DrawLine(GetCharacterLeftFootPosition(),groundScanPoint,Color.red);
        
        groundScanPoint = new Vector2(GetCharacterRightFootPosition().x,GetCharacterRightFootPosition().y-0.1f);
        var groundRightCollider = Physics2D.OverlapCircle(groundScanPoint, 0.5f, groundLayer);
        var groundRightColliderPlatform = Physics2D.OverlapCircle(groundScanPoint, 0.5f, platformLayer);
        Debug.DrawLine(GetCharacterRightFootPosition(),groundScanPoint,Color.red);
        
        isGrounded =  groundCenterCollider != null || groundCenterColliderPlatform != null
                       || groundLeftCollider != null || groundLeftColliderPlatform != null
                       || groundRightCollider != null || groundRightColliderPlatform != null;
        return isGrounded;
    }
    

    /*
     ***************DEFENSIVE AWARENESS**********************************
    */

    public Collider2D ShieldCollider()
    {
        var shield = transform.Find("Shield");
        if (shield != null)
        {
            return shield.GetComponent<Collider2D>();
        }
        return null;
    }
    public bool IsDefenceWarningActivated() {
        return defenceWarning; 
    }
    public void SetDefenceWarning(bool defenceWarningNewVal) {
        defenceWarning = defenceWarningNewVal; 
    }
    public void ToggleBlockingOn()
    {
        isBlockingDefenceActive = true;
        ShieldCollider().enabled = true;
        FreezeRigidbodyInPlace();
    }
    
    public void ToggleBlockingOff()
    {
        isBlockingDefenceActive = false;
        ShieldCollider().enabled = false;
        SetDefenceWarning(false);
        UnFreezeRigidbody();
    }
    public bool IsBlockingDefenceActive()
    { 
        return isBlockingDefenceActive;
    }
    public void SetBlockingDefenceActive(bool newVal)
    { 
        isBlockingDefenceActive = newVal;
    }

    public bool IsBlockingAnimationInProgress()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Block");
    }
    

    public void SetAllowBlocking(bool allowBlockingNewVal)
    {
        allowBlocking = allowBlockingNewVal;
    }
    public bool CanBlock()
    {
        return allowBlocking;
    }
    

    /*
     ***************Attack Awareness**********************************
    */
    public bool IsAttacking() {
        return isAttacking; 
    }

    public void SetAttacking(bool isAttackingNewVal)
    {
        isAttacking = isAttackingNewVal;
    }
    
    //this will need to be moved into another class for each enemy
    public void StartAttackingCountdown(Character targetedCharacter)
    {
        if (IsAttacking() && defenceWarningCountdown < 5f)
        {
            defenceWarningCountdown++;
        }
        else if (IsAttacking() && defenceWarningCountdown >= 5f)
        {
            SetAttacking(false);
            defenceWarningCountdown = 0;
        }
    }

    public void FreezeRigidbodyInPlace()
    {
        rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void UnFreezeRigidbody()
    {
        rb2d.constraints = RigidbodyConstraints2D.None;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    public Constants.EnemyMovementOverride GetMovementOverride()
    {
        return movementOverrideName;
    }
    
    public MovementOverride GetCurrentMovementOverride()
    {
        return movementOverride;
    }
    
    public bool HasMovementOverride()
    {
        return movementOverrideName != Constants.EnemyMovementOverride.None;
    }
    
    public void SetMovementOverride(Constants.EnemyMovementOverride newOverride)
    {
        movementOverrideName = newOverride;
    }
    
    public Constants.EnemyMovementOverride CheckForMovementOverride()
    {
        if (!IsGrounded())
        {
            return movementOverrideName = Constants.EnemyMovementOverride.Fall;
        } else if (IsGrounded() && movementOverrideName == Constants.EnemyMovementOverride.Fall)
        {
            movementOverrideName = Constants.EnemyMovementOverride.None;
        }
        return movementOverrideName;
    }

    protected bool ShouldHeal()
    {
        if (health.dead)
        {
            return false;
        }
        return health.currentHealth <= 3;
    }
    public void StartHealing()
    {
        if (health.IsDead())
        {
            isHealing = false;
        }
        else
        {
            isHealing = true;
            FreezeRigidbodyInPlace();
        }
    }

    public bool IsHealing()
    {
        return isHealing;
    }

    public void HealEnemy()
    {
        health.Heal(healAmount);
        isHealing = false;
    }

    public bool IsHealingAnimationInProgress()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Heal");
    }

    public void SetCharacterColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }

    public void ResetCharacterColor()
    {
        GetComponent<SpriteRenderer>().color = originalCharacterColor;
    }

    public bool IsCharacterDocileOrNeutral()
    {
        return Personality.IsDocile(currentPersonality) || Personality.IsNeutral(currentPersonality);
    }

    public bool IsCharacterNeutral()
    {
        return Personality.IsNeutral(currentPersonality);
    }

    public bool IsCharacterDocile()
    {
        return Personality.IsDocile(currentPersonality);
    }

    public bool IsCharacterAggressive()
    {
        return Personality.IsAggressive(currentPersonality);
    }

    void HealPlayer(Collision2D col)
    {
        health.Heal(1);
        Destroy(col.collider.gameObject);
        AudioManager.instance.PlaySound("HealthPickup_sfx");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        UndoIgnoreMyColliders(_myCollider, col.collider);
        if (isPlayer && SpriteFinder.IsSpear(col.collider.name) && col.collider.GetComponentInChildren<SpearTip>().hasStruck)
        {
            IgnoreMyColliders(_myCollider,col.collider);
        }
        // HEALTH POTION LOGIC
        else if (col.collider.name == "healthpotion")
        {
            HealPlayer(col);
        }
        else if (SpriteFinder.ShouldIgnoreCharacterCompletely(col.collider,_myCollider))
        {
           IgnoreMyColliders(_myCollider,col.collider);
        } 
    }

    public void IgnoreMyColliders(Collider2D myCollider, Collider2D colliderToIgnore)
    {
        Physics2D.IgnoreCollision(myCollider, colliderToIgnore, true);
        if (!SpriteFinder.IsSpear(colliderToIgnore.name))
        {
            AddToIgnoredColliderList(colliderToIgnore);
        }
        var childColliders = myCollider.GetComponentsInChildren<BoxCollider2D>();
        foreach (var collider in childColliders)
        {
            Physics2D.IgnoreCollision(collider, colliderToIgnore, true);
        }
        var childCircleColliders = myCollider.GetComponentsInChildren<CircleCollider2D>();
        foreach (var collider in childCircleColliders)
        {
            Physics2D.IgnoreCollision(collider, colliderToIgnore, true);
        }
    }

    private void UndoIgnoreMyColliders(Collider2D myCollider, Collider2D colliderToStopIgnoring)
    {
        if (ignoredColliders.Length > 0 && ignoredColliders.Contains(colliderToStopIgnoring))
        {
            Physics2D.IgnoreCollision(myCollider, colliderToStopIgnoring, false);
            RemoveFromIgnoredColliderList(colliderToStopIgnoring);
            var childColliders = myCollider.GetComponentsInChildren<BoxCollider2D>();
            foreach (var collider in childColliders)
            {
                Physics2D.IgnoreCollision(collider, colliderToStopIgnoring, false);
            }
            var childCircleColliders = myCollider.GetComponentsInChildren<CircleCollider2D>();
            foreach (var collider in childCircleColliders)
            {
                Physics2D.IgnoreCollision(collider, colliderToStopIgnoring, false);
            }
        }
    }

    private void AddToIgnoredColliderList(Collider2D colliderToIgnore)
    {
        List<Collider2D> updatedColliderList = new List<Collider2D>();
        updatedColliderList.AddRange(ignoredColliders);
        updatedColliderList.Add(colliderToIgnore);
        ignoredColliders = updatedColliderList.ToArray();
    }

    private void RemoveFromIgnoredColliderList(Collider2D colliderToRemoveFromIgnore)
    {
        List<Collider2D> updatedColliderList = new List<Collider2D>();
        updatedColliderList.AddRange(ignoredColliders);
        updatedColliderList.Remove(colliderToRemoveFromIgnore);
        ignoredColliders = updatedColliderList.ToArray();
    }

    public void ResetAllIgnoredColliders()
    {
        foreach (var ignoredCollider in ignoredColliders)
        {
            UndoIgnoreMyColliders(_myCollider, ignoredCollider);
        }
    }

    public float GetWalkSpeed()
    {
        return walkSpeed;
    }

    public float GetRunSpeed()
    {
        return runSpeed;
    }

    public bool GetAllowPatrol()
    {
        return allowPatrol;
    }

    public bool GetAllowBlocking()
    {
        return allowBlocking;
    }

    public void SetCanHeal(bool canHealNewVal)
    {
        canHeal = canHealNewVal;
    }

    public bool GetCanHeal()
    {
        return canHeal;
    }
}
