using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class PlaySound : MonoBehaviour
{
    public AudioSource dialogueSource, uiSource, inGameSFXSource;
    public AudioMixerSnapshot mutedSnapshot, originalSnapshot;
    public AudioMixer mainMixer;
    private void Start()
    {
        mainMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            dialogueSource.Play();
        }

        if(Input.GetKeyDown(KeyCode.U))
        {
            uiSource.Play();
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            inGameSFXSource.Play();
        }

        if(Input.GetKeyDown(KeyCode.M))
        {
            mutedSnapshot.TransitionTo(1f);
        }

        if(Input.GetKeyDown(KeyCode.N))
        {
            originalSnapshot.TransitionTo(1f);
        }

        //mainMixer.TransitionToSnapshots(new AudioMixerSnapshot[] { mutedSnapshot, originalSnapshot }, new float[] { 0.5f, 0.5f }, )

    }
}
