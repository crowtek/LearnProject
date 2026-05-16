using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InteractionUIController interactionUI;

    [Header("Detection Settings")]
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private int maxInteractions = 5; // Limit detection for performance

    [Header("Events")]
    [SerializeField] private BoolEventChannelSO dialogueEventChannel;

    private bool _isDialogueActive = false;
    private Collider[] _overlapResults; // Pre-allocated array to avoid GC

    private void Awake()
    {
        _overlapResults = new Collider[maxInteractions];
    }

    private void OnEnable()
    {
        if (interactAction?.action != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }

        if (dialogueEventChannel != null)
            dialogueEventChannel.OnEventRaised += HandleDialogueStateChanged;
    }

    private void OnDisable()
    {
        if (interactAction?.action != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
        }

        if (dialogueEventChannel != null)
            dialogueEventChannel.OnEventRaised -= HandleDialogueStateChanged;
    }

    private void Update()
    {
        if (_isDialogueActive)
        {
            interactionUI.Hide();
            return;
        }

        IInteractable closest = GetClosestInteractable();

        if (closest != null)
        {
            interactionUI.Show(closest.GetInteractionPoint());
        }
        else
        {
            interactionUI.Hide();
        }
    }

    private void HandleDialogueStateChanged(bool isActive) => _isDialogueActive = isActive;

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (_isDialogueActive) return;

        IInteractable interactable = GetClosestInteractable();
        interactable?.Interact();
    }

    private IInteractable GetClosestInteractable()
    {
        int numFound = Physics.OverlapSphereNonAlloc(
            transform.position,
            interactRadius,
            _overlapResults,
            interactLayer,
            QueryTriggerInteraction.Collide
        );

        if (numFound == 0) return null;

        IInteractable closestInteractable = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < numFound; i++)
        {
            if (_overlapResults[i].TryGetComponent(out IInteractable interactable))
            {
                float dist = Vector3.SqrMagnitude(transform.position - _overlapResults[i].transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestInteractable = interactable;
                }
            }
        }

        return closestInteractable;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}