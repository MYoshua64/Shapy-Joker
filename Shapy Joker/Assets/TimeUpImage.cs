using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeUpImage : MonoBehaviour
{
    public void Show()
    {
        GetComponent<Image>().color = new Color(1, 1, 1, 1);
        iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f, "time", 1f, "delay", 1f,
            "onupdatetarget", gameObject, "onupdate", "ChangeAlpha"));
    }

    void ChangeAlpha(float value)
    {
        GetComponent<Image>().color = new Color(1, 1, 1, value);
    }
}
