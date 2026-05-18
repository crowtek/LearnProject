using UnityEngine;
using Cysharp.Threading.Tasks;

public class AudioManager : MonoBehaviour
{
    [Header("Listening Channels")]
    [SerializeField] private AudioEventChannelSO musicRequestChannel;
    [SerializeField] private AudioEventChannelSO sfxRequestChannel;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioConfigurationSO BGM;

    private AudioSource activeMusicSource;
    private const float FADE_DURATION = 0.5f;

    private void Start()
    {
        if (musicSourceA == null || musicSourceB == null || sfxSource == null)
        {
            Debug.LogError("AudioManager: One or more AudioSources are not assigned.");
        }
        PlayMusic(BGM);
    }

    private void OnEnable()
    {
        if (musicRequestChannel != null) musicRequestChannel.OnEventRaised += PlayMusic;
        if (sfxRequestChannel != null) sfxRequestChannel.OnEventRaised += PlaySFX;

        activeMusicSource = musicSourceA;
    }

    private void OnDisable()
    {
        if (musicRequestChannel != null) musicRequestChannel.OnEventRaised -= PlayMusic;
        if (sfxRequestChannel != null) sfxRequestChannel.OnEventRaised -= PlaySFX;
    }

    private void PlayMusic(AudioConfigurationSO config)
    {
        if (config == null || (activeMusicSource.isPlaying && activeMusicSource.clip == config.clip)) return;

        // Bestimme die inaktive Source für das Crossfade
        AudioSource targetSource = (activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;

        config.ApplyTo(targetSource);

        // Starte den fließenden Übergang (Crossfade) via UniTask
        CrossfadeMusic(activeMusicSource, targetSource, FADE_DURATION).Forget();

        activeMusicSource = targetSource;
    }

    private void PlaySFX(AudioConfigurationSO config)
    {
        if (config == null) return;

        sfxSource.pitch = config.useRandomPitch
            ? config.pitch + Random.Range(-config.pitchRandomness, config.pitchRandomness)
            : config.pitch;

        sfxSource.PlayOneShot(config.clip, config.volume);
    }

    private async UniTaskVoid CrossfadeMusic(AudioSource fadeOutSource, AudioSource fadeInSource, float duration)
    {
        float targetVolume = fadeInSource.volume;
        float startVolume = fadeOutSource.volume;

        fadeInSource.volume = 0f;
        fadeInSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;

            fadeOutSource.volume = Mathf.Lerp(startVolume, 0f, normalizedTime);
            fadeInSource.volume = Mathf.Lerp(0f, targetVolume, normalizedTime);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        fadeOutSource.Stop();
        fadeOutSource.volume = startVolume;
    }
}