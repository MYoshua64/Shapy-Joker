using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerText : MonoBehaviour
{
    TextMeshProUGUI thisText;

    private void Awake()
    {
        thisText = GetComponent<TextMeshProUGUI>();
    }

    public void StartBlinking()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 50f/255, "time", 0.25f, "looptype", iTween.LoopType.pingPong,
            "onupdatetarget", gameObject, "onupdate", "ChangeAlpha"));
    }

    void ChangeAlpha(float value)
    {
        thisText.color = new Color(thisText.color.r, thisText.color.g, thisText.color.b, value);
    }

    public void StopBlinking()
    {
        iTween.Stop(gameObject);
        float start = thisText.color.a;
        iTween.ValueTo(gameObject, iTween.Hash("from", start, "to", 1, "time", 0.25f,
            "onupdatetarget", gameObject, "onupdate", "ChangeAlpha"));
    }
}
