using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GeneralAI : MonoBehaviour
{
    public Transform target;
    public Transform healthBar;
    public Animator animator;
    public NavMeshAgent navMeshAgent;

    public StatSystem stats;

    public float speedSmoothTime = 0.1f;
    float currentSpeed;
    public bool running { get; set; }

    public bool targetIsVisible { get; set; }

    public float viewRadius = 12;

    public float attackDamage = 5;
    public float attackRange = 2;
    public float attackDelay = 1;

    public bool isLeader { get; set; }
    public int maxTeamSize = 10;

    // Start is called before the first frame update
    void Awake()
    {
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
        if (healthBar == null)
        {
            healthBar = transform.Find("Health");
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if(stats == null)
        {
            stats = GetComponent<StatSystem>();
        }
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (stats.health <= 0)
        {
            Destroy(transform.GetComponent<Rigidbody>());
            Destroy(transform.GetComponent<CapsuleCollider>());
            Destroy(navMeshAgent);
            stats.health = 0;
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
            {
                animator.Play("Die");
            }
            transform.tag = "Dead";
        }
        else
        {
            navMeshAgent.speed = running ? stats.runSpeed : stats.walkSpeed;
            currentSpeed = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
            float animationSpeedPercent = running ? currentSpeed / stats.runSpeed * 2 : currentSpeed / stats.walkSpeed * 0.5f;
            animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        }

        healthBar.transform.rotation = target.rotation;
        healthBar.Find("Red").localScale = new Vector3(stats.health / stats.maxHealth, healthBar.localScale.y, healthBar.localScale.z);

        float dstToTarget = Vector3.Distance(transform.position, target.position);

        targetIsVisible = false;
        if (dstToTarget < viewRadius)
        {
            Vector3 origin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            Vector3 direction = target.position - origin;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity, 3))
            {
                if (hit.transform == target)
                {
                    targetIsVisible = true;
                }
            }
        }
    }

    public bool AtEndOfPath()
    {
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    public void MoveTo(Vector3 destination)
    {
        if (destination != null)
        {
            navMeshAgent.SetDestination(destination);
        }
    }
}
