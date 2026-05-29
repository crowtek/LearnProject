using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LanguageManagerSO", menuName = "Scriptable Objects/Dialogue/LanguageManagerSO")]
public class LanguageManagerSO : ScriptableObject
{
    [Header("Available Languages")]
    [SerializeField] private List<DialogueDatabaseSO> dialogueDatabases = new List<DialogueDatabaseSO>();

    [Header("Runtime State")]
    [SerializeField] private string defaultLanguageName = "English";

    private DialogueDatabaseSO _currentDatabase;

    public event Action<DialogueDatabaseSO> LanguageChanged;

    public DialogueDatabaseSO CurrentDatabase
    {
        get
        {
            EnsureCurrentDatabase();
            return _currentDatabase;
        }
    }

    public string CurrentLanguageName => CurrentDatabase != null ? CurrentDatabase.languageName : string.Empty;

    private void OnEnable()
    {
        EnsureCurrentDatabase();
    }

    public bool SetLanguage(string languageName)
    {
        if (string.IsNullOrWhiteSpace(languageName))
        {
            Debug.LogWarning("[LanguageManager] Cannot switch to an empty language name.");
            return false;
        }

        DialogueDatabaseSO database = FindDatabase(languageName);
        if (database == null)
        {
            Debug.LogWarning($"[LanguageManager] Missing dialogue database for language: {languageName}");
            return false;
        }

        if (_currentDatabase == database)
            return true;

        _currentDatabase = database;
        LanguageChanged?.Invoke(_currentDatabase);
        return true;
    }

    public DialogueEntry GetEntry(string key)
    {
        DialogueDatabaseSO database = CurrentDatabase;
        if (database == null)
        {
            Debug.LogWarning("[LanguageManager] No dialogue database is available.");
            return default;
        }

        return database.GetEntry(key);
    }

    public string GetInteractPrompt(string npcName)
    {
        DialogueDatabaseSO database = CurrentDatabase;
        return database != null ? database.GetInteractPrompt(npcName) : npcName;
    }

    private void EnsureCurrentDatabase()
    {
        if (_currentDatabase != null)
            return;

        _currentDatabase = FindDatabase(defaultLanguageName);

        if (_currentDatabase == null && dialogueDatabases.Count > 0)
            _currentDatabase = dialogueDatabases.Find(database => database != null);
    }

    private DialogueDatabaseSO FindDatabase(string languageName)
    {
        return dialogueDatabases.Find(database =>
            database != null &&
            string.Equals(database.languageName, languageName, StringComparison.OrdinalIgnoreCase));
    }
}