using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;

[RequireComponent(typeof(Rigidbody))]
public class RollingSoundFmod : MonoBehaviour
{
	public float maxVelocity = 2f;

    [SerializeField] private float groundSpherecastLengthLocal = 0.01f;
    [SerializeField] private float groundSpherecastRadiusLocal = 0.51f;
    [SerializeField] private LayerMask groundLayer;

    public FMODUnity.StudioEventEmitter _fmodEmitter;
    private const string _velocityParam = "BallVelocity";
    private const string _onGroundParam = "OnGround";
    private const string _yPosParam = "YPos";
    private const string _ballTypeParam = "BallType";

    public int ballType;

    private Rigidbody _rigidbody;

    void Start() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + Vector3.down*GroundSpherecastLength, GroundSpherecastRadius);
    }

    private float GroundSpherecastLength => groundSpherecastLengthLocal*transform.lossyScale.y;
    private float GroundSpherecastRadius => groundSpherecastRadiusLocal*transform.lossyScale.y;

    // private RaycastHit _r;
    //private bool IsGrounded => Physics.SphereCast(transform.position, GroundSpherecastRadius, Vector3.down, out _r, GroundSpherecastLength, groundLayer);
    private bool IsGrounded => Physics.CheckSphere(transform.position + Vector3.down*GroundSpherecastLength, GroundSpherecastRadius, groundLayer);

    // Update is called once per frame
    void Update()
    {
        Vector3 vel = _rigidbody.velocity;
        float velocity = (new Vector2(vel.x, vel.z)).magnitude;

        float velocityFactor;
        // if (IsGrounded) {
        //     velocityFactor = velocity/maxVelocity;
        // } else {
        //     velocityFactor = 0f;
        // }

        // Debug.Log(IsGrounded);
        // Debug.Log(velocity);

        velocityFactor = velocity/maxVelocity;

        _fmodEmitter.SetParameter(_velocityParam, velocityFactor);
        _fmodEmitter.SetParameter(_onGroundParam, IsGrounded ? 1 : 0);
        _fmodEmitter.SetParameter(_yPosParam, transform.position.y - 4f);
        _fmodEmitter.SetParameter(_ballTypeParam, ballType);
        // if (velocityFactor > 0f) Debug.Log(velocityFactor);
        //float volume = Mathf.SmoothDamp(audioSource.volume, targetVolume, ref _smoothVelocity, smoothTime);
    }
}
