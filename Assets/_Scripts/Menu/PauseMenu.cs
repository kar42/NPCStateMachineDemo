using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameController gameController;
    [SerializeField] PlayerController playerController;

    private InputAction pauseGame;

    public bool restartButtonPressed;
    public bool exittButtonPressed;
    public bool settingsButtonPressed;
    public bool saveButtonPressed;

    public GameObject jokeText;

    private void Start()
    {
        restartButtonPressed = false;
        exittButtonPressed = false;
        saveButtonPressed = false;
    }

    public void Pause()
    {
        Debug.Log("Game Paused");
        //GenerateRandomJoke();
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void SaveGame()
    {
        saveButtonPressed = true;
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ResumeGame()
    {
        //Debug.Log("Game Resumed");
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartFromLastSavePoint()
    {
        //Debug.Log("Restart Game Pressed");
        restartButtonPressed = true;
        gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game Pressed");
        exittButtonPressed = true;
    }

    public void SettingsMenu()
    {
        Debug.Log("Settings Pressed");
        settingsButtonPressed = true;
    }

    public void CloseOverlay()
    {
        gameObject.SetActive(false);
    }

    public void OpenOverlay()
    {
       // gameObject.SetActive(true);
    }
}
