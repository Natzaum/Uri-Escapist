using UnityEngine;
using UnityEngine.AI;

public class EnemyAiTest : MonoBehaviour
{
  public NavMeshAgent agent;
  public Transform player;
  public Animator animator;
  public LayerMask whatIsGround, whatIsPlayer;

  //Patrolling
  public Vector3 walkPoint;
  bool walkPointSet;
  public float walkPointRange;

  //Attacking/catching
  public float catchRange = 1.5f;

  private void Awake()
  {
    player = GameObject.Find("Player").transform;
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponent<Animator>();
  }

  void Update()
  {
    float distanceToPlayer = Vector3.Distance(player.position, transform.position);

    if (distanceToPlayer > catchRange) Patrolling();
    if (distanceToPlayer <= catchRange) ChasePlayer();
    if (distanceToPlayer <= catchRange) CatchPlayer();
  }

  private void Patrolling()
  {
    if (!walkPointSet) SearchWalkPoint();

    if (walkPointSet)
      agent.SetDestination(walkPoint);

    Vector3 distanceToWalkPoint = transform.position - walkPoint;

    animator.SetBool("isWalking", true);

    //walkpoint reached
    if (distanceToWalkPoint.magnitude < 1f)
      walkPointSet = false;
  }

  private void SearchWalkPoint()
  {
    float randomZ = Random.Range(-walkPointRange, walkPointRange);
    float randomX = Random.Range(-walkPointRange, walkPointRange);

    walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

    if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
      walkPointSet = true;
  }

  private void ChasePlayer()
  {
    agent.SetDestination(player.position);
  }

  private void CatchPlayer()
  {
    agent.isStopped = true;
    animator.SetBool("isWalking", false);

    Debug.Log("Game Over! Player capturado!");
    FindObjectOfType<GameOverManager>().ShowGameOver();
  }
}
