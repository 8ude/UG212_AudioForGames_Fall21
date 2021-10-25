using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public class SyncAudio : MonoBehaviour
{
    public FloatReference SyncTime;
	public float driftThreshold = 0.05f; // seconds

	public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float target = Mathf.Repeat(SyncTime.Value, audioSource.clip.length);
        if (Mathf.Abs(audioSource.time - target) > driftThreshold) {
        	audioSource.time = target;
        }
    }
}
