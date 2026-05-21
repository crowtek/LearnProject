using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private EncounterZoneData zoneData;

    [Header("Audio Channels")]
    [SerializeField] private AudioEventChannelSO musicChannel;
    [SerializeField] private AudioEventChannelSO sfxChannel;

    [Header("Audio Configurations")]
    [SerializeField] private AudioConfigurationSO BGM_enter;
    [SerializeField] private AudioConfigurationSO BGM_exit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<EncounterManager>().SetInDangerZone(false);
            musicChannel.RaiseEvent(BGM_enter);       
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<EncounterManager>().SetInDangerZone(true, zoneData);
            musicChannel.RaiseEvent(BGM_exit);
        }
    }
}
