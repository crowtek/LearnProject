using System;
using System.Collections.Generic;

[Serializable]
public class StoryProgressSaveData
{
    public int version = 1;
    public List<string> completedFlags = new List<string>();
    public string lastFlag;
}