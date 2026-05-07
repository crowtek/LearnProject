using UnityEngine;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [SerializeField] private UIDocument battleHUD;
    [SerializeField] private BattleEntityData playerData;

    private VisualElement root;
    private Button closeBtn;

    void Awake()
    {
        Instance = this;
        battleHUD.gameObject.SetActive(false);
    }

    public void StartBattle(BattleEntityData enemyData)
    {
        battleHUD.gameObject.SetActive(true);
        root = battleHUD.rootVisualElement;

        var playerName = root.Q<Label>("PlayerName");
        var playerHP = root.Q<Label>("PlayerHP");
        var enemyName = root.Q<Label>("EnemyName");
        var enemyHP = root.Q<Label>("EnemyHP");

        playerName.text = playerData.entityName;
        playerHP.text = $"HP: {playerData.maxHP}";

        enemyName.text = enemyData.entityName;
        enemyHP.text = $"HP: {enemyData.maxHP}";

        closeBtn = root.Q<Button>("CloseButton");
        closeBtn.clicked += EndBattle;

        Debug.Log($"Kampf gegen {enemyData.entityName} beginnt!");
    }

    private void EndBattle()
    {
        battleHUD.gameObject.SetActive(false);
    }
}
