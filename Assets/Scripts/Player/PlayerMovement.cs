using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityMultiplier = 3f;
    [SerializeField] private float groundedGravity = -2f;

    [Header("Refs")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform mainCameraTransform;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private float verticalVelocity;

    public Vector2 MovementInput { get; set; }

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (mainCameraTransform == null) mainCameraTransform = Camera.main.transform;
    }

    private void FixedUpdate() // Gets Data from Input handler
    {
        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * MovementInput.y + right * MovementInput.x).normalized;
        Vector3 horizontalMove = moveDirection * (MovementInput.magnitude * moveSpeed);

        if (controller.isGrounded)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
        }

        Vector3 finalMovement = horizontalMove;
        finalMovement.y = verticalVelocity;

        controller.Move(finalMovement * Time.fixedDeltaTime);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        animator.SetFloat(SpeedHash, MovementInput.magnitude * moveSpeed);
    }
}