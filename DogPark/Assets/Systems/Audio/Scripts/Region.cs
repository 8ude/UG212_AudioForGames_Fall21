using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    public Collider[] subregions;

    public bool drawDebug = false;

    void OnDrawGizmos()
    {
        // if (drawDebug) {
        //     // Draw a magenta box at half the max distance from the collider
        //     // (only works for BoxColliders at the moment)
        //     float distance = GetComponent<AudioSource>().maxDistance;

        //     Gizmos.color = Color.magenta;
        //     foreach (Collider collider in subregions) {
        //         Gizmos.matrix = collider.transform.localToWorldMatrix;
        //         var boxCollider = collider as BoxCollider;

        //         Vector3 extension = new Vector3(
        //             distance/collider.transform.lossyScale.x,
        //             distance/collider.transform.lossyScale.y,
        //             distance/collider.transform.lossyScale.z
        //         );

        //         if (boxCollider) {
        //             Gizmos.DrawWireCube(
        //                 boxCollider.center,
        //                 boxCollider.size + extension
        //             );
        //         }
        //     }
        // }
    }

    public float DistanceToPoint(Vector3 listenerPosition, out Vector3 closestPoint) {
        float distance = Mathf.Infinity;
        closestPoint = Vector3.zero;
        foreach (Collider subregion in subregions) {
            Vector3 point = subregion.ClosestPoint(listenerPosition);
            float subregionDistance = Vector3.Distance(listenerPosition, point);
            if (subregionDistance < distance) {
                distance = subregionDistance;
                closestPoint = point;
            }
            if (distance < 0.0001) break;
        }
        return distance;
    }

    // Return listener's distance from this region (= min distance over all subregions)
    public float DistanceToPoint(Vector3 listenerPosition) {
        Vector3 temp;
        return DistanceToPoint(listenerPosition, out temp);
    }
}
