using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public class WaterFlow: MonoBehaviour {
    // -- fields --
    [SerializeField]
    [Tooltip("The magnitude of the force this flow applies to items.")]
    private FloatReference fFlowMag;

    [SerializeField]
    [Tooltip("An optional point that objects clump at.")]
    private Transform fClumpPoint;

    [SerializeField]
    [Tooltip("The distance at which the clump's attraction starts to fall off.")]
    private FloatReference fClumpThreshold;

    // -- props --
    // the objects caught in this flow
    private readonly HashSet<WaterCatch> mItems = new HashSet<WaterCatch>();
    // the correctly scaled flow force
    private Vector3 mFlow;

    // -- lifecycle --
    protected void Awake() {
        mFlow = transform.forward * fFlowMag;
    }

    protected void FixedUpdate() {
        Flow();
    }

    // -- commands --
    public void Enter(WaterCatch item) {
        mItems.Add(item);
        item.EnterFlow();
    }

    private void Flow() {
        foreach (var item in mItems) {
            // TODO: properly remove destroyed ojnects from list
            if(item == null) continue;
            // don't clump if the item was thrown into this flow
            if (fClumpPoint && !item.IsInInitialFlow()) {
                FlowTowardsClump(item);
            } else {
                FlowDownstream(item);
            }
        }
    }

    private void FlowDownstream(WaterCatch item) {
        item.AddForce(mFlow);
    }

    private void FlowTowardsClump(WaterCatch item) {
        // get direction to clump
        var dist = fClumpPoint.position - item.Position;
        var dir = Vector3.Normalize(dist);

        // back off as object gets closer
        var scale = Mathf.InverseLerp(0, fClumpThreshold, Vector3.Magnitude(dist));

        // add a flow force towards the clump
        item.AddForce(scale * fFlowMag * dir);
    }

    public void Exit(WaterCatch item) {
        mItems.Remove(item);
        item.ExitFlow();
    }
}
