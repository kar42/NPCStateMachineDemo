using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    
    private PlayerInput playerInput;

    [Header("Input Manager")]
    [SerializeField] public GameObject inputManager;
    
    [FormerlySerializedAs("PlayerVariables")]
    [Header("Player Variables")]
    [SerializeField] public PlayerVariables playerVariables;
    
    public GameOver gameOverScreen;
    public PauseMenu pauseMenu;
    public PlayerHealth playerHealth;

    [SerializeField] public PlayerController playerController;
    [SerializeField] public CrowMovement crowMovement;
    
        
    public int waittime = 2;
    //private PlayerInputActions gameControls;
    [SerializeField] public int  score = 0;
    
    private InputAction pauseGame;
    private InputAction spearThrowView;

    public bool gamePaused;

    Text ScoreHUD = null;

    private void Awake()
    {
        playerInput = inputManager.GetComponent<PlayerInput>();
        // reset to false;
        playerVariables.isSpearControlButtonHeld = false;
        gamePaused = false;
        
        // Get the score HUD
        ScoreHUD = GameObject.Find("Points").GetComponent<Text>();
        //cast score to string and display it
        ScoreHUD.text = "" + score;
    }

    private void Update()
    {
        if (playerVariables.isSpearThrown && playerVariables.isSpearControlButtonHeld)
        {
            //print("manual control activated");
            SpearThrowManualControl();
        }
        if(playerHealth.dead)
        {
            GameOver();
        }
        if (pauseMenu.restartButtonPressed)
        {
            RestartGame();
        }
        if (pauseMenu.exittButtonPressed)
        {
            ExitGame();
        }
        
        //cast score to string and display it
        ScoreHUD.text = "" + score;
        
    }

    private void SpearThrowManualControl()
    {
        var spear = GameObject.Find("Spear(Clone)");
        
        playerController.isActiveCharacter = false;
        crowMovement.isActiveCharacter = false;
        
    }


    public void GameOver()
    {
        StartCoroutine(WaitBeforeShowingGameOverMenu());
        //gameOverScreen.Setup(test);
    }
    
    private IEnumerator WaitBeforeShowingGameOverMenu()
    {

        yield return new WaitForSeconds(waittime);

        gameOverScreen.Setup(score);
        
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        //SceneManager.LoadScene("GameScene");
        //playerMovement.gamePaused = false;
        pauseMenu.restartButtonPressed = false;
        //playerMovement.gamePaused = false;

    }

    public void OpenSettingsMenu()
    {
        Time.timeScale = 0f;
        pauseMenu.gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    private void PauseGame()
    {
        if (!gamePaused)
        {
            //Debug.Log("We got here");
            pauseMenu.Pause();
            gamePaused = true;
        }
    }

    private void OnEnable()
    {
        spearThrowView = playerInput.actions["SpearThrowView"];
        spearThrowView.Enable();
        spearThrowView.started += _ => setSpearControlButtonHeld(true);
        spearThrowView.canceled +=  _ => setSpearControlButtonHeld(false);
        
        pauseGame = playerInput.actions["Pause Menu"];
        pauseGame.Enable();
        pauseGame.performed += _ => PauseGame();
    }

    private void setSpearControlButtonHeld(bool isButtonHeld)
    {
        playerVariables.isSpearControlButtonHeld = true;
    }
    

    
    public void GoBackToPauseMenu()
    {
        pauseMenu.gameObject.SetActive(true);
        pauseMenu.settingsButtonPressed = false;
    }
}
