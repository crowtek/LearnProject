using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FlagCondition
{
    [StoryFlag]
    [Tooltip("All of these story flags must be completed for the condition to pass.")]
    public List<string> requiredFlags;

    [StoryFlag]
    [Tooltip("None of these story flags may be completed for the condition to pass.")]
    public List<string> blockedFlags;

    public bool IsMet(ICollection<string> completedFlags)
    {
        if (HasBlockedFlag(completedFlags))
        {
            return false;
        }

        return HasAllRequiredFlags(completedFlags);
    }

    public bool HasAnyFlag()
    {
        return HasAnyValidFlag(requiredFlags) || HasAnyValidFlag(blockedFlags);
    }

    public bool ReferencesFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag))
        {
            return false;
        }

        return ContainsFlag(requiredFlags, flag) || ContainsFlag(blockedFlags, flag);
    }

    private bool HasAllRequiredFlags(ICollection<string> completedFlags)
    {
        if (requiredFlags == null)
        {
            return true;
        }

        foreach (string flag in requiredFlags)
        {
            if (string.IsNullOrEmpty(flag))
            {
                continue;
            }

            if (completedFlags == null || !completedFlags.Contains(flag))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasBlockedFlag(ICollection<string> completedFlags)
    {
        if (blockedFlags == null || completedFlags == null)
        {
            return false;
        }

        foreach (string flag in blockedFlags)
        {
            if (string.IsNullOrEmpty(flag))
            {
                continue;
            }

            if (completedFlags.Contains(flag))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAnyValidFlag(List<string> flags)
    {
        if (flags == null)
        {
            return false;
        }

        foreach (string flag in flags)
        {
            if (!string.IsNullOrEmpty(flag))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsFlag(List<string> flags, string targetFlag)
    {
        if (flags == null)
        {
            return false;
        }

        foreach (string flag in flags)
        {
            if (flag == targetFlag)
            {
                return true;
            }
        }

        return false;
    }
}