using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private QuestManagerSO questManager;

    private void Awake()
    {
        if (questManager != null)
        {
            questManager.Initialize();
            Debug.Log("Quest System erfolgreich über ScriptableObject initialisiert!");
        }
    }
}