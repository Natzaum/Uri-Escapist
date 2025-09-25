using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform[] patrolPoints;   // pontos de patrulha
    public Animator animator;
    public Transform player;

    [Header("Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseRange = 7f;
    public float catchRange = 1.5f;
    public float waitTimeAtWaypoint = 2f; // tempo parado em cada ponto

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;

    private enum State { Patrol, Chase, Catch }
    private State currentState = State.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (patrolPoints.Length > 0)
        {
            GoToNextPoint();
            animator.SetBool("isWalking", true);
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        switch (currentState)
        {
            case State.Patrol:
                if (!isWaiting) // só patrulha se não estiver esperando
                    Patrol();

                if (distanceToPlayer < chaseRange)
                {
                    StopAllCoroutines(); // interrompe a espera
                    agent.isStopped = false;
                    isWaiting = false;
                    animator.SetBool("isWalking", true);
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                ChasePlayer();

                if (distanceToPlayer < catchRange)
                    currentState = State.Catch;
                else if (distanceToPlayer > chaseRange * 1.5f)
                {
                    currentState = State.Patrol;
                    agent.speed = patrolSpeed;
                    GoToNextPoint();
                }
                break;

            case State.Catch:
                CatchPlayer();
                break;
        }
    }


    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.speed = patrolSpeed;

        if (isWaiting) return; // não força andar se está esperando

        animator.SetBool("isWalking", true);

        if (!agent.pathPending && agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }


    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        Vector3 holdPos = transform.position; // guarda posição
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("isWalking", false);

        float elapsed = 0f;
        while (elapsed < waitTimeAtWaypoint)
        {
            transform.position = holdPos; // fixa posição
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
        animator.SetBool("isWalking", true);
        GoToNextPoint();

        isWaiting = false;
    }




    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        // avança para o próximo waypoint
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        Vector3 target = patrolPoints[currentPatrolIndex].position;

        // só define destino se não for praticamente o mesmo ponto
        if (Vector3.Distance(transform.position, target) > agent.stoppingDistance + 0.1f)
        {
            agent.SetDestination(target);
        }
    }


    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        animator.SetBool("isWalking", true);
        agent.SetDestination(player.position);
    }

    void CatchPlayer()
    {
        agent.isStopped = true;
        animator.SetBool("isWalking", false);

        Debug.Log("Game Over! Player capturado!");
        FindObjectOfType<GameOverManager>().ShowGameOver();
    }
}
