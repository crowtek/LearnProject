using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabaseSO", menuName = "Scriptable Objects/Quest/QuestDatabaseSO")]
public class QuestDatabaseSO : ScriptableObject
{
    public List<QuestDataSO> questList = new List<QuestDataSO> {};
}
