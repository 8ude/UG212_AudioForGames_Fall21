using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public float volume = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        AudioListener.volume = volume;
    }
}
