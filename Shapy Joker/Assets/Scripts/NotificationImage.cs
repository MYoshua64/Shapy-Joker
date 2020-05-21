using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationImage : MonoBehaviour
{
    [SerializeField] float stayTime = 1.0f;
    bool isActive = false;

    public void Show()
    {
        iTween.Stop(gameObject);
        GetComponent<Image>().color = new Color(1, 1, 1, 1);
        isActive = true;
        iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f, "time", 1f, "delay", stayTime,
            "onupdatetarget", gameObject, "onupdate", "ChangeAlpha", "oncompletetarget", gameObject, "oncomplete", "CancelActive"));
    }

    void ChangeAlpha(float value)
    {
        GetComponent<Image>().color = new Color(1, 1, 1, value);
    }

    void CancelActive()
    {
        isActive = false;
    }
}
