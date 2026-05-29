public interface ISaveHandler
{
    bool SaveExists(string filePath);
    void Save(StoryProgressSaveData saveData, string filePath);
    StoryProgressSaveData Load(string filePath);
    void Delete(string filePath);
}