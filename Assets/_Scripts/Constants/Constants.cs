using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Constants
{
    //Layers
    public const int AttackPointLayerNum = 20;
    public const int ProjectileLayerNum = 13;
    public const int PlayerLayerNum = 10;
    public const int MorriganLayerNum = 14;
    public const int NPCInteractableLayerNum = 16;
    public const int PlayerSensorLayerNum = 17;
    public const int EnemyLayerNum = 8;
    public const int GroundLayerNum = 6;
    public const int WallLayerNum = 7;
    public const int PlatformLayerNum = 18;
    public const int HitBoxLayerNum = 21;

    public const string TAG_HEALTHPICKUP = "HealthPickup";
    

    public static readonly Regex[] EnemyNamePatterns = {
        new Regex("Enemy.*"),
        new Regex("Light.*"),
        new Regex("Knight.*"),
        new Regex("Sorc.*"),
        new Regex("Fox.*"),
        new Regex("Wolf.*"),
        new Regex("Deer.*"),
        new Regex("Irish.*")};
    
    public static readonly Regex[] AnimalEnemyNamePatterns = {
        new Regex("Fox.*"),
        new Regex("Wolf.*"),
        new Regex("Deer.*"),
        new Regex("Elk.*")};
    
    public static readonly Regex[] HumanoidEnemyNamePatterns = {
        new Regex("Enemy.*"),
        new Regex("Light.*"),
        new Regex("Knight.*"),
        new Regex("Sorc.*")};

    public enum BowAttackDirection {
        AttackUp,
        AttackStraight,
        AttackDown
    }
    
    /*
    public enum EnemyMovementOverride {
        None,
        KnockBack,
        StepBack
    }*/
    
    public enum EnemyExpressions {
        None,
        EnemyAlerted,
        DisplayHealth
    }
    
    public enum GoalType {
        None,
        Patrol,
        Flee,
        ChaseTarget,
        MindFog,
        Die
    }
    public enum TaskName {
        Idle,
        Move,
        Vanish,
        Attack,
        RangedAttack,
        Block,
        Heal,
        MovementOverride,
        Die,
        MindFog
    }
    public enum EnemyMovementOverride {
        None,
        KnockBack,
        Hurt,
        Freeze,
        Fall
    }
    
    
    
    public enum StateName {
        Idle,
        Walk,
        Run,
        Vanish,
        Attack,
        Block,
        Heal,
        CastAOE,
        CastTrackingBall,
        KnockBack,
        Hurt,
        Freeze,
        Fall,
        Death,
        MindFog
    }

    //Animator Commands
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Moving = Animator.StringToHash("Moving");
    public static readonly int MovingFast = Animator.StringToHash("MovingFast");
    public static readonly int Grounded = Animator.StringToHash("Grounded");
    public static readonly int AttackDown = Animator.StringToHash("AttackDown");
    public static readonly int AttackUp = Animator.StringToHash("AttackUp");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int Defend = Animator.StringToHash("Defend");
    public static readonly int Heal = Animator.StringToHash("Heal");
    public static readonly int AirSpeedY = Animator.StringToHash("AirSpeedY");
    public static readonly int AttackStance = Animator.StringToHash("AttackStance");
    public static readonly int CombatReady = Animator.StringToHash("CombatReady");
    public static readonly int MindFogEnabled = Animator.StringToHash("MindFogEnabled");
    public static readonly int CastAOE = Animator.StringToHash("CastAOE");
    public static readonly int CastTrackingBall = Animator.StringToHash("CastTrackingBall");
    public static readonly int Death = Animator.StringToHash("Death");
    public static readonly int Flee = Animator.StringToHash("Flee");
    public static readonly int Vanish = Animator.StringToHash("Vanish");
    public static readonly int EnemyAlerted = Animator.StringToHash("EnemyAlerted");
    public static readonly int Hurt = Animator.StringToHash("Hurt");
    public static readonly int Fall = Animator.StringToHash("Fall");
}