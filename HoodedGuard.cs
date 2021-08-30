using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoodedGuard : MonoBehaviour
{
    GeneralAI generalAI;
    public Transform leaderIcon;
    public Transform returnPoint;
    Vector3 returnPosition;

    public float[] patrolTime = { 2, 5 };
    public float[] seekDst = { 4, 6 };

    Vector3 targetLastPosition;

    List<Target> team = new List<Target>();

    public int maxTeamSize = 15;

    public enum State
    {
        Idle,
        Return,
        Seek,
        Chase,
        Attack,
        Patrol
    }
    State currentState = State.Idle;
    State desiredState = State.Idle;

    // Start is called before the first frame update
    void Awake()
    {
        generalAI = GetComponent<GeneralAI>();
        if (leaderIcon == null)
        {
            leaderIcon = transform.Find("Leader Icon");
        }
        if (returnPoint == null)
        {
            try
            {
                Debug.Log("Setting closest return point for " + gameObject.name);
                GameObject bestTarget = null;
                float closestDistance = Mathf.Infinity;
                foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag("ReturnPoint"))
                {
                    float distanceToTarget = Vector3.Distance(transform.position, potentialTarget.transform.position);
                    if (distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        bestTarget = potentialTarget;
                    }
                }
                returnPoint = bestTarget.transform;
            }
            catch
            {
                Debug.LogWarning("No return points found setting return point to " +  transform.position);
                returnPoint = transform;
            }
        }

        returnPosition = returnPoint.position;

        team.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if (!CompareTag("Dead"))
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
        Target[] allTargets = FindObjectsOfType<Target>();
        foreach (Target currentTarget in allTargets)
        {
            float dstToCurrentTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            switch (currentTarget.tag)
            {
                case "Hostile":
                    if (dstToCurrentTarget < generalAI.viewRadius)
                    {
                        if (!team.Contains(currentTarget))
                        {
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
                                returnPosition = currentTarget.transform.position;
                                generalAI.MoveTo(currentTarget.transform.position);
                            }
                        }
                    }
                    else
                    {
                        if (team.Contains(currentTarget))
                        {
                            team.Remove(currentTarget);
                        }
                    }
                    break;
                case "Player":
                    if (dstToCurrentTarget < generalAI.attackRange && currentState == State.Chase)
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
            foreach(Target potentialLeader in team)
            {
                StatSystem currentStats = potentialLeader.GetComponent<StatSystem>();
                GeneralAI currentGeneralAI = potentialLeader.GetComponent<GeneralAI>();
                //Calculating score based off different criteria
                float score = currentStats.health + currentStats.maxHealth + currentStats.walkSpeed + currentStats.runSpeed + currentGeneralAI.viewRadius + currentGeneralAI.attackDamage - currentGeneralAI.attackDelay + currentGeneralAI.attackRange;
                if(score > bestScore)
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

            if (currentState != State.Attack)
            {
                currentState = State.Chase;
            }
            generalAI.MoveTo(generalAI.target.position);
        }
        else
        {
            if (currentState != desiredState)
            {
                switch (desiredState)
                {
                    case State.Patrol:
                        StartCoroutine(Patrol());
                        break;
                    case State.Seek:
                        StartCoroutine(Seek());
                        break;
                    case State.Return:
                        StartCoroutine(Return());
                        break;
                }
            }
        }
    }

    IEnumerator Patrol()
    {
        generalAI.running = false;
        currentState = State.Patrol;

        StartCoroutine(Patroling());

        //When alone patrol for shorter
        float waitTime = team.Count == 1 ? Random.Range(patrolTime[0], patrolTime[1]) / 2: Random.Range(patrolTime[0], patrolTime[1]);
        yield return new WaitForSeconds(waitTime);
        desiredState = State.Return;
    }

    IEnumerator Patroling()
    {
        generalAI.MoveTo(transform.position + transform.right * 5 + -transform.forward * 5);

        yield return new WaitUntil(generalAI.AtEndOfPath);
        if(desiredState == State.Patrol)
        {
            StartCoroutine(Patroling());
        }
    }

    IEnumerator Seek()
    {
        generalAI.running = true;
        currentState = State.Seek;
        float runDst = Random.Range(seekDst[0], seekDst[1]);
        generalAI.MoveTo(targetLastPosition * runDst);

        yield return new WaitUntil(generalAI.AtEndOfPath);
        desiredState = State.Patrol;
    }

    IEnumerator Return()
    {
        generalAI.running = false;
        if(returnPoint != null)
        {
            returnPosition = returnPoint.position;
        }
        currentState = State.Return;
        generalAI.MoveTo(returnPosition);

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
