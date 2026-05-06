using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class VillagerMovement : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    // verhindert unnötige String Berechnungen in der Update
    private static readonly int SpeedHash = Animator.StringToHash("Speed"); 

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    async void Start()
    {
        await WanderRoutine();
    }

    void Update()
    {
        animator.SetFloat(SpeedHash, agent.velocity.magnitude);
    }

    private async Awaitable WanderRoutine()
    {
        var token = destroyCancellationToken;

        try
        {
            while (!token.IsCancellationRequested)
            {
                Vector3 targetDestination = GetRandomPoint(transform.position, 10f);
                agent.SetDestination(targetDestination);

                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                {
                    await Awaitable.EndOfFrameAsync();
                }

                await Awaitable.WaitForSecondsAsync(2f, token);
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("Wander routine canceled.");
        }
        
    }

    Vector3 GetRandomPoint(Vector3 center, float distance)
    {
        Vector3 randomPos = Random.insideUnitSphere * distance;
        randomPos += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center;
    }
}
