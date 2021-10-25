using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IKBody : MonoBehaviour
{
    public enum Type {
        TwoLegs,
        FourLegs
    }
    public Type type = Type.FourLegs;

    public Transform backLeftFoot;
    public Transform backRightFoot;

    // Only for FourLegs:
    public Transform frontLeftFoot;
    public Transform frontRightFoot;

    public float xRotationMultiplier = 1f; // Only for FourLegs
    public float zRotationMultiplier = 1f;
    public float yDisplacementMultiplier = 1f;
    public float yDisplacementMax = 1f;

    public float smoothHalfLife = 0.05f;


    // These are set automatically but must be saved to prefab if spawning from prefab (don't ask me why):
    public float initialHeightOffset;
    public float initialAvgFootOffset;

    public float zSeparation;
    public float xSeparation;

    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isPlaying) return;

    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying) {
            // Save initial variables from edit mode (they don't seem to init correctly at game start)
            initialHeightOffset = transform.position.y - transform.parent.position.y;
            initialAvgFootOffset = AverageFootHeight() - transform.position.y;

            // Assume legs are arranged in a rectangle aligned with the body object's zx plane
            xSeparation = Vector3.Project((backRightFoot.position - backLeftFoot.position), transform.right).magnitude;
            if (type == Type.FourLegs) {
                zSeparation = Vector3.Project((backLeftFoot.position - frontLeftFoot.position), transform.forward).magnitude;
            }
            return;
        }

        float defaultHeight = initialHeightOffset + transform.parent.position.y;
        
        Vector3 pos = transform.position;
        float footOffset = (AverageFootHeight() - transform.position.y);
        float yDisplacement = Mathf.Clamp(yDisplacementMultiplier*(footOffset - initialAvgFootOffset), -yDisplacementMax, yDisplacementMax);
        pos.y = defaultHeight + yDisplacement;

        // shouldn't be necessary but force local z & x to zero to fix bug caused by PetRotation hackiness :(
        pos.x = transform.parent.position.x;
        pos.z = transform.parent.position.z;

        float factor = 1f - Mathf.Min(1f, Mathf.Exp((Mathf.Log(0.5f)/smoothHalfLife)*Time.deltaTime));
        transform.position = Vector3.Lerp(transform.position, pos, factor);

        float zRotation = zRotationMultiplier*Mathf.Rad2Deg*Mathf.Atan2(AverageLeftHeight() - AverageRightHeight(), xSeparation);
        float xRotation = (type == Type.FourLegs)
                            ? xRotationMultiplier*Mathf.Rad2Deg*Mathf.Atan2(AverageBackHeight() - AverageFrontHeight(), zSeparation)
                            : 0f;

        Quaternion targetRotation = Quaternion.Euler(
            xRotation,
            0f,
            zRotation
        );

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, factor);
    }

    private float AverageFootHeight() {
        return (type == Type.TwoLegs) ? AverageBackHeight() : 0.5f*(AverageBackHeight() + AverageFrontHeight());
    }
    private float AverageBackHeight() {
        return 0.5f*(backLeftFoot.position.y + backRightFoot.position.y);
    }
    private float AverageFrontHeight() {
        return 0.5f*(frontLeftFoot.position.y + frontRightFoot.position.y);
    }
    private float AverageLeftHeight() {
        return (type == Type.TwoLegs) ? backLeftFoot.position.y : 0.5f*(backLeftFoot.position.y + frontLeftFoot.position.y);
    }
    private float AverageRightHeight() {
        return (type == Type.TwoLegs) ? backRightFoot.position.y  : 0.5f*(backRightFoot.position.y + frontRightFoot.position.y);
    }
}
