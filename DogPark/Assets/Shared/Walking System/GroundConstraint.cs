using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Set to position of parent but don't allow going underneath the ground
// [ExecuteInEditMode]
public class GroundConstraint : MonoBehaviour
{
    [SerializeField] private float raycastSize = 100f;
    [SerializeField] private LayerMask groundLayer;

    public float smoothTime;

    public float yOffset = 0f;

    public enum Type {
        Above,
        Inside,
        Both,
    }
    public Type type;

    private Vector3 _v; // for smoothing

    private Vector3 initialOffset;
    // Start is called before the first frame update
    void Start()
    {
        initialOffset = transform.position - transform.parent.position;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitInfo;

        Vector3 defaultPosition = initialOffset + transform.parent.position;

        Vector3 target = defaultPosition;

        bool hit = false;

        if (type == Type.Above || type == Type.Both) {
            // raycast down instead of up, in case the ground is a box collider and we're inside it
            Vector3 rayBegin = defaultPosition + raycastSize*Vector3.up;
            hit = Physics.Raycast(rayBegin, Vector3.down, out hitInfo, raycastSize, groundLayer);
            if (hit) {
                target = hitInfo.point;
            }
        }

        if (!hit && (type == Type.Inside || type == Type.Both)) {
            Vector3 rayBegin = defaultPosition;
            hit = Physics.Raycast(rayBegin, Vector3.down, out hitInfo, raycastSize, groundLayer);
            if (hit) {
                target = hitInfo.point;
            }
        }

        if (hit) {
            target.y += yOffset;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _v, smoothTime);
        }
    }
}
