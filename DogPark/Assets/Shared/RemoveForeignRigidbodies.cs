using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RemoveForeignRigidbodies : NetworkBehaviour
{
    [SyncVar] [SerializeField] private GameObject mOwner;
    public override void OnStartClient() {
        base.OnStartClient();

        var owner = mOwner.GetComponent<NetworkIdentity>();
        if (!owner.isLocalPlayer) {
            RemoveRigidbodiesInChildren();
        }
    }

    public void SetOwner(GameObject owner) {
        mOwner = owner;
    }

    private void RemoveRigidbodiesInChildren() {
        foreach (var j in GetComponentsInChildren<Joint>()) {
            if (j.gameObject != gameObject) {
                Destroy(j);
            }
        }
        foreach (var rb in GetComponentsInChildren<Rigidbody>()) {
            if (rb.gameObject != gameObject) {
                rb.isKinematic = true;
            }
        }
    }
}
