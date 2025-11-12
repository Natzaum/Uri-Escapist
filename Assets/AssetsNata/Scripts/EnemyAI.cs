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
    public float patrolSpeed = 0.375f;
    public float chaseSpeed = 0.75f;
    public float chaseRange = 5f;
    public float catchRange = 1.5f;
    public float waitTimeAtWaypoint = 2f;
    public float waypointReachThreshold = 2f;
    
    [Header("Auto Scale")]
    public bool autoScaleDistances = true; // Ajustar distâncias baseado na escala
    
    [Header("Chase Behavior")]
    public bool alwaysFollowPlayer = false; // Se true, sempre vai para o player em patrol speed até chegar perto
    public bool alwaysChasePlayer = false; // Se true, sempre persegue em chase speed (ignora distância)
    public float extendedDetectionRange = 20f; // Alcance de detecção aumentado
    
    [Header("Vision")]
    [Tooltip("Se marcado, inimigo não vê através de paredes")]
    public bool hasLineOfSight = true;
    
    [Tooltip("Layer de objetos que bloqueiam visão")]
    public LayerMask obstacleLayer;
    
    [Tooltip("Altura dos olhos do inimigo (para raycast)")]
    public float eyeHeight = 1.5f;

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
        
        // Usar alcance de detecção aumentado
        float effectiveChaseRange = (alwaysChasePlayer || alwaysFollowPlayer) ? extendedDetectionRange : chaseRange;
        
        // Verificar se tem linha de visão (não vê através de paredes)
        bool canSeePlayer = !hasLineOfSight || CanSeePlayer();
        
        // Debug visual (vermelho = perseguindo, amarelo = seguindo, azul = patrulhando, cinza = bloqueado)
        Color debugColor = Color.blue;
        if (!canSeePlayer) 
            debugColor = Color.gray; // Cinza: visão bloqueada
        else if (alwaysChasePlayer) 
            debugColor = Color.red;
        else if (alwaysFollowPlayer) 
            debugColor = Color.yellow;
        else if (distanceToPlayer < effectiveChaseRange) 
            debugColor = Color.red;
        
        Debug.DrawLine(transform.position, player.position, debugColor);

        // MODO 1: Always Chase Player (sempre em chase speed) - MAS SÓ SE VEJO
        if (alwaysChasePlayer && currentState != State.Catch && canSeePlayer)
        {
            if (currentState != State.Chase)
            {
                Debug.Log("🎯 Modo Always Chase ativo - perseguindo em alta velocidade!");
                currentState = State.Chase;
                agent.speed = chaseSpeed;
            }
        }
        
        // MODO 2: Always Follow Player (patrol speed até chegar perto, depois chase) - MAS SÓ SE VEJO
        if (alwaysFollowPlayer && !alwaysChasePlayer && currentState != State.Catch && canSeePlayer)
        {
            if (distanceToPlayer < chaseRange)
            {
                // Chegou perto, mudar para Chase
                if (currentState != State.Chase)
                {
                    Debug.Log($"🏃 Player perto ({distanceToPlayer:F1}m)! Mudando para Chase Speed!");
                    currentState = State.Chase;
                    agent.speed = chaseSpeed;
                }
            }
            else
            {
                // Longe, manter em Patrol (mas indo para o player)
                if (currentState != State.Patrol)
                {
                    Debug.Log("🚶 Player longe, seguindo em Patrol Speed");
                    currentState = State.Patrol;
                    agent.speed = patrolSpeed;
                }
            }
        }

        switch (currentState)
        {
            case State.Patrol:
                if (!isWaiting)
                    Patrol();

                // Detecção com alcance configurável
                if (distanceToPlayer < effectiveChaseRange)
                {
                    Debug.Log($"Player detectado! Distância: {distanceToPlayer:F1} < Range: {effectiveChaseRange:F1}");
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
                else if (distanceToPlayer > effectiveChaseRange * 1.5f && !alwaysChasePlayer)
                {
                    // Só voltar para patrulha se NÃO estiver em always chase
                    Debug.Log("Player muito longe, voltando para patrulha");
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
        agent.speed = patrolSpeed;

        if (isWaiting)
            return;

        if (animator != null)
            animator.SetBool("isWalking", true);

        // MODO 1: Always Chase Player - vai direto em alta velocidade
        if (alwaysChasePlayer && player != null)
        {
            agent.SetDestination(player.position);
            return;
        }
        
        // MODO 2: Always Follow Player - vai direto em baixa velocidade (patrol speed)
        if (alwaysFollowPlayer && player != null)
        {
            agent.SetDestination(player.position);
            return;
        }

        // Patrulha normal com waypoints
        if (patrolPoints.Length == 0)
            return;

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
        Debug.Log($"🚨 INIMIGO ALERTADO POR ERRO! Perseguindo em velocidade de Chase por {duration}s!");
        
        // Parar patrulha
        StopAllCoroutines();
        isWaiting = false;
        
        // FORÇAR estado de Chase
        currentState = State.Chase;
        agent.isStopped = false;
        
        // USAR velocidade de CHASE (não patrulha!)
        agent.speed = chaseSpeed;
        Debug.Log($"⚡ Velocidade definida para Chase Speed: {chaseSpeed}");
        
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
    
    // Método público para ativar/desativar perseguição constante
    public void SetAlwaysChase(bool value)
    {
        alwaysChasePlayer = value;
        
        if (value)
        {
            Debug.Log("🎯 Modo Always Chase ATIVADO - Inimigo perseguirá sempre!");
            currentState = State.Chase;
            agent.speed = chaseSpeed;
        }
        else
        {
            Debug.Log("🎯 Modo Always Chase DESATIVADO - Inimigo voltará a patrulhar");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualizar chase range normal
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Visualizar extended detection range (se always chase ativo)
        if (alwaysChasePlayer)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, extendedDetectionRange);
        }
        
        // Visualizar catch range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchRange);
        
        // Linha para o player
        if (player != null)
        {
            Gizmos.color = alwaysChasePlayer ? Color.red : Color.blue;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

    private bool CanSeePlayer()
    {
        // If player is not found, cannot see
        if (player == null) return false;
        
        // Get eye position (slightly above ground)
        Vector3 eyePosition = transform.position + transform.up * eyeHeight;
        
        // Direction from enemy to player
        Vector3 directionToPlayer = player.position - eyePosition;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Perform raycast from eye position to player
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(eyePosition, directionToPlayer.normalized, out hit, distanceToPlayer, obstacleLayer);
        
        // Debug visualization
        if (hitSomething)
        {
            // Vision is blocked
            Debug.DrawLine(eyePosition, hit.point, Color.red);
            Debug.DrawLine(hit.point, player.position, Color.gray);
            return false;
        }
        else
        {
            // Vision is clear
            Debug.DrawLine(eyePosition, player.position, Color.green);
            return true;
        }
    }
}
