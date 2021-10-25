using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using System.Linq;


// If the player is inside a zone, only the audio for that zone will be played.
// Otherwise, set the volume of each audio zone based on the player proximity to that zone.
public class SpatialAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class FmodAudioRegion {
        public Region audioRegion;
        public string fmodParameter;
    }
    public List<FmodAudioRegion> audioRegions; // list of audio regions with associated fmod param

    private const float _fmodScaleFactor = 0.1449275362f; // don't ask

    public float sharpness = 5f; // How sharp the transitions between regions are.

    public GameObjectReference listener = null;
    private AudioSource _audio;

    public FMODUnity.StudioEventEmitter fmodEmitter;

    public float[] debugAudioParams;

    // Start is called before the first frame update
    void Start()
    {
        debugAudioParams = new float[audioRegions.Count];
    }

    // Update is called once per frame
    void Update()
    {
        if (!listener.Value) return;

        int n = audioRegions.Count;
        List<float> volumes = new List<float>(n);

        Vector3 listenerPos = listener.Value.transform.position;
        for (int i = 0; i < n; i++) {
            float distance = audioRegions[i].audioRegion.DistanceToPoint(listenerPos);
            float volume = Mathf.Pow(1f/(0.0001f + distance), sharpness); // hacky but works out
            volumes.Add(volume);
        }

        float volumeSum = volumes.Sum();
        for (int i = 0; i < n; i++) {
            float volume = volumes[i]/volumeSum; // normalize so that sum of volumes is 1.0

            // set the FMOD param for each region:
            fmodEmitter.SetParameter(audioRegions[i].fmodParameter, scaleVolumeForFmod(volume));
            
            debugAudioParams[i] = scaleVolumeForFmod(volume);
        }
    }

    // Adjust volume param to match Fmod's logarithmic scale,
    // So that volumes always sum to 0 dB.
    float scaleVolumeForFmod(float v) {
        return Mathf.Pow(v, _fmodScaleFactor);
    }
}
