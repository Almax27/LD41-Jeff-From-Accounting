using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class FAFAudio : SingletonBehaviour<FAFAudio>
{
    AudioSource musicSource = null;
    AudioSource nextMusicSource = null;
    List<AudioSource> pool = new List<AudioSource>();
    Stack<AudioSource> freeStack = new Stack<AudioSource>();
    float fadeIn = 1;
    float fadeOut = 1;
    bool crossfade = false;

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

    public void TryPlayMusic(AudioClip _clip, float _fadeOut = 1, float _fadeIn = 1, bool _crossFade = false)
    {
        if ((musicSource && musicSource.clip == _clip) || //check current
            (nextMusicSource && nextMusicSource.clip == _clip))//check next (transition)
        {
            return;
        }
        PlayMusic(_clip, _fadeOut, _fadeIn, _crossFade);
    }

    public void PlayMusic(AudioClip _clip, float _fadeOut = 1, float _fadeIn = 1, bool _crossFade = false)
    {
        if (nextMusicSource)
            Destroy(nextMusicSource);

        GameObject gobj = new GameObject("NextMusic");
        DontDestroyOnLoad(gobj);
        nextMusicSource = gobj.AddComponent<AudioSource>();
        nextMusicSource.clip = _clip;
        nextMusicSource.loop = true;
        nextMusicSource.Play();
        nextMusicSource.volume = 0;

        fadeIn = _fadeIn;
        fadeOut = _fadeOut;
        crossfade = _crossFade;
    }
	
	// Update is called once per frame
	void Update () 
    {
	    if (nextMusicSource != null)
        {
            const float targetVolume = 0.5f;
            if(musicSource && musicSource.volume > 0 && fadeOut > 0)
            {
                musicSource.volume -= Time.deltaTime / fadeOut;
            }
            else if(nextMusicSource.volume < targetVolume && fadeIn > 0)
            {
                nextMusicSource.volume += Time.deltaTime / fadeIn;
            }
            else
            {
                if(musicSource)
                    Destroy(musicSource.gameObject);

                musicSource = nextMusicSource;
                musicSource.name = "CurrentMusic";
                musicSource.volume = targetVolume;
                nextMusicSource = null;
            }
        }
	}
}
