using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class SceneMovementManager : MonoBehaviour
{
    [Header("Listening Channel")]
    [SerializeField] private SceneTransitionEventChannelSO transitionChannel;

    private string pendingSpawnName;

    private void OnEnable()
    {
        if (transitionChannel != null)
            transitionChannel.OnTransitionRequested += HandleTransitionRequest;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (transitionChannel != null)
            transitionChannel.OnTransitionRequested -= HandleTransitionRequest;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HandleTransitionRequest(string sceneName, string spawnPointTag)
    {
        pendingSpawnName = spawnPointTag; 
        LoadSceneAsync(sceneName).Forget();
    }

    private async UniTaskVoid LoadSceneAsync(string sceneName)
    {
        // Hier kommt später UI-Fade-Out hin

        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            await UniTask.Yield();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(pendingSpawnName)) return;

        FindAndTeleport().Forget();
    }

    private async UniTaskVoid FindAndTeleport()
    {
        await UniTask.Yield(PlayerLoopTiming.Update);

        GameObject spawnPoint = GameObject.Find(pendingSpawnName);

        if (spawnPoint != null)
        {
            TeleportPlayer(spawnPoint.transform);
        }
        else
        {
            Debug.LogError($"SceneMovementManager: SpawnPoint mit dem NAMEN '{pendingSpawnName}' wurde in der geladenen Szene nicht gefunden!");
        }

        pendingSpawnName = null;
    }

    private void TeleportPlayer(Transform targetTransform)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("SceneMovementManager: Kein GameObject mit dem Tag 'Player' in der Szene gefunden!");
            return;
        }

        if (player.TryGetComponent(out CharacterController cc))
        {
            cc.enabled = false;
            player.transform.position = targetTransform.position;
            player.transform.rotation = targetTransform.rotation;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = targetTransform.position;
            player.transform.rotation = targetTransform.rotation;
        }

        // Hier kommt später dein UI-Fade-In hin
        Debug.Log($"Spieler erfolgreich zu '{targetTransform.name}' teleportiert.");
    }
}