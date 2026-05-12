using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Detection Settings")]
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private LayerMask interactLayer;

    [Header("Events")]
    [SerializeField] private BoolEventChannelSO dialogueEventChannel;

    private bool _isDialogueActive = false;

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            // Sicherstellen, dass die Map aktiv ist
            interactAction.action.actionMap.Enable();
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }

        if (dialogueEventChannel != null)
            dialogueEventChannel.OnEventRaised += HandleDialogueStateChanged;
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }

        if (dialogueEventChannel != null)
            dialogueEventChannel.OnEventRaised -= HandleDialogueStateChanged;
    }

    private void HandleDialogueStateChanged(bool isActive)
    {
        _isDialogueActive = isActive;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Wenn bereits ein Dialog läuft, ignorieren wir den Tastendruck
        if (_isDialogueActive) return;

        PerformOverlapCheck();
    }

    public void PerformOverlapCheck()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            interactRadius,
            interactLayer,
            QueryTriggerInteraction.Collide
        );

        if (colliders.Length == 0) return;

        // Den nächsten Interaktionspartner finden
        var closest = colliders
            .OrderBy(c => Vector3.Distance(transform.position, c.transform.position))
            .FirstOrDefault();

        if (closest != null && closest.TryGetComponent(out IInteractable interactable))
        {
            Debug.Log($"Interagiere mit: {closest.name}");
            interactable.Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}