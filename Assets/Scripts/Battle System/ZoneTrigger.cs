using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private EncounterZoneData zoneData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<EncounterManager>().SetInDangerZone(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<EncounterManager>().SetInDangerZone(true, zoneData);
        }
    }
}
