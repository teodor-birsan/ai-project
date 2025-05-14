using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public GameObject agent;
    public void NextScene(string sceneName)
    {
        try
        {
            agent.SetActive(false);
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            StartCoroutine(ReactivateAgent());
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

    IEnumerator ReactivateAgent()
    {
        yield return new WaitForSeconds(0.5f);
        agent.gameObject.SetActive(true);
    }
}
