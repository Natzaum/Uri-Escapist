using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform[] patrolPoints;
    public Animator animator;
    public Transform player;

    [Header("Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseRange = 7f;
    public float catchRange = 1.5f;
    public float waitTimeAtWaypoint = 2f;
    public float waypointReachThreshold = 2f;
    
    [Header("Auto Scale")]
    public bool autoScaleDistances = true; // Ajustar distâncias baseado na escala

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;

    private enum State
    {
        Patrol,
        Chase,
        Catch,
    }

    private State currentState = State.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Ajustar configurações baseado na escala
        float scale = Mathf.Max(transform.localScale.x, transform.localScale.z);
        
        if (autoScaleDistances && scale > 1f)
        {
            // Escalar todas as distâncias
            patrolSpeed *= scale;
            chaseSpeed *= scale;
            chaseRange *= scale;
            catchRange *= scale;
            waypointReachThreshold *= scale;
            
            Debug.Log($"Enemy AI escalado! Scale: {scale}, ChaseRange: {chaseRange}, CatchRange: {catchRange}");
        }
        
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.5f * scale;
        agent.radius = 0.5f * scale;
        agent.height = 2f * scale;

        if (patrolPoints.Length > 0)
        {
            GoToNextPoint();
            if (animator != null)
                animator.SetBool("isWalking", true);
        }
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogWarning("Player não encontrado! Certifique-se que o Player tem a tag 'Player'");
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        
        // Debug visual
        Debug.DrawLine(transform.position, player.position, 
            distanceToPlayer < chaseRange ? Color.red : Color.blue);

        switch (currentState)
        {
            case State.Patrol:
                if (!isWaiting)
                    Patrol();

                if (distanceToPlayer < chaseRange)
                {
                    Debug.Log($"Player detectado! Distância: {distanceToPlayer} < ChaseRange: {chaseRange}");
                    StopAllCoroutines();
                    agent.isStopped = false;
                    isWaiting = false;
                    if (animator != null)
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
        if (patrolPoints.Length == 0)
            return;

        agent.speed = patrolSpeed;

        if (isWaiting)
            return;

        animator.SetBool("isWalking", true);

        // Verificação melhorada para waypoint alcançado
        if (patrolPoints[currentPatrolIndex] != null)
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position);
            
            if (distanceToWaypoint <= waypointReachThreshold)
            {
                if (!agent.pathPending && agent.hasPath)
                {
                    StartCoroutine(WaitAtWaypoint());
                }
            }
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        Vector3 holdPos = transform.position;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("isWalking", false);

        float elapsed = 0f;
        while (elapsed < waitTimeAtWaypoint)
        {
            transform.position = holdPos;
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
        if (patrolPoints.Length == 0)
            return;

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        Vector3 target = patrolPoints[currentPatrolIndex].position;

        // Sempre definir o destino, o NavMesh vai lidar com isso
        agent.SetDestination(target);
    }

    public void ForceChasePlayer(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ForceChase(duration));
    }

    private IEnumerator ForceChase(float duration)
    {
        Debug.Log($"🚨 INIMIGO ALERTADO! Perseguindo por {duration} segundos!");
        
        // IMPORTANTE: Mudar para estado de perseguição
        currentState = State.Chase;
        agent.isStopped = false;
        isWaiting = false;
        
        // IMPORTANTE: Mudar para velocidade de perseguição
        agent.speed = chaseSpeed;
        
        if (animator != null)
            animator.SetBool("isWalking", true);
        
        float timer = 0f;

        while (timer < duration)
        {
            if (player == null)
                player = GameObject.FindWithTag("Player")?.transform;
                
            if (player != null)
            {
                agent.SetDestination(player.position);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("⏱️ Tempo de alerta acabou. Voltando para patrulha.");
        currentState = State.Patrol;
        agent.speed = patrolSpeed;
        GoToNextPoint();
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
        if (animator != null)
            animator.SetBool("isWalking", false);

        Debug.Log("Game Over! Player capturado!");
        GameOverManager gom = FindObjectOfType<GameOverManager>();
        if (gom != null)
            gom.ShowGameOver();
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualizar chase range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Visualizar catch range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchRange);
        
        // Linha para o player
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
