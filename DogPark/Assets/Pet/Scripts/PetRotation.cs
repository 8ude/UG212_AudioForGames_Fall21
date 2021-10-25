using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Set visible pet rotation from direction of movement 
public class PetRotation : MonoBehaviour
{
    public float velocityThreshold;

    public float smoothFactor = 0.4f;

    private Vector3 offsetFromParent;
    private Rigidbody parentRigidbody;
    private Quaternion prevRotation;
    // Start is called before the first frame update
    void Start()
    {
        offsetFromParent = transform.position - transform.parent.position;
        parentRigidbody = transform.parent.GetComponent<Rigidbody>();

        prevRotation = transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Ensure offset from parent remains the same, even when parent rotates
        transform.position = transform.parent.position + offsetFromParent;

        transform.rotation = prevRotation; // ignore parent rotation

        if (parentRigidbody.velocity.sqrMagnitude > velocityThreshold*velocityThreshold) {
            // Find component of velocity parallel to the ground (xz plane)
            Vector3 movement = Vector3.ProjectOnPlane(parentRigidbody.velocity, Vector3.up);
            // match our forward rotation to the source
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(Vector3.forward, movement), smoothFactor);
        }         
        prevRotation = transform.rotation;
    }
}
