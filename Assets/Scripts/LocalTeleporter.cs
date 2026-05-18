using UnityEngine;

public class LocalTeleporter : MonoBehaviour
{
    [Header("Teleport Target")]
    [SerializeField] private Transform targetDestination;

    [Header("Settings")]
    [SerializeField] private bool matchRotation = true; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Teleport(other.gameObject);
        }
    }

    private void Teleport(GameObject player)
    {
        if (targetDestination == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Kein 'Target Destination' im Inspector zugewiesen!");
            return;
        }

        if (player.TryGetComponent(out CharacterController cc))
        {
            cc.enabled = false;

            player.transform.position = targetDestination.position;
            if (matchRotation)
            {
                player.transform.rotation = targetDestination.rotation;
            }

            cc.enabled = true;
        }
        else
        {
            player.transform.position = targetDestination.position;
            if (matchRotation)
            {
                player.transform.rotation = targetDestination.rotation;
            }
        }

        Debug.Log($"Spieler erfolgreich lokal nach '{targetDestination.name}' teleportiert.");
    }
}