using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public enum MusicTransitionMode
{
    Fade,
    CrossFade,
    Wait
}

[System.Serializable]
public class MusicSetup
{
    public AudioClip clip = null;
    public float volume = 1.0f;
    public bool loop = true;
    public float fadeIn = 1.0f;
    public float fadeOut = 1.0f;
    public MusicTransitionMode transitionMode = MusicTransitionMode.CrossFade;
}

public class FAFAudio : SingletonBehaviour<FAFAudio>
{
    public AudioMixerGroup musicMixerGroup = null;

    MusicSetup currentMusicSetup = null;
    public AudioSource currentMusicSource = null;
    public AudioSource nextMusicSource = null;

    MusicSetup transitioningMusicSetup = null;
    Queue<MusicSetup> musicQueue = new Queue<MusicSetup>();

    List<AudioSource> pool = new List<AudioSource>();
    Stack<AudioSource> freeStack = new Stack<AudioSource>();

    protected override void Awake()
    {
        base.Awake();
        if(currentMusicSource == null)
        {
            currentMusicSource = CreateMusicSource("CurrentMusic");
        }
        if (nextMusicSource == null)
        {
            nextMusicSource = CreateMusicSource("NextMusic");
        }
    }

    AudioSource CreateMusicSource(string _name)
    {
        GameObject gobj = new GameObject(_name, typeof(AudioSource));
        if(gobj)
        {
            gobj.transform.parent = this.transform;
            AudioSource source = gobj.GetComponent<AudioSource>();
            source.outputAudioMixerGroup = musicMixerGroup;
            return source;
        }
        return null;
    }

    public AudioSource Play(AudioClip _clip, Vector3 _pos, float _volume = 1, float _pitch = 1, AudioMixerGroup _mixerGroup = null)
    {
        AudioSource source = null;
        if (freeStack.Count > 0)
        {
            source = freeStack.Pop();
        }
        else
        {
            GameObject gobj = new GameObject(_clip.name, typeof(AudioSource));
            if(gobj)
            {
                source = gobj.GetComponent<AudioSource>();
                pool.Add(source);
            }
        }
        if(source)
        {
            source.transform.parent = this.transform;
            source.gameObject.SetActive(true);
            source.clip = _clip;
            source.transform.position = _pos;
            source.volume = _volume;
            source.pitch = _pitch;
            source.outputAudioMixerGroup = _mixerGroup;
            source.loop = false;
            source.Play();
            StartCoroutine(FreeAudioSource(source));
        }
        return source;
    }

    IEnumerator FreeAudioSource(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        while(source && source.isPlaying)
        {
            source.loop = false;
            yield return null;
        }
        if(source)
        {
            freeStack.Push(source);
        }
    }

    public void PlayOnce2D(AudioClip _clip, Vector3 _pos, float _volume = 1, float _pitchVariation = 0)
    {
        if (_clip)
        {
            GameObject gobj = new GameObject(_clip.name);
            AudioSource source = gobj.AddComponent<AudioSource>();
            AutoDestruct autoDestruct = gobj.AddComponent<AutoDestruct>();

            gobj.transform.position = _pos;

            source.clip = _clip;
            source.volume = _volume;
            source.pitch = source.pitch + Random.Range(-_pitchVariation, _pitchVariation);
            source.Play();

            autoDestruct.delay = _clip.length;
        }
    }

    public bool TryPlayMusic(MusicSetup musicSetup, bool queue = true)
    {
        if(musicSetup == null || musicSetup == transitioningMusicSetup)
        {
            return false;
        }

        if (transitioningMusicSetup != null)
        {
            if (queue)
            {
                musicQueue.Enqueue(musicSetup);
                return true;
            }
        }

        if (nextMusicSource)
        {
            transitioningMusicSetup = null;

            StopAllCoroutines();
            StartCoroutine(TransitionMusic(musicSetup));

            return true;
        }

        return false;
    }

    void Fade(AudioSource source, float duration, float volume)
    {
        if (source)
        {
            if (Mathf.Abs(duration) > 0)
                source.volume = Mathf.Clamp(source.volume + Time.deltaTime / duration, 0, volume);
            else
                source.volume = 0;
        }
    }

    IEnumerator TransitionMusic(MusicSetup nextMusicSetup)
    {
        if (nextMusicSetup == null) yield break;

        transitioningMusicSetup = nextMusicSetup;
        nextMusicSource.Stop();
        nextMusicSource.clip = nextMusicSetup.clip;
        nextMusicSource.loop = nextMusicSetup.loop;

        switch (nextMusicSetup.transitionMode)
        {
            case MusicTransitionMode.Fade:
                nextMusicSource.volume = 0;
                nextMusicSource.PlayDelayed(nextMusicSetup.fadeOut);
                break;
            case MusicTransitionMode.CrossFade:
                nextMusicSource.volume = 0;
                nextMusicSource.Play();
                break;
            case MusicTransitionMode.Wait:
                if (currentMusicSource)
                {
                    currentMusicSource.loop = false;
                }
                nextMusicSource.volume = nextMusicSetup.volume;
                break;
            default:
                break;
        }

        bool finishedTransition = false;
        while (!finishedTransition)
        {
            switch (nextMusicSetup.transitionMode)
            {
                case MusicTransitionMode.Fade:
                    if (currentMusicSource && currentMusicSource.volume > 0)
                    {
                        Fade(currentMusicSource, -Mathf.Abs(currentMusicSetup.fadeOut), currentMusicSetup.volume);
                    }
                    else if(nextMusicSource && nextMusicSource.volume < nextMusicSetup.volume)
                    {
                        Fade(nextMusicSource, Mathf.Abs(nextMusicSetup.fadeIn), nextMusicSource.volume);
                    }
                    else
                    {
                        finishedTransition = true;
                    }
                    break;
                case MusicTransitionMode.CrossFade:
                    if (currentMusicSetup != null)
                    {
                        Fade(currentMusicSource, -Mathf.Abs(currentMusicSetup.fadeOut), currentMusicSetup.volume);
                    }
                    Fade(nextMusicSource, Mathf.Abs(nextMusicSetup.fadeIn), nextMusicSetup.volume);
                    if ((currentMusicSetup == null || !currentMusicSource || currentMusicSource.volume <= 0) && (!nextMusicSource || nextMusicSource.volume >= nextMusicSetup.volume))
                    {
                        finishedTransition = true;
                    }
                    break;
                case MusicTransitionMode.Wait:
                    if(!currentMusicSource || !currentMusicSource.isPlaying)
                    {
                        if (nextMusicSource)
                        {
                            nextMusicSource.Play();
                        }
                        finishedTransition = true;
                    }
                    break;
                default:
                    break;
            }

            yield return null; //wait a frame
        }

        //finished transition so swap the sources
        var tempSource = currentMusicSource;
        currentMusicSource = nextMusicSource;
        nextMusicSource = tempSource;

        //swap the setups
        currentMusicSetup = nextMusicSetup;
        nextMusicSetup = null;

        //rename sources
        if (currentMusicSource) currentMusicSource.gameObject.name = "CurrentMusic";
        if (nextMusicSource) nextMusicSource.gameObject.name = "NextMusic";

        //stop the old music
        if (nextMusicSource) nextMusicSource.Stop();

        transitioningMusicSetup = null;

        if (musicQueue.Count > 0)
        {
            StartCoroutine(TransitionMusic(musicQueue.Dequeue()));
        }
    }
}
