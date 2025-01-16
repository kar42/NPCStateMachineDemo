using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    
    Text GameOverScore = null;
    
    public void Setup(int score)
    {
        //GameObject.Find("HUD").SetActive(false);
        gameObject.SetActive(true);
        GameOverScore = GameObject.Find("EnemiesKilledCount").GetComponent<Text>();
        GameOverScore.text = "Enemies Killed: " + score;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("AIForGamesProject");
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
