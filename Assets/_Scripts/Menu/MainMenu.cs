using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("AIForGamesProject");
    }
    public void NewGame()
    {
        //Debug.Log("New Game Loaded: Not yet Implemented");
        SceneManager.LoadScene("IntroCutScene");
    }
    
    public void CoOp()
    {
        SceneManager.LoadScene("OnlineLoading");
    }
    
    public void Settings()
    {
        Debug.Log("Settings Pressed: Not yet Implemented");
    }
    public void Exit()
    {
        Debug.Log("Exit Pressed.");
        Application.Quit();
    }


}
