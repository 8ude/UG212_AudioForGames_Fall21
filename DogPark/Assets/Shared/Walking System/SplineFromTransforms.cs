using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Spline))]
[ExecuteInEditMode]
public class SplineFromTransforms : MonoBehaviour
{
    private Spline spline;
    public List<Transform> nodes;

    // If false, set the rotation of each spline node based on the rotation of the corresponding transform node.
    // if true, set the rotation to interpolate smoothly based on the position of the previous and next nodes in the chain.
    public bool forceSmooth = false;

    private List<Transform> _prevNodes;

    private bool _forceUpdate;

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        foreach (Transform node in nodes ) {
            if (Selection.Contains(node.gameObject)) {
                //Gizmos.matrix = node.localToWorldMatrix;
                Gizmos.DrawSphere(node.position, 0.1f);

                Vector3 topHandle = node.TransformPoint(Vector3.up);
                Vector3 bottomHandle = node.TransformPoint(Vector3.down);
                Gizmos.DrawLine(topHandle, bottomHandle);
                Gizmos.DrawSphere(topHandle, 0.05f);
                Gizmos.DrawSphere(bottomHandle, 0.05f);
            }
        }
    }
#endif

    // Start is called before the first frame update
    void Start()
    {
        _forceUpdate = true;
        spline = GetComponent<Spline>();
    }

    void LateUpdate()
    {
        if (nodes.Count < 2) {
            Debug.LogWarning("Can't set spline from less than 2 Transforms.");
            return;
        }

        if (forceSmooth) {
            SetSmoothHandleRotations();
        }

        while (spline.nodes.Count > nodes.Count) {
            spline.RemoveNode(spline.nodes[0]);
        }
        int i;
        for (i = 0; i < nodes.Count; i++) {
            if (i < spline.nodes.Count) {
                // Sync position of existing node
                if (nodes[i].hasChanged || _forceUpdate) {
                    spline.nodes[i].Position = _SplinePositionAtIndex(i);
                    spline.nodes[i].Direction = _SplineDirectionAtIndex(i);
                    nodes[i].hasChanged = false;
                }
            } else {
                // Add new node to spline
                spline.AddNode(new SplineNode(
                    _SplinePositionAtIndex(i),
                    _SplineDirectionAtIndex(i))
                );
            }
        }
        _forceUpdate = false;
    }

    // Set node rotations to halfway between prev and next node in chain
    void SetSmoothHandleRotations() {
        for (int i = 0; i < nodes.Count; i++) {        
            Transform prevNode = null, nextNode = null;
            Transform thisNode = nodes[i];
            Vector3 direction = Vector3.zero;
            Vector3 fromPrev = Vector3.zero, toNext = Vector3.zero;

            if (0 < i) {
                prevNode = nodes[i-1];
                fromPrev = Vector3.Normalize(thisNode.position - prevNode.position);
                direction = fromPrev;
            }
            if (i < nodes.Count - 1) {
                nextNode = nodes[i+1];
                toNext = Vector3.Normalize(nextNode.position - thisNode.position);
                direction = toNext;
            }

            if (0 < i && i < nodes.Count - 1) {
                direction = Vector3.Slerp(fromPrev, toNext, 0.5f);
            }

            thisNode.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            // thisNode.hasChanged = true;
        }
    }

    Vector3 _SplinePositionAtIndex(int i) {
        // position relative to the top-level Spline object 
        return spline.transform.InverseTransformPoint(nodes[i].position);
    }

    Vector3 _SplineDirectionAtIndex(int i) {
        // Not actually a direction but another point in local space which defines an axis
        Vector3 handle = nodes[i].TransformPoint(Vector3.up);
        return spline.transform.InverseTransformPoint(handle);
    }
}
