using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TypewriterHandler : MonoBehaviour
{
    private Coroutine typingCoroutine;

    // Das UI kann von außen prüfen, ob der Typewriter noch läuft
    public bool IsTyping => typingCoroutine != null;

    public void RunText(Label targetLabel, string message, float delay = 0.05f)
    {
        // Falls bereits ein Text getippt wird, stoppen wir ihn
        StopTyping();

        typingCoroutine = StartCoroutine(TypeTextRoutine(targetLabel, message, delay));
    }

    private IEnumerator TypeTextRoutine(Label targetLabel, string message, float delay)
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