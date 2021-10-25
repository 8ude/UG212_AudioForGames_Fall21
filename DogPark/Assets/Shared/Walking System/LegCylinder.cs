using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class LegCylinder : MonoBehaviour
{
    private float localRadius = 0.5f; // Radius of the builtin cylinder mesh
    public float rotationMultiplier = 1.0f; // anything other than 1 doesn't make sense physically but might look more natural
    public Transform body;

    private Vector3 prevPosition;

    private float initialY; 

    // Start is called before the first frame update
    void Start()
    {
        prevPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = transform.position - prevPosition;
        // get component of movement along body's local z (forward)
        float forwardDelta = body.InverseTransformDirection(delta).z;

        float radius = transform.lossyScale.z*localRadius; // global radius of cylinder (is this right?)

        // apply rotation around local y axis
        transform.Rotate(new Vector3(0f, -rotationMultiplier*Mathf.Rad2Deg*forwardDelta/radius, 0f), Space.Self);

        prevPosition = transform.position;
    }
}
