using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnCollision : MonoBehaviour
{

    public AudioSource audioSource;

    [SerializeField]
    public StringToListOfAudioClips clipsByTonality;

    [SerializeField]
    public StringReference Tonality;

    public float minVelocity = 0.1f;
    public float maxVelocity = 2f;

    public float minVolume = 0f;
    public float maxVolume = 1f;

    public float debounceTime = 0.4f;
    private float _lastCollision = 0f;

    public enum SelectionMethod{Sequential, Random, RandomBag};
    public SelectionMethod selectionMethod;

    [SerializeField] private LayerMask layerMask = ~0;

    private int _clipIndex = 0;

    private bool _inTrigger = false;

    public bool runOnCollisionStay = false;
    public bool runOnTrigger = true;

    public float readonlyCollisionVelocity;
    public float readonlyVolume;

    // Start is called before the first frame update
    void Start()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!this.enabled) return;
        if (!IsInLayerMask(collision.gameObject)) return;

        ProcessCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        if (runOnCollisionStay) {
            OnCollisionEnter(collision);
        }
    }

    void OnTriggerEnter(Collider collider) {
        if (!this.enabled) return;
        if (!IsInLayerMask(collider.gameObject)) return;
        if (!this.runOnTrigger) return;

        // Check _inTrigger to prevent multiple triggerEnter events for a single entrance
        if (runOnCollisionStay || !_inTrigger) {
            _inTrigger = true;

            // Seems like we can't adjust to velocity when using triggers, so we just play the sound at max volume
            PlaySound(1f);
        }
    }
    void OnTriggerStay(Collider collider) {
        if (runOnCollisionStay) {
            OnTriggerEnter(collider);
        }
    }

    void OnTriggerExit(Collider collider) {
        _inTrigger = false;
    }

    private bool IsInLayerMask(GameObject obj) {
        // Check collider matches layerMask
        return (layerMask == (layerMask | (1 << obj.layer)));
    }

    void ProcessCollision(Collision collision)
    {
        float contactVelocity = 0f;
        // Take the greatest velocity (normal to collision) from all contact points
        // if (collision.contacts.Length > 1) Debug.Log(collision.contacts.Length);
        for (int i = 0; i < collision.contacts.Length; i++) {
            float contactNormalVelocity =  Vector3.Project(collision.relativeVelocity, collision.contacts[i].normal).magnitude;
            if (contactVelocity < contactNormalVelocity) contactVelocity = contactNormalVelocity;
        }

        float now = Time.time;
        if (contactVelocity < minVelocity || now - _lastCollision < debounceTime) {
            return;
        }
        _lastCollision = now;

        // Adjust volume based on strength of impact
        readonlyCollisionVelocity = contactVelocity;
        float factor = Mathf.InverseLerp(minVelocity, maxVelocity, contactVelocity);
        PlaySound(factor);

        // Debug.Log("velocity "+contactVelocity);
        // Debug.Log("SOUND "+factor);
    }

    private void PlaySound(float volumeFactor) {
        // Debug.Log(gameObject.name);
        // TODO: refactor this out
        AudioClip clip;
        string tonality = Tonality.Value;
        if (clipsByTonality.ContainsKey(tonality)) {
            //
        } else if (clipsByTonality.ContainsKey("default")) {
            //Debug.LogWarning("clipsByTonality contains no clips for '"+tonality+"', using default");
            tonality = "default";
        } else {
           //Debug.LogError("clipsByTonality contains no clips for '"+tonality+"' and none for 'default'");
            return;
        }
        List<AudioClip> clips = clipsByTonality[tonality];

        switch (selectionMethod) {
            case SelectionMethod.Sequential:
                _clipIndex = (_clipIndex+1)%clips.Count;
                clip = clips[_clipIndex];
                break;
            case SelectionMethod.Random:
                clip = clips[Random.Range(0,clips.Count)];
                break;
            case SelectionMethod.RandomBag:
            default:
                if (_clipIndex == 0) {
                    clips.Shuffle();
                }
                _clipIndex = (_clipIndex+1)%clips.Count;
                clip = clips[_clipIndex];
                break;
        }

        // Debug.Log(clip.name);
        float volume = Mathf.Lerp(minVolume, maxVolume, volumeFactor);
        readonlyVolume = volume;
        audioSource.PlayOneShot(clip, volume);
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = Random.Range(0, n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}
