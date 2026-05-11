using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryFlagDatabaseSO", menuName = "Scriptable Objects/StoryFlagDatabaseSO")]
public class StoryFlagDatabaseSO : ScriptableObject
{
    public List<string> allFlags = new List<string>();
}
