using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatSystem : MonoBehaviour
{
    public float walkSpeed = 2;
    public float runSpeed = 4;
    public float speedMultiplier = 1;
    public float jumpHeight = 1;
    [Range(0, Mathf.Infinity)] public float maxStamina = 100;
    [Range(0, Mathf.Infinity)] public float stamina = 100;

    [Range(0, Mathf.Infinity)] public float maxHealth = 100;
    [Range(0, Mathf.Infinity)] public float health = 100;

    public float score;
    public Dictionary<string, float> highScores = new Dictionary<string, float>();
    public List<string> inventory = new List<string>();
    public float minutesRemaining = 5;
    public float secondsRemaining = 0;
    public float timeRemaining { get; set; }
    public string timeText { get; set; }

    public bool isPlayer = false;

    // Start is called before the first frame update
    void Awake()
    {
        timeRemaining = secondsRemaining + minutesRemaining * 60;
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayer)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            minutesRemaining = Mathf.FloorToInt(timeRemaining / 60);
            secondsRemaining = Mathf.FloorToInt(timeRemaining % 60);

            timeText = string.Format("{0:00}:{1:00}", minutesRemaining, secondsRemaining);

            score = Mathf.Round((health + timeRemaining) * 100) / 100;
        }
    }
}
