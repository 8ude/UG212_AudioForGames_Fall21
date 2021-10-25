using System;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using A = UnityEngine.Assertions.Assert;

[RequireComponent(typeof(Collider))]
public class CollisionProxy: MonoBehaviour {
    // -- fields --
    [SerializeField]
    [Tooltip("The target to send collision events to. Must implement CollisionTarget.")]
    private GameObjectReference fTarget;

    // -- props --
    private Collider mCollider;
    private CollisionTarget.Any[] mTargets;

    // -- lifecycle --
    protected void Start() {
        mCollider = GetComponent<Collider>();
        mTargets = fTarget.Value.GetComponents<CollisionTarget.Any>();
        A.AreNotEqual(mTargets.Length, 0, "CollisionProxy - must have at least one target");
    }

    // -- events --
    private void OnCollisionEnter(Collision collision) {
        foreach (var target in mTargets) {
            if (target is CollisionTarget.Enter t) {
                t.OnCollisionEnter(collision);
            }
        }
    }

    private void OnCollisionStay(Collision collision) {
        foreach (var target in mTargets) {
            if (target is CollisionTarget.Stay t) {
                t.OnCollisionStay(collision);
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        foreach (var target in mTargets) {
            if (target is CollisionTarget.Exit t) {
                t.OnCollisionExit(collision);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        foreach (var target in mTargets) {
            if (target is CollisionTarget.TriggerEnter t) {
                t.OnTriggerSourceEnter(mCollider, other);
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        foreach (var target in mTargets) {
            if (target is CollisionTarget.TriggerStay t) {
                t.OnTriggerSourceStay(mCollider, other);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        foreach (var target in mTargets) {
            if (target is CollisionTarget.TriggerExit t) {
                t.OnTriggerSourceExit(mCollider, other);
            }
        }
    }
}
