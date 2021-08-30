using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float minutesRemaining = 5;
    public float secondsRemaining = 0;
    public float timeRemaining { get; set; }
    public string timeText { get; set; }

    private void Awake()
    {
        timeRemaining = minutesRemaining * 60 + secondsRemaining;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        minutesRemaining = Mathf.FloorToInt(timeRemaining / 60);
        secondsRemaining = Mathf.FloorToInt(timeRemaining % 60);

        timeText = string.Format("{0:00}:{1:00}", minutesRemaining, secondsRemaining);
    }

    public void ResetTimer(float minutes, float seconds)
    {
        timeRemaining = minutes * 60 + seconds;
        minutesRemaining = minutes;
        secondsRemaining = seconds;
    }

    public float GetSeconds()
    {
        return secondsRemaining;
    }
}
