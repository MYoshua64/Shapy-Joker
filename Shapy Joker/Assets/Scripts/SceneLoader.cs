using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void GoToScene(int index)
    {
        GameManager.gamePaused = false;
        GameManager.isGameOver = false;
        SceneManager.LoadScene(index);
    }
}
