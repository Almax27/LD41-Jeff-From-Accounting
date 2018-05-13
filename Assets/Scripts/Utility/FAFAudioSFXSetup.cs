using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SFX_", menuName = "Audio/SFX", order = 1)]
public class FAFAudioSFXSetup : ScriptableObject {

    public List<AudioClip> Clips = new List<AudioClip>();
    public AudioMixerGroup MixerGroup = null;
    [Range(0, 1)] public float Volume = 1.0f;
    [Range(0, 1)] public float VolumeVariance = 0.0f;
    [Range(0, 1)] public float Pitch = 1.0f;
    [Range(0, 1)] public float PitchVariance = 0.0f;
    [Range(0, 1)] public float SpatialBlend = 0.0f;

    public AudioSource Play(Vector3 _pos, float _volumeScalar = 1.0f, float _pitchScalar = 1.0f, float _pitchOffset = 1.0f)
    {
        AudioSource source = null;
        if (Clips != null && Clips.Count > 0)
        {
            float volume = (Volume  * _volumeScalar) + Random.Range(-VolumeVariance, VolumeVariance);
            float pitch = (Pitch * _pitchScalar) + _pitchOffset + Random.Range(-PitchVariance, PitchVariance);
            source = FAFAudio.Instance.Play(Clips[Random.Range(0, Clips.Count - 1)], _pos, volume, pitch, MixerGroup);
            if(source)
            {
                source.spatialBlend = SpatialBlend;
                source.minDistance = 10;
            }
        }
        return source;
    }
}
