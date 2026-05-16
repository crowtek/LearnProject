using UnityEngine;

public interface IInteractable
{
    void Interact();
    string GetInteractPrompt(); // Z.B. "Reden" oder "Öffnen"
    Transform GetInteractionPoint(); // Position for interaction UI element
}