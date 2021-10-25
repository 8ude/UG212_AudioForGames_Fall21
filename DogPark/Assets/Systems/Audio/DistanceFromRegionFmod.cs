using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceFromRegionFmod : MonoBehaviour
{
    public float maxDistance;
    public Region audioRegion;

    public string fmodParam;
    public Transform listener;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float intensity = Mathf.InverseLerp(maxDistance, 0, audioRegion.DistanceToPoint(listener.position));
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(fmodParam, intensity);
    }
}
