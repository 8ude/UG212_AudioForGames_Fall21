using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityEngine;

// Force the attached transform to track the target transform but stay inside a region
// E.g. this transform is the closest point in the river to the player
public class InsideRegionConstraint : MonoBehaviour
{
    public GameObjectReference target;
    public Region region;

    // Update is called once per frame
    void Update()
    {
        if (target.Value != null) {
            Vector3 pos;
            float distance = region.DistanceToPoint(target.Value.transform.position, out pos);
            this.transform.position = pos;
        }
    }
}
