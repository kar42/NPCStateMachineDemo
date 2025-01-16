using UnityEngine;

[System.Serializable]
public class Wave
{
    [System.Serializable]
    public struct EnemySpawnInfo
    {
        public GameObject enemyPrefab;   
        public int count;                
    }

    public EnemySpawnInfo[] enemiesToSpawn; 
    public float timeBetweenSpawns;
}