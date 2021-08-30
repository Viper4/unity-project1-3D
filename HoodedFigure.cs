using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HoodedFigure : MonoBehaviour
{
    GeneralAI generalAI;
    public Transform leaderIcon;

    List<Target> team = new List<Target>();
    public int confidenceLevel = 3;
    public float[] waitTime = { 2, 5 };
    public float[] seekDst = { 3, 6 };

    Vector3 targetLastPosition;

    public enum State
    {
        Idle,
        Wander,
        Seek,
        Chase,
        Attack,
        Panic
    }
    State currentState = State.Idle;
    State desiredState = State.Wander;

    // Start is called before the first frame update
    void Awake()
    {
        generalAI = GetComponent<GeneralAI>();
        if (leaderIcon == null)
        {
            leaderIcon = transform.Find("Leader Icon");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!CompareTag("Dead"))
        {
            AI();
        }
        else
        {
            leaderIcon.gameObject.SetActive(false);
        }
    }

    private void AI()
    {
        bool targetIsNear = false;
        int leaderCount = 0;
        int confidenceBoost = 0;
        Target[] allTargets = FindObjectsOfType<Target>();
        team.Clear();
        foreach (Target currentTarget in allTargets)
        {
            float dstToCurrentTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            switch (currentTarget.tag)
            {
                case "Hostile":
                    if(dstToCurrentTarget < generalAI.viewRadius)
                    {
                        if (!team.Contains(currentTarget))
                        {
                            if(currentTarget.name == "HoodedGuard")
                            {
                                confidenceBoost += 1;
                            }
                            team.Add(currentTarget);
                        }
                        if(currentTarget.transform != transform)
                        {
                            if (!generalAI.targetIsVisible && currentTarget.GetComponent<GeneralAI>().targetIsVisible)
                            {
                                targetIsNear = true;
                            }
                            
                            if (!generalAI.isLeader && currentTarget.GetComponent<GeneralAI>().isLeader && dstToCurrentTarget > generalAI.viewRadius - 1)
                            {
                                generalAI.MoveTo(currentTarget.transform.position);
                            }
                        }
                        if (currentTarget.GetComponent<GeneralAI>().isLeader)
                        {
                            leaderCount += 1;
                            if (currentTarget.transform != transform && team.Count <= generalAI.maxTeamSize && generalAI.isLeader)
                            {
                                generalAI.isLeader = false;
                            }
                        }
                    }
                    break;
                case "Player":
                    if(dstToCurrentTarget < generalAI.attackRange && currentState == State.Chase)
                    {
                        StartCoroutine(Attack(currentTarget));
                    }
                    break;
            }
        }

        //Picking a leader if none in a team of at least 2
        if (team.Count > 1 && leaderCount == 0)
        {
            Target newLeader = null;
            float bestScore = -Mathf.Infinity;
            foreach (Target potentialLeader in team)
            {
                StatSystem currentStats = potentialLeader.GetComponent<StatSystem>();
                GeneralAI currentGeneralAI = potentialLeader.GetComponent<GeneralAI>();
                //Calculating score based off different criteria
                float score = currentStats.health + currentStats.maxHealth + currentStats.walkSpeed + currentStats.runSpeed + currentGeneralAI.viewRadius + currentGeneralAI.attackDamage - currentGeneralAI.attackDelay + currentGeneralAI.attackRange;
                if (score > bestScore)
                {
                    bestScore = score;
                    newLeader = potentialLeader;
                }
            }
            newLeader.GetComponent<GeneralAI>().isLeader = true;
        }

        //Icon to identify the leader
        leaderIcon.gameObject.SetActive(generalAI.isLeader);
        leaderIcon.rotation = generalAI.target.rotation;

        //If target is visible otherwise do other actions
        if (generalAI.targetIsVisible || targetIsNear)
        {
            generalAI.running = true;
            desiredState = State.Seek;
            targetLastPosition = generalAI.target.position;

            //If confident Chase otherwise panic
            if(team.Count >= confidenceLevel - confidenceBoost)
            {
                if(currentState != State.Attack)
                {
                    currentState = State.Chase;
                }
                generalAI.MoveTo(generalAI.target.position);
            }
            else
            {
                currentState = State.Panic;
                generalAI.MoveTo(transform.position + -(generalAI.target.position - transform.position).normalized * 3);
            }
        }
        else 
        {
            if(currentState != desiredState)
            {
                switch (desiredState)
                {
                    case State.Idle:
                        StartCoroutine(Idle());
                        break;
                    case State.Wander:
                        StartCoroutine(Wander());
                        break;
                    case State.Seek:
                        StartCoroutine(Seek());
                        break;
                }
            }
        }
    }

    IEnumerator Idle()
    {
        currentState = State.Idle;
        //waitTime is shorter if I am the leader
        float[] newWaitTime = { waitTime[0], waitTime[1] };
        for (int i = 0; i < waitTime.Length; i++)
        {
            newWaitTime[i] = generalAI.isLeader ? waitTime[i] / 2 : waitTime[i];
        }
        yield return new WaitForSeconds(Random.Range(newWaitTime[0], newWaitTime[1]));

        desiredState = State.Wander;
    }

    IEnumerator Wander()
    {
        generalAI.running = false;
        currentState = State.Wander;
        //When alone wander farther otherwise wander normally
        Vector3 randomDirection = team.Count == 1 ? Random.onUnitSphere * Random.Range(generalAI.viewRadius / 2, generalAI.viewRadius * 2) : Random.insideUnitSphere * generalAI.viewRadius;

        randomDirection += transform.position;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, generalAI.viewRadius, 1);
        generalAI.MoveTo(hit.position);

        yield return new WaitUntil(generalAI.AtEndOfPath);
        desiredState = State.Idle;
    }

    IEnumerator Seek()
    {
        generalAI.running = true;
        currentState = State.Seek;
        float runDst = Random.Range(seekDst[0], seekDst[1]);
        generalAI.MoveTo(targetLastPosition * runDst);

        yield return new WaitUntil(generalAI.AtEndOfPath);
        desiredState = State.Idle;
    }

    IEnumerator Attack(Target target)
    {
        currentState = State.Attack;
        target.GetComponent<StatSystem>().health -= generalAI.attackDamage;

        yield return new WaitForSeconds(generalAI.attackDelay);
        currentState = State.Chase;
    }
}
