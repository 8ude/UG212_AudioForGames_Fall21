using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using Mirror;

public class AudioSyncReference : NetworkBehaviour
{
    public float time = 0;
    public FloatVariable SyncTime;

    // Start is called before the first frame update
    void Start()
    {
        time = (float)NetworkTime.time;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        SyncTime.Value = time;
    }
}
