using UnityEngine;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [SerializeField] private UIDocument battleHUD;
    [SerializeField] private BattleEntityData playerTemplate; 
    [SerializeField] private PlayerRuntimeState playerRuntime;
    [SerializeField] private BoolEventChannelSO battleStateEventChannel;

    [Header("Results")]
    [SerializeField] private BattleResultEventChannelSO battleResultEventChannel;
    private int accumulatedEXP;

    private int currentPlayerHP;
    private int currentEnemyHP;
    private string currentEnemyName;

    private VisualElement root;
    private VisualElement playerPortre;
    private VisualElement playerSprite;
    private VisualElement enemySprite;
    private Button closeBtn;
    private Button attackBtn;
    private Button defButton;
    private Label playerHPLabel;
    private VisualElement HpBarFill;
    private Label level;

    public enum BattleState { Idle, Start, PlayerTurn, EnemyTurn, Busy, Won, Lost, End }
    private BattleState currentState = BattleState.Idle;

    void Awake()
    {
        Instance = this;
        battleHUD.gameObject.SetActive(false);
    }

    public void StartBattle(BattleEntityData enemyData)
    {
        if (currentState != BattleState.Idle) return;

        currentPlayerHP = playerRuntime.currentHP;
        currentEnemyHP = enemyData.maxHP;
        currentEnemyName = enemyData.entityName;

        battleHUD.gameObject.SetActive(true);
        currentState = BattleState.Start;
        root = battleHUD.rootVisualElement;

        playerPortre = root.Q<VisualElement>("PlayerImage");
        playerSprite = root.Q<VisualElement>("PlayerSprite");
        enemySprite = root.Q<VisualElement>("EnemySprite");
        playerHPLabel = root.Q<Label>("PlayerHP");
        HpBarFill = root.Q<VisualElement>("HPBarFill");
        level = root.Q<Label>("level");

        root.Q<Label>("PlayerName").text = playerTemplate.entityName;

        playerPortre.style.backgroundImage = new StyleBackground(playerTemplate.portrait);
        playerSprite.style.backgroundImage = new StyleBackground(playerTemplate.battleImage);
        enemySprite.style.backgroundImage = new StyleBackground(enemyData.battleImage);

        UpdateUI();

        attackBtn = root.Q<Button>("AttackButton"); 
        attackBtn.clicked += PlayerAttack;
        closeBtn = root.Q<Button>("CloseButton");
        closeBtn.clicked += EndBattle;
        defButton = root.Q<Button>("DefButton");
        defButton.clicked += () => EnemyAttacks(enemyData.attack);

        battleStateEventChannel.RaiseEvent(false);
        accumulatedEXP = enemyData.expReward;
        Debug.Log($"Kampf gegen {currentEnemyName} beginnt!");
        Debug.Log($"Erfahrungspunkte zu gewinnen: {accumulatedEXP}");
    }

    private void UpdateUI()
    {
        playerHPLabel.text = $"HP: {currentPlayerHP} / {playerRuntime.maxHP}";
        level.text = $"Lv. {playerRuntime.currentLevel}";

        float hpRatio = (float)currentPlayerHP / playerRuntime.maxHP;
        float hpPercent = Mathf.Clamp(hpRatio * 100f, 0, 100f);

        HpBarFill.style.width = new Length(hpPercent, LengthUnit.Percent);
    }

    public void PlayerAttack()
    {
        currentEnemyHP -= playerRuntime.attack;
        UpdateUI();

        if (currentEnemyHP <= 0) EndBattle();
    }
    public void EnemyAttacks(int damage)
    {
        currentPlayerHP -= damage;

        if (currentPlayerHP <= 0)
        {
            currentPlayerHP = 0;
            playerRuntime.isDead = true;
            // Handle Game Over
        }
        UpdateUI();
    }


    private void EndBattle()
    {
        currentState = BattleState.Idle;
        playerRuntime.currentHP = currentPlayerHP;

        battleHUD.gameObject.SetActive(false);
        battleStateEventChannel.RaiseEvent(true);

        closeBtn.clicked -= EndBattle;
        attackBtn.clicked -= PlayerAttack;

        BattleResult result = new BattleResult
        {
            earnedEXP = (currentEnemyHP <= 0) ? accumulatedEXP : 0,
            isVictory = currentEnemyHP <= 0
        };

        battleResultEventChannel.RaiseEvent(result);
    }
}