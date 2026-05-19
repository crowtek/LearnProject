using System.Collections.Generic;
using UnityEngine;

// Works with StoryFlagDrawer for the Editor PropertyDrawer
[CreateAssetMenu(fileName = "StoryFlagDatabaseSO", menuName = "Scriptable Objects/StoryFlagDatabaseSO")]
public class StoryFlagDatabaseSO : ScriptableObject
{
    public List<string> allFlags = new List<string>();
}
