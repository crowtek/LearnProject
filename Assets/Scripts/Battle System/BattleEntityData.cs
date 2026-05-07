using UnityEngine;

[CreateAssetMenu(fileName = "NewEntity", menuName = "Scriptable Objects/Battle/Entity Data")]
public class BattleEntityData : ScriptableObject
{
    public string entityName;
    public int maxHP;
    public int attack;
    public Sprite portrait;
    public GameObject modelPrefab;
}
