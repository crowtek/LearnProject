using System.IO;
using UnityEngine;

public class JsonStorySaveHandler : ISaveHandler
{
    public bool SaveExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public void Save(StoryProgressSaveData saveData, string filePath)
    {
        EnsureDirectoryExists(filePath);
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, json);
    }

    public StoryProgressSaveData Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new StoryProgressSaveData();
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<StoryProgressSaveData>(json) ?? new StoryProgressSaveData();
    }

    public void Delete(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}