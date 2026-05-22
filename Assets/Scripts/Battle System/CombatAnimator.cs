using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Attach this to every combat prefab (player and enemy).
/// BattleManager calls its methods instead of touching Animator directly.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CombatAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Fires a trigger and invokes the callback once the animation clip is near its end.
    /// Uses the clip's length so you don't have to hardcode delays everywhere.
    /// Falls back to the supplied <paramref name="fallbackDuration"/> if the trigger name
    /// can't be found in the current Animator state.
    /// </summary>
    public void PlayAndThen(string triggerName, float fallbackDuration, Action onComplete)
    {
        if (string.IsNullOrEmpty(triggerName))
        {
            Invoke(nameof(NullAction), fallbackDuration);
            StartCoroutine(DelayedCallback(fallbackDuration, onComplete));
            return;
        }

        animator.SetTrigger(triggerName);
        StartCoroutine(WaitForAnimationAndCallback(triggerName, fallbackDuration, onComplete));
    }

    /// <summary>Plays a trigger with no completion callback.</summary>
    public void Play(string triggerName)
    {
        if (!string.IsNullOrEmpty(triggerName))
            animator.SetTrigger(triggerName);
    }

    // -------------------------------------------------------------------------

    private IEnumerator WaitForAnimationAndCallback(string triggerName, float fallbackDuration, Action onComplete)
    {
        // Give the Animator one frame to transition into the new state
        yield return null;

        // Try to get the length of the current state
        float clipLength = fallbackDuration;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.length > 0.01f)
            clipLength = info.length;

        // Wait until roughly 90% through so the next action starts feeling snappy
        yield return new WaitForSeconds(clipLength * 0.9f);
        onComplete?.Invoke();
    }

    private IEnumerator DelayedCallback(float delay, Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }

    private void NullAction() { }
}
