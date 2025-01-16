using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    public Wave[] waves;   
    
    // Where enemies will spawn.
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public Transform spawnPoint3;

    public Text waveNumberText;

    public int enemiesKilled = 0;

    private int currentWave = 1;
    
    public delegate void WaveEvent(int waveNumber);
    public static event WaveEvent OnNewWave;
    
    private int score = 0;
    
    
    
    GameController gameController = null;

    private void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        //StartCoroutine(ShowWaveNumberText());
        if (waves.Length > 0)
        {
            StartCoroutine(SpawnWave());
        }
    }

    IEnumerator SpawnWave()
    {
        while (currentWave < waves.Length)
        {
            StartCoroutine(ShowWaveNumberText());
            
            Wave currentWaveData = waves[currentWave-1];
            

            foreach (var enemyInfo in currentWaveData.enemiesToSpawn)
            {
                for (int i = 0; i < enemyInfo.count; i++)
                {
                    SpawnEnemy(enemyInfo.enemyPrefab);
                    yield return new WaitForSeconds(currentWaveData.timeBetweenSpawns);
                }
            }
            
            // Wait for all enemies to be killed

            var animalsSpawned = 0;
            
            foreach (var ai in currentWaveData.enemiesToSpawn)
            {
                
                if(ai.enemyPrefab.gameObject.GetComponent<AbstractEnemy>().IsAnimalEnemy(ai.enemyPrefab.gameObject.name))
                {
                    print("Animal Enemy Found");
                    animalsSpawned++;
                }
            }
            
            print(currentWaveData.enemiesToSpawn.Sum(e => e.count));
            
            while (enemiesKilled < currentWaveData.enemiesToSpawn.Sum(e => e.count-animalsSpawned))
            {
                yield return null;
            }
            
            print("All Enemies Killed in wave.");
            enemiesKilled = 0; // Reset for next wave
            currentWave++;
            StartCoroutine(ShowWaveCompleteText());
            OnNewWave?.Invoke(currentWave);
            yield return new WaitForSeconds(3f); // Adjust as necessary
        }
    }

    
    IEnumerator ShowWaveNumberText()
    {
        waveNumberText.gameObject.SetActive(true);
        waveNumberText.text = "Wave " + currentWave;
        yield return new WaitForSeconds(5f); // Time between waves; can be adjusted as necessary
        waveNumberText.gameObject.SetActive(false);
    }

    
    IEnumerator ShowWaveCompleteText()
    {
        waveNumberText.gameObject.SetActive(true);
        waveNumberText.text = "Wave Complete!";
        yield return new WaitForSeconds(5f); // Time between waves; can be adjusted as necessary
        waveNumberText.gameObject.SetActive(false);
    }

    void Update()
    {
        //waveNumberText.text = "Wave " + currentWave;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        Transform selectedSpawnPoint;

        int randomPoint = Random.Range(1, 4);
        switch (randomPoint)
        {
            case 1:
                selectedSpawnPoint = spawnPoint1;
                break;
            case 2:
                selectedSpawnPoint = spawnPoint2;
                break;
            case 3:
                selectedSpawnPoint = spawnPoint3;
                break;
            default:
                selectedSpawnPoint = spawnPoint1;
                break;
        }

        Instantiate(enemyPrefab, selectedSpawnPoint.position, Quaternion.identity);
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    private void OnEnable()
    {
        AbstractEnemy.OnEnemyDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        AbstractEnemy.OnEnemyDeath -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath(AbstractEnemy deadEnemy)
    {
        enemiesKilled++;
        score++;
        gameController.score = score;
    }

}

