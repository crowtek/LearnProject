using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewEntity", menuName = "Scriptable Objects/Battle/Entity Data")]
public class BattleEntityData : ScriptableObject
{
    public string entityName;
    public int maxHP;
    public int attack;
    public int defense;
    public int agility;
    public int expReward;
    public Sprite portrait;
    public Sprite battleImage;
}
