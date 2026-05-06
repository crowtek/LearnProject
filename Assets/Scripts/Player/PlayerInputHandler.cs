using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    private InputSystem_Actions inputs;

    void Awake()
    {
        inputs = new InputSystem_Actions();

        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement reference is missing in PlayerInputHandler.");
            return;
        }
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();

    void Update()
    {
        Vector2 rawInput = inputs.Player.Move.ReadValue<Vector2>();
        playerMovement.MovementInput = rawInput;
    }

    public void SetInputActive(bool active)
    {
        if(active)
            inputs.Enable();
        else
            inputs.Disable();
    }
}
