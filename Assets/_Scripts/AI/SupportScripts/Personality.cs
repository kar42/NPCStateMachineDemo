using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Personality
{
    public enum EnemyPersonality {
        Docile,
        Neutral,
        Aggressive
    }

    public static bool IsDocile(EnemyPersonality personality)
    {
        return personality == EnemyPersonality.Docile;
    }

    public static bool IsNeutral(EnemyPersonality personality)
    {
        return personality == EnemyPersonality.Neutral;
    }

    public static bool IsAggressive(EnemyPersonality personality)
    {
        return personality == EnemyPersonality.Aggressive;
    }
}
