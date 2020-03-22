using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Tooltip("Turn time in minutes")]
    [SerializeField] [Range(1, 5)] float turnTime;
    float timeCountDown;
    bool running = false;

    private void Awake()
    {
        Blackboard.timer = this;
    }

    public void Run()
    {
        timeCountDown = turnTime * 60;
        running = true;
    }

    public void Stop()
    {
        running = false;
    }

    private void Update()
    {
        if (!running || GameManager.gamePaused) return;
        timeCountDown -= Time.deltaTime;
        float counter = Mathf.Ceil(timeCountDown);
        Blackboard.cm.UpdateTimerText(counter);
        if (timeCountDown <= 0)
        {
            Stop();
            Blackboard.gm.HandleTurnEnd(true);
        }
    }


}
