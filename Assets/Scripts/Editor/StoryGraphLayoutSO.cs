#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryGraphLayoutSO : ScriptableObject
{
    public List<StoryGraphNodePosition> nodePositions = new List<StoryGraphNodePosition>();
}

[Serializable]
public class StoryGraphNodePosition
{
    public string nodeKey;
    public Vector2 position;
}
#endif
