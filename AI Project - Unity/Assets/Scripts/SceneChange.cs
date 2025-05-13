using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public void NextScene(string sceneName)
    {
        try
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
