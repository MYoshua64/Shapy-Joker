using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] FaderImage faderImage;
    public static bool isLoading = true;
    int nextLoadedScene = 0;

    private void Awake()
    {
        Blackboard.sceneLoader = this;
        faderImage.FadeOnSceneEntry();
    }

    public void DeactivateFader()
    {
        faderImage.gameObject.SetActive(false);
        isLoading = false;
    }

    public void LoadNewScene()
    {
        SceneManager.LoadScene(nextLoadedScene);
    }

    public void StartTransitionToScene(int index)
    {
        if (isLoading) return;
        isLoading = true;
        Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
        GameManager.gamePaused = false;
        if (Blackboard.gm) Blackboard.gm.isGameOver = false;
        nextLoadedScene = index;
        faderImage.gameObject.SetActive(true);
        faderImage.FadeOnSceneExit();
    }

    public void ExitGame()
    {
        if (isLoading) return;
        isLoading = true;
        Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
        faderImage.gameObject.SetActive(true);
        faderImage.FadeOnApplicationQuit();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
