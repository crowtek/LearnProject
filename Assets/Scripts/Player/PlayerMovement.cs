using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Refs")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform mainCameraTransform;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    public Vector2 MovementInput { get; set; }

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (mainCameraTransform == null) mainCameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * MovementInput.y + right * MovementInput.x).normalized;
        controller.Move(moveDirection * (MovementInput.magnitude * moveSpeed) * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        animator.SetFloat(SpeedHash, MovementInput.magnitude * moveSpeed);
    }
}