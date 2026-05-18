using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioConfig_", menuName = "Scriptable Objects/Audio/Audio Configuration")]
public class AudioConfigurationSO : ScriptableObject
{
    public AudioClip clip;
    public AudioMixerGroup outputGroup;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(.1f, 3f)] public float pitch = 1f;

    [Header("Engine Polish")]
    public bool loop = false;
    public bool useRandomPitch = false;
    [Range(0f, 0.3f)] public float pitchRandomness = 0.05f;

    public void ApplyTo(AudioSource source)
    {
        source.clip = clip;
        source.outputAudioMixerGroup = outputGroup;
        source.volume = volume;
        source.loop = loop;
        source.pitch = useRandomPitch
            ? pitch + Random.Range(-pitchRandomness, pitchRandomness)
            : pitch;
    }
}