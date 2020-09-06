using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Tooltip("Turn time in minutes")]
    float turnTime;
    public float timeCountDown { get; private set; }
    bool running = false;

    private void Awake()
    {
        Blackboard.timer = this;
    }

    public void SetTurnTime(float time)
    {
        turnTime = time;
    }

    public void ResetTimer()
    {
        timeCountDown = turnTime;
    }

    public void Run()
    {
        running = true;
    }

    public void Stop()
    {
        Blackboard.cm.StopTimerTextBlinking();
        running = false;
    }

    private void Update()
    {
        if (!running || GameManager.gamePaused) return;
        timeCountDown -= Time.deltaTime;
        Blackboard.cm.UpdateTimerText();
        if (timeCountDown <= 0)
        {
            Stop();
            Blackboard.gm.HandleTurnEnd(true);
        }
    }
}
