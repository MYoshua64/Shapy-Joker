using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] FaderImage faderImage;
    int nextLoadedScene = 0;

    private void Awake()
    {
        Blackboard.sceneLoader = this;
        faderImage.FadeOnSceneEntry();
    }

    public void DeactivateFader()
    {
        faderImage.gameObject.SetActive(false);
    }

    public void LoadNewScene()
    {
        SceneManager.LoadScene(nextLoadedScene);
    }

    public void StartTransitionToScene(int index)
    {
        GameManager.gamePaused = false;
        if (Blackboard.gm) Blackboard.gm.isGameOver = false;
        nextLoadedScene = index;
        faderImage.gameObject.SetActive(true);
        faderImage.FadeOnSceneExit();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
