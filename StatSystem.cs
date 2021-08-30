using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatSystem : MonoBehaviour
{
    Timer timer;

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

    public bool isPlayer = false;

    // Start is called before the first frame update
    void Awake()
    {
        timer = GameObject.Find("GameManager").GetComponent<Timer>();

        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayer)
        {
            score = Mathf.Round((health + timer.timeRemaining) * 100) / 100;
        }
    }
}
