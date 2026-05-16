using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TypewriterHandler : MonoBehaviour
{
    private Coroutine typingCoroutine;

    public void RunText(Label targetLabel, string message, float delay = 0.05f)
    {
        // Stop any existing typing before starting a new one
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(targetLabel, message, delay));
    }

    private IEnumerator TypeText(Label targetLabel, string message, float delay)
    {
        targetLabel.text = "";
        foreach (char letter in message.ToCharArray())
        {
            targetLabel.text += letter;
            yield return new WaitForSeconds(delay);
        }
        typingCoroutine = null;
    }

    public void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }
}