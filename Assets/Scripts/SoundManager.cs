using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundManager : MonoBehaviour
{
    public bool mute;
    public bool muteEffects;

    public AudioSource audioSourceSounds;
    public AudioSource audioSourceMusic;

    public AudioClip fillSound;
    public AudioClip failSound;
    public AudioClip wonSound;
    public AudioClip moveSound;
    public AudioClip moveFailSound;
    public AudioClip undoSound;

    public bool noStartAnim;

    private void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("SoundManager").Length != 1 &&
            GameObject.FindGameObjectsWithTag("SoundManager") != null)
        {
            Destroy(gameObject);
        }
        else
        {
            if (!mute)
            {
                audioSourceMusic.Play();
            }

            DontDestroyOnLoad(this);
        }
    }

    public void PlayFillSound()
    {
        if (!muteEffects)
        {
            audioSourceSounds.PlayOneShot(fillSound);
        }
    }

    public void PlayUndoSound()
    {
        if (!muteEffects)
        {
            audioSourceSounds.PlayOneShot(undoSound);
        }
    }
    
    public void PlayFailSound()
    {
        if (!muteEffects)
        {
            audioSourceSounds.PlayOneShot(failSound);
        }
    }

    public void PlayWonSound()
    {
        if (!muteEffects)
        {
            audioSourceSounds.PlayOneShot(wonSound);
        }
    }

    public void PlayMoveSound()
    {
        if (!muteEffects)
        {
            audioSourceSounds.PlayOneShot(moveSound);
        }
    }

    public void PlayMoveFailSound()
    {
        if (!muteEffects)
        {
            audioSourceSounds.PlayOneShot(moveFailSound);
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mute = !mute;

            if (audioSourceMusic.isPlaying)
            {
                audioSourceMusic.Stop();
            }
            else
            {
                audioSourceMusic.Play();
            }
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            muteEffects = !muteEffects;
        }
    }
}