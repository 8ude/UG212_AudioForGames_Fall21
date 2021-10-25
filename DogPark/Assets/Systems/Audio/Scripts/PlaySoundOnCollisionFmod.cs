using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using System.Linq;

public class PlaySoundOnCollisionFmod : MonoBehaviour
{
    public float minVelocity = 0.1f;
    public float maxVelocity = 2f;

    public float debounceTime = 0.4f;
    private float _lastCollision = 0f;

    [SerializeField] private LayerMask layerMask = ~0;

    private bool _inTrigger = false;

    public bool runOnCollisionStay = false;
    public bool runOnTrigger = true;

    public FMODUnity.StudioEventEmitter _fmodEmitter;
    private const string _volumeParam = "ImpactVelocity";
    private const string _ballTypeParam = "BallType";
    public int ballType; // unused for feet sounds

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
        float factor = Mathf.InverseLerp(minVelocity, maxVelocity, contactVelocity);
        PlaySound(factor);
    }

    void PlaySound(float volumeFactor) {
        _fmodEmitter.Play();
        _fmodEmitter.SetParameter(_volumeParam, volumeFactor);
        _fmodEmitter.SetParameter(_ballTypeParam, ballType);
    }
}
