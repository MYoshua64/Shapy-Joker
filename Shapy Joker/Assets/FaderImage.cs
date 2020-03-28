using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaderImage : MonoBehaviour
{
    public void FadeOnSceneEntry()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", 0.75f, "onupdatetarget", gameObject,
            "onupdate", "ChangeAlpha", "oncompletetarget", Blackboard.sceneLoader.gameObject, "oncomplete", "DeactivateFader"));
    }

    public void FadeOnSceneExit()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", 0.75f, "onupdatetarget", gameObject,
            "onupdate", "ChangeAlpha", "oncompletetarget", Blackboard.sceneLoader.gameObject, "oncomplete", "LoadNewScene"));
    }

    void ChangeAlpha(float value)
    {
        GetComponent<Image>().color = new Color(0, 0, 0, value);
    }
}
