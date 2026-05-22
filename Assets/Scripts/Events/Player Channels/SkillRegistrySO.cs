using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillRegistry", menuName = "Scriptable Objects/Battle/Skill Registry")]
public class SkillRegistrySO : ScriptableObject
{
    [System.Serializable]
    public struct SkillEntry
    {
        [Tooltip("Must match the 'skillIdToUnlock' string on WeaponCategorySO skill nodes exactly.")]
        public string id;
        public BattleSkillData skill;
    }

    [SerializeField] private List<SkillEntry> entries = new List<SkillEntry>();

    private Dictionary<string, BattleSkillData> lookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, BattleSkillData>(entries.Count);
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.id))
            {
                Debug.LogWarning($"[SkillRegistry] Entry with skill '{entry.skill?.skillName}' has an empty ID — skipped.");
                continue;
            }
            if (lookup.ContainsKey(entry.id))
            {
                Debug.LogWarning($"[SkillRegistry] Duplicate skill ID '{entry.id}' — only the first entry will be used.");
                continue;
            }
            lookup[entry.id] = entry.skill;
        }
    }

    /// Returns the skill for the given ID, or null if not found
    public BattleSkillData Get(string id)
    {
        if (lookup == null) BuildLookup();
        lookup.TryGetValue(id, out var skill);
        return skill;
    }
}