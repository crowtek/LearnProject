using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public enum BattleState { Idle, Start, PlayerTurn, EnemyTurn, Busy, Won, Lost, End }

    [Header("UI Ref")]
    [SerializeField] private BattleUIController uiController;
    private Camera overworldCamera;

    [Header("Bridge Data")]
    [SerializeField] private BattleTransferDataSO battleTransferData;

    [Header("3D Spawn Points in the Arena")]
    [SerializeField] private Transform player3DSpawnPoint;
    [SerializeField] private Transform enemy3DSpawnPoint;

    [Header("Player Ref")]
    [SerializeField] private PlayerRuntimeState playerRuntime;

    [Header("Broadcasting Channels")]
    [SerializeField] private BattleResultEventChannelSO battleResultEventChannel;
    [SerializeField] private BoolEventChannelSO battleStateEventChannel;

    [Header("Audio Channels")]
    [SerializeField] private AudioEventChannelSO musicChannel;
    [SerializeField] private AudioEventChannelSO sfxChannel;

    [Header("Audio files to play")]
    [SerializeField] private AudioConfigurationSO battleStartSFX;
    [SerializeField] private AudioConfigurationSO battleMusicBGM;
    [SerializeField] private AudioConfigurationSO attackSFX;
    [SerializeField] private AudioConfigurationSO mapBGM;
    [SerializeField] private AudioConfigurationSO battleWonBGM;

    private BattleEntityData currentEnemy;
    private int currentPlayerHP;
    private int currentEnemyHP;
    private int accumulatedEXP;
    private BattleState currentState = BattleState.Idle;

    private GameObject spawnedPlayerGO;
    private GameObject spawnedEnemyGO;

    void Awake()
    {
        uiController.Initialize();
        if (uiController.SkillDistro != null)
        {
            uiController.SkillDistro.SetupDynamicWeaponRows(playerRuntime);
        }
    }

    void Start()
    {
        overworldCamera = Camera.main;
        if (overworldCamera != null)
        {
            overworldCamera.gameObject.SetActive(false);
        }

        if (battleTransferData == null || battleTransferData.currentEnemyData == null)
        {
            Debug.LogError("Keine Kampfdaten für die 3D-Szene gefunden!");
            return;
        }

        currentEnemy = battleTransferData.currentEnemyData;
        currentPlayerHP = playerRuntime.currentHP;
        currentEnemyHP = currentEnemy.maxHP;
        accumulatedEXP = currentEnemy.expReward;

        // Play battle start and BGM audio
        if (sfxChannel != null) sfxChannel.RaiseEvent(battleStartSFX);
        if (musicChannel != null) musicChannel.RaiseEvent(battleMusicBGM);

        Spawn3DCombatants();

        uiController.ShowBattleUI(true);
        uiController.BindButtons(PlayerAttack, PlayerDefend, () => EndBattle(false));

        UpdateGameState(); // Init UI Update at start
        currentState = BattleState.PlayerTurn;
    }

    private void Spawn3DCombatants()
    {
        if (playerRuntime.playerCombatPrefab != null && player3DSpawnPoint != null)
        {
            spawnedPlayerGO = Instantiate(playerRuntime.playerCombatPrefab, player3DSpawnPoint.position, player3DSpawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning("Player Combat Prefab oder Spawn Point nicht zugewiesen!");
        }

        if (currentEnemy.combatPrefab != null && enemy3DSpawnPoint != null)
        {
            spawnedEnemyGO = Instantiate(currentEnemy.combatPrefab, enemy3DSpawnPoint.position, enemy3DSpawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning("Enemy Combat Prefab oder Spawn Point nicht zugewiesen!");
        }
    }

    private void UpdateGameState() // UI-Updates
    {
        uiController.UpdateStats(currentPlayerHP, playerRuntime.maxHP, playerRuntime.currentLevel);
    }

    public void PlayerAttack()
    {
        if (currentState != BattleState.PlayerTurn) return;
        currentState = BattleState.Busy;

        if (sfxChannel != null) sfxChannel.RaiseEvent(attackSFX);

        int damage = Mathf.Max(1, playerRuntime.attack - (currentEnemy.defense / 2));
        currentEnemyHP -= damage;

        uiController.ShowMonsterDamage(damage);

        // Check if enemy is dead 
        if (currentEnemyHP <= 0)
        {
            DetermineBattleOutcome();
        }
        else
        {
            Invoke(nameof(EnemyTurn), 1.5f);
        }
    }

    public void PlayerDefend()
    {
        if (currentState != BattleState.PlayerTurn) return;
        currentState = BattleState.Busy;
        // Defend logic
        EnemyTurn();
    }

    private void EnemyTurn()
    {
        currentState = BattleState.EnemyTurn;

        int damage = Mathf.Max(1, currentEnemy.attack - (playerRuntime.defense / 2));
        currentPlayerHP -= damage;

        // Verhindern, dass HP unter 0 fallen (sieht in der UI sonst komisch aus)
        currentPlayerHP = Mathf.Max(0, currentPlayerHP);

        uiController.ShowPlayerDamage(damage);

        UpdateGameState();

        if (currentPlayerHP <= 0)
        {
            DetermineBattleOutcome();
        }
        else
        {
            currentState = BattleState.PlayerTurn;
        }
    }

    private void DetermineBattleOutcome()
    {
        if (currentEnemyHP <= 0)
        {
            if (musicChannel != null) musicChannel.RaiseEvent(battleWonBGM);
            currentState = BattleState.Won;
            string msg = $"{currentEnemy.entityName} wurde besiegt!\r\nDu erhältst {accumulatedEXP} EXP.";

            if (playerRuntime.currentEXP + accumulatedEXP >= playerRuntime.expToNextLevel)
            {
                msg += "\r\n\r\nLEVEL UP!";
                uiController.ShowVictoryScreen(msg, EvaluatePostBattleSkillFlow);
            }
            else
            {
                uiController.ShowVictoryScreen(msg, () => EndBattle(true));
            }
        }
        else if (currentPlayerHP <= 0)
        {
            currentState = BattleState.Lost;
            uiController.ShowVictoryScreen("Du wurdest kampfunfähig...", () => EndBattle(false));
        }
    }

    private void EvaluatePostBattleSkillFlow()
    {
        if (playerRuntime.unspentSkillPoints > 0)
        {
            uiController.SkillDistro.OpenSkillDistribution(playerRuntime, () =>
            {
                uiController.SkillDistro.ApplyAllocatedPoints(playerRuntime);
                EndBattle(true);
            });
        }
        else
        {
            EndBattle(true);
        }
    }

    public void EndBattle(bool isVictory)
    {
        uiController.UnbindButtons(PlayerAttack, PlayerDefend, null);
        uiController.ShowBattleUI(false);

        playerRuntime.currentHP = currentPlayerHP;

        battleResultEventChannel.RaiseEvent(new BattleResult
        {
            earnedEXP = isVictory ? accumulatedEXP : 0,
            isVictory = isVictory
        });

        battleTransferData.Clear();
        battleStateEventChannel.RaiseEvent(true);
        if (musicChannel != null) musicChannel.RaiseEvent(mapBGM);

        if (spawnedPlayerGO != null) Destroy(spawnedPlayerGO);
        if (spawnedEnemyGO != null) Destroy(spawnedEnemyGO);

        // Show workd camera again
        if (overworldCamera != null)
        {
            overworldCamera.gameObject.SetActive(true);
        }

        SceneManager.UnloadSceneAsync("BattleScene");
    }
}