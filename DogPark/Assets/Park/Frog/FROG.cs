using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public class FROG : MonoBehaviour
{
    public Rigidbody rb;
    public FloatReference force;
    // Update is called once per frame
    void Update()
    {
        var input = Vector3.forward * Input.GetAxis("Vertical")
            + Vector3.right * Input.GetAxis("Horizontal");
        rb.AddForce(force * input.normalized, ForceMode.VelocityChange);
    }
}
