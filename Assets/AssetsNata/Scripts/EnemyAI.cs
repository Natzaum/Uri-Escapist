using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform[] patrolPoints;
    public Animator animator;
    public Transform player;

    [Header("Settings")]
    public float patrolSpeed = 0.3f;
    public float chaseSpeed = 0.6f;
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
    private int currentPatrolIndex = -1;
    private float waitRemaining;
    public bool IsChasing { get; private set; }
    private bool pursuing;
    private bool caught;
    private bool initialized;
    private float initialPatrolSpeed, initialChaseSpeed;
    private float speedScale = 1;

    private void Awake() => InitializeDifficulty();

    private void InitializeDifficulty()
    {
        if (initialized) return;
        initialized = true;
        agent = GetComponent<NavMeshAgent>();
        float scale = Mathf.Max(transform.localScale.x, transform.localScale.z);
        speedScale = autoScaleDistances && scale > 1 ? scale : 1;
        chaseRange *= speedScale;
        extendedDetectionRange *= speedScale;
        catchRange *= speedScale;
        waypointReachThreshold *= speedScale;
        initialPatrolSpeed = patrolSpeed * speedScale * GameDifficulty.EnemySpeedMultiplier;
        initialChaseSpeed = chaseSpeed * speedScale * GameDifficulty.EnemySpeedMultiplier;
        patrolSpeed = initialPatrolSpeed;
        chaseSpeed = initialChaseSpeed;
        alwaysFollowPlayer = MenuPrincipal.GameMode == "normal";
        alwaysChasePlayer = MenuPrincipal.GameMode == "dificil";
    }

    private void Start()
    {
        if (agent == null) { Debug.LogError("EnemyAI precisa de NavMeshAgent.", this); enabled = false; return; }
        float scale = Mathf.Max(transform.localScale.x, transform.localScale.z);
        agent.stoppingDistance = .5f * scale;
        agent.radius = .5f * scale;
        agent.height = 2f * scale;
        agent.speed = patrolSpeed;
    }

    public void ApplyScore(int correct, int errors, float perCorrect, float perError, float bonusEveryTwo)
    {
        InitializeDifficulty();
        // Recalcula a partir da base: acertar depois de errar não apaga aumentos anteriores.
        float increase = (correct * perCorrect + errors * perError + (correct / 2) * bonusEveryTwo)
            * GameDifficulty.EnemyGrowthMultiplier * speedScale;
        patrolSpeed = initialPatrolSpeed + increase;
        chaseSpeed = initialChaseSpeed + increase;
    }

    private void Update()
    {
        if (GameInterface.WorldPaused || caught || agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) return;
        float distance = Vector3.Distance(transform.position, player.position);
        bool visible = CanSeePlayer();
        bool detected = distance <= chaseRange && visible;
        if (distance <= catchRange && visible) { CatchPlayer(); return; }

        // Destinos sempre passam pelo NavMesh: a perseguição respeita os corredores.
        bool chase = alwaysChasePlayer || detected;
        IsChasing = chase;
        if (chase || alwaysFollowPlayer)
        {
            pursuing = true;
            waitRemaining = 0;
            agent.isStopped = false;
            agent.speed = chase ? chaseSpeed : patrolSpeed;
            agent.SetDestination(player.position);
            SetWalking(true);
        }
        else
        {
            if (pursuing) { pursuing = false; agent.ResetPath(); currentPatrolIndex = -1; }
            Patrol();
        }
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;
        if (waitRemaining > 0)
        {
            waitRemaining -= Time.deltaTime;
            if (waitRemaining <= 0) GoToNextPoint();
            return;
        }
        if (currentPatrolIndex < 0) { GoToNextPoint(); return; }
        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, waypointReachThreshold))
        {
            agent.isStopped = true;
            SetWalking(false);
            waitRemaining = waitTimeAtWaypoint;
            if (waitRemaining <= 0) GoToNextPoint();
        }
    }

    private void GoToNextPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) { ChoosePatrolDestination(); return; }
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            if (patrolPoints[currentPatrolIndex] == null) continue;
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            SetWalking(true);
            return;
        }
        SetWalking(false);
    }

    private void ChoosePatrolDestination()
    {
        // Cenas sem waypoints continuam patrulhando, sem usar a posição do jogador.
        float radius = Mathf.Max(12f, waypointReachThreshold * 4f);
        var path = new NavMeshPath();
        for (int i = 0; i < 20; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 candidate = transform.position + new Vector3(offset.x, 0, offset.y);
            if (!NavMesh.SamplePosition(candidate, out var hit, 3f, agent.areaMask) ||
                Vector3.Distance(transform.position, hit.position) <= waypointReachThreshold * 1.5f) continue;
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            currentPatrolIndex = 0;
            agent.isStopped = false;
            agent.SetPath(path);
            SetWalking(true);
            return;
        }
        agent.isStopped = true;
        waitRemaining = 1f;
        SetWalking(false);
    }

    private void SetWalking(bool walking) { if (animator != null) animator.SetBool("isWalking", walking); }
    private void CatchPlayer()
    {
        caught = true;
        agent.isStopped = true;
        SetWalking(false);
        GameInterface.Instance.ShowResult(false);
    }

    // Compatibilidade: scripts antigos não podem substituir as regras da dificuldade.
    public void ForceChasePlayer(float duration) { }
    public void SetAlwaysChase(bool value) { alwaysChasePlayer = MenuPrincipal.GameMode == "dificil"; }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * eyeHeight;
        Vector3 delta = target - origin;
        // Uma máscara antiga vazia não deve permitir detecção através de paredes.
        int mask = obstacleLayer.value == 0 ? Physics.DefaultRaycastLayers : obstacleLayer.value;
        foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform) ||
                hit.transform == player || hit.transform.IsChildOf(player)) continue;
            return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchRange);
    }
}
