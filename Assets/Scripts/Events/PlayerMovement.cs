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

    private void OnDisable()
    {
        // Animation stoppen, wenn das Objekt (z.B. beim Kampfstart) deaktiviert wird
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
    }

    private void FixedUpdate()
    {
        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;

        // Y-Achse ignorieren für rein flache 2D-Bewegung im 3D-Raum
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Bewegungsrichtung und Stärke berechnen
        Vector3 moveDirection = (forward * MovementInput.y + right * MovementInput.x).normalized;
        Vector3 horizontalMove = moveDirection * (MovementInput.magnitude * moveSpeed);

        // Bewegen (nur noch X und Z, Y bleibt unangetastet)
        controller.Move(horizontalMove * Time.fixedDeltaTime);

        // Rotation
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // Animator aktualisieren
        animator.SetFloat(SpeedHash, MovementInput.magnitude * moveSpeed);
    }
}