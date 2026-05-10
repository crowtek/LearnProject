using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private EncounterZoneData currentZone;
    [SerializeField] private float stepDistanceThreshold = 2f;

    private Vector3 lastPosition;
    private float distanceWalked;
    private int stepsSinceLastBattle;
    private bool isInDangerZone = true;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!isInDangerZone) return;

        float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
        distanceWalked += distanceThisFrame;
        lastPosition = transform.position;

        if (distanceWalked > stepDistanceThreshold)
        {
            Debug.Log("Distance walked: " + distanceThisFrame);
            distanceWalked = 0;
            CheckForEncounter();
        }
    }
    private void CheckForEncounter()
    {
        stepsSinceLastBattle++;

        if (stepsSinceLastBattle < currentZone.minStepsBetweenBattles) return;

        if (Random.value < currentZone.encounterProbability)
        {
            TriggerBattle();
        }
    }

    private void TriggerBattle()
    {
        stepsSinceLastBattle = 0;

        // Wähle ein zufälliges Monster aus der Zone
        int randomIndex = Random.Range(0, currentZone.possibleEncounters.Length);
        BattleEntityData randomMonster = currentZone.possibleEncounters[randomIndex];

        Debug.Log($"Random Encounter! Ein {randomMonster.entityName} erscheint!");
        BattleManager.Instance.StartBattle(randomMonster);
    }

    public void SetInDangerZone(bool active, EncounterZoneData zone = null)
    {
        isInDangerZone = active;
        if (zone != null) currentZone = zone;
        lastPosition = transform.position;
    }
}
