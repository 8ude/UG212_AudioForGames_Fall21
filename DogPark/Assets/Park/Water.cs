using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public class Water: MonoBehaviour, CollisionTarget.TriggerEnterAndExit {
    // -- fields --
    [SerializeField]
    [Tooltip("Density (buoyancy) of the liquid.")]
    private FloatReference fDensity;

    [SerializeField]
    [Tooltip("Additional drag imposed by the river.")]
    private FloatReference fDrag;

    // -- props --
    // the map of items caught in the river
    private readonly Dictionary<int, WaterCatch> mItems = new Dictionary<int, WaterCatch>();

    // -- lifecycle --
    protected void FixedUpdate() {
        foreach (var item in mItems.Values) {
            // TODO: properly remove destroyed ojnects from list
            if(item == null) continue;
            AddBuoyancy(item);
        }
    }

    // -- commands --
    private void EnterWater(WaterCatch item) {
        item.OffsetDrag(fDrag);
    }

    private void EnterFlow(WaterFlow flow, Collider other) {
        var item = FindCatch(other);

        // if one does not exist, add it to the river
        if (item == null) {
            item = AddCatch(other);

            // if we couldn't add a catch, this item can't float
            if (item == null) {
                return;
            }

            EnterWater(item);
        }

        // add the item to the flow
        flow.Enter(item);
    }

    private void AddBuoyancy(WaterCatch item) {
        // only apply buoyancy if the target is mostly below the waterline
        var depth = Mathf.Max(transform.position.y - item.Position.y, 0.0f);

        // strictly speaking, buoyancy is proportional to the submersed volume,
        // but we use depth as an approximation
        // TODO: use object volume * depth as a better approximation?
        item.AddForce(depth * -fDensity * Physics.gravity);
    }

    private void ExitFlow(WaterFlow flow, Collider other) {
        var item = FindCatch(other);
        if (item == null) {
            return;
        }

        // release the item from the flow
        flow.Exit(item);

        // if this item has exited every flow, remove it from the water
        if (!item.IsInFlow()) {
            ExitWater(item);
            RemoveCatch(other);
        }
    }

    private void ExitWater(WaterCatch item) {
        item.ResetDrag();
    }

    private WaterCatch AddCatch(Collider other) {
        // ensure we have at least one rigidbody
        var rigidbodies = other.GetComponentsInChildren<Rigidbody>();
        if (rigidbodies.Length == 0) {
            return null;
        }

        // then create the catch and store it
        var item = new WaterCatch(rigidbodies);
        mItems[other.GetHashCode()] = item;

        return item;
    }

    private void RemoveCatch(Collider other) {
        var id = other.GetHashCode();
        mItems.Remove(id);
    }

    // -- queries --
    private WaterCatch FindCatch(Collider other) {
        return mItems.Get(other.GetHashCode());
    }

    // -- CollisionTarget --
    public void OnTriggerSourceEnter(Collider source, Collider other) {
        EnterFlow(source.GetComponentInParent<WaterFlow>(), other);
    }

    public void OnTriggerSourceExit(Collider source, Collider other) {
        ExitFlow(source.GetComponentInParent<WaterFlow>(), other);
    }
}
