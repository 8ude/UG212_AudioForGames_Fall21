using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

// This currently just manages tonality changes in time, 
// but it should manage changes in space as well.
public class TonalityManager : MonoBehaviour
{
    [System.Serializable]
    public class TonalSection {
        public float length; // in units
        public string tonality;
    }

    public List<TonalSection> tonalSections;
    public float unitLengthSecs = 1f; // seconds

    //public AudioSyncReference syncReference;
    public FloatReference SyncTime;
    public StringVariable Tonality;
    public AudioClip referenceClip; // should be the longest of all loops

    public void Start () {

    }

    public void Update () {
        // Debug.Log(GetTonality());
        Tonality.Value = GetTonality();
    }

    public string GetTonality () {
        float t = Mathf.Repeat(SyncTime.Value, referenceClip.length);
        float s = 0;
        string tonality = null;
        foreach (TonalSection ts in tonalSections) {
            tonality = ts.tonality;
            s += ts.length*unitLengthSecs;
            if (s >= t) break;
        }
        // (assume final tonal section lasts forever)
        return tonality;
    }
}