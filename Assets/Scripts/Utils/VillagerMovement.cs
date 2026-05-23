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

    private static readonly int SpeedHash = Animator.StringToHash("Speed"); // verhindert unnötige String Berechnungen in der Update

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
        if (agent == null || animator == null || !agent.isActiveAndEnabled) return;

        animator.SetFloat(SpeedHash, agent.velocity.magnitude);
    }
    private async Awaitable WanderRoutine()
    {
        var token = destroyCancellationToken;

        try
        {
            while (!token.IsCancellationRequested)
            {
                // 1. Check vor der Zielbestimmung: Ist der Agent aktiv und auf dem NavMesh?
                if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
                {
                    // Warte kurz und versuche es im nächsten Zyklus erneut (wenn die Oberwelt wieder aktiv ist)
                    await Awaitable.WaitForSecondsAsync(0.5f, token);
                    continue;
                }

                Vector3 targetDestination = GetRandomPoint(transform.position, 10f);
                agent.SetDestination(targetDestination);

                while (agent.pathPending || (agent.isActiveAndEnabled && agent.isOnNavMesh && agent.remainingDistance > agent.stoppingDistance))
                {
                    // Wenn der Agent während des Gehens deaktiviert wird, brechen wir die Bewegungsschleife sofort ab
                    if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
                    {
                        break;
                    }

                    await Awaitable.EndOfFrameAsync();
                }

                // Warte 2 Sekunden vor dem nächsten Punkt, sofern wir nicht gecancelt wurden
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
