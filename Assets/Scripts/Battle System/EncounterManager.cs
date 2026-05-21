using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private EncounterZoneData currentZone;
    [SerializeField] private float stepDistanceThreshold = 2f;

    [Header("Data Bridge")]
    [SerializeField] private BattleTransferDataSO battleTransferData;

    [Header("Event Channels")]
    [SerializeField] private BoolEventChannelSO battleStateEventChannel;

    private Vector3 lastPosition;
    private float distanceWalked;
    private int stepsSinceLastBattle;
    private bool isInDangerZone = true;

    void Start()
    {
        lastPosition = transform.position;
    }

    private void OnEnable()
    {
        if (battleStateEventChannel != null)
        {
            battleStateEventChannel.OnEventRaised += OnBattleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (battleStateEventChannel != null)
        {
            battleStateEventChannel.OnEventRaised -= OnBattleStateChanged;
        }
    }

    void Update()
    {
        if (!isInDangerZone) return;

        Vector2 currentPositionFlat = new Vector2(transform.position.x, transform.position.z);
        Vector2 lastPositionFlat = new Vector2(lastPosition.x, lastPosition.z);

        // Calculate the distance based ONLY on horizontal movement
        float distanceThisFrame = Vector2.Distance(currentPositionFlat, lastPositionFlat);

        distanceWalked += distanceThisFrame;
        lastPosition = transform.position;

        if (distanceWalked > stepDistanceThreshold)
        {
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

        int randomIndex = Random.Range(0, currentZone.possibleEncounters.Length);
        battleTransferData.PrepareBattle(currentZone.possibleEncounters[randomIndex]);

        battleStateEventChannel.RaiseEvent(false); 
        SceneManager.LoadScene("BattleScene", LoadSceneMode.Additive);
    }

    private void OnBattleStateChanged(bool isOverworldInputActive)
    {
        if (isOverworldInputActive)
        {
            isInDangerZone = true;
            stepsSinceLastBattle = 0;
            distanceWalked = 0f;
            lastPosition = transform.position;
        }
    }

    public void SetInDangerZone(bool active, EncounterZoneData zone = null)
    {
        isInDangerZone = active;
        if (zone != null) currentZone = zone;
        lastPosition = transform.position;
    }
}