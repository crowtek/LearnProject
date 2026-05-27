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
    [SerializeField] private int maxInteractions = 5;

    [Header("Events")]
    [SerializeField] private BoolEventChannelSO dialogueEventChannel;

    private bool _isDialogueActive = false;
    private Collider[] _overlapResults;

    private IInteractable _currentClosestInteractable = null;
    private Transform _lastInteractionTarget = null;

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
            if (_currentClosestInteractable != null)
            {
                _currentClosestInteractable = null;
                _lastInteractionTarget = null;
                interactionUI.Hide();
            }
            return;
        }

        IInteractable closest = FindClosestInteractable();

        // Check if interactable changed since last frame
        if (closest != _currentClosestInteractable)
        {
            _currentClosestInteractable = closest;

            if (_currentClosestInteractable != null)
            {
                // get the Transform and give it directly to the UI
                _lastInteractionTarget = _currentClosestInteractable.GetInteractionPoint();
                interactionUI.Show(_lastInteractionTarget);
            }
            else
            {
                _lastInteractionTarget = null;
                interactionUI.Hide();
            }
        }
        else if (_currentClosestInteractable != null)
        {
            if (_lastInteractionTarget != null)
            {
                interactionUI.transform.position = _lastInteractionTarget.position;
            }
        }
    }

    private void HandleDialogueStateChanged(bool isActive) => _isDialogueActive = isActive;

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (_isDialogueActive) return;

        _currentClosestInteractable?.Interact();
    }

    private IInteractable FindClosestInteractable()
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