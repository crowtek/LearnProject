using UnityEngine;

[CreateAssetMenu(fileName = "EncounterZoneData", menuName = "Scriptable Objects/Battle/EncounterZoneData")]
public class EncounterZoneData : ScriptableObject
{
    public string zoneName;
    public float encounterProbability = 0.1f;
    public int minStepsBetweenBattles = 10;
    public BattleEntityData[] possibleEncounters;
}
