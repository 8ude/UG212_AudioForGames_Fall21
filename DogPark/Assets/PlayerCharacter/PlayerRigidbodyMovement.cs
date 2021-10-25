using System;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRigidbodyMovement : MonoBehaviour
{
  [Header("Tunables")]
  [SerializeField] private FloatReference BaseMoveForce;
  [SerializeField] private FloatReference RunningForceMultiplier;
  [SerializeField] private FloatReference MaxVelocity;
  [SerializeField] private FloatReference RunningMaxVelMultiplier;
  [SerializeField] private FloatReference JumpForce;
  [SerializeField] private FloatReference Drag;
  [SerializeField] private GameObjectReference cameraReference;
  [SerializeField] private KeyCode JumpKey;
  [SerializeField] private FloatReference GroundedRaycastSize;
  [SerializeField] private LayerMask groundLayer;

  [SerializeField] private FloatReference minVelocitySquaredThreshold;
  [SerializeField] private BoolReference lookForwardOnStop;
  //[Tooltip("Proportional ")]
  [SerializeField] private FloatReference lookForwardSmoothFactor;

  [Header("References")]
  [SerializeField] private new Rigidbody rigidbody;
  [SerializeField] private Transform mFootTransform;
  [SerializeField] private Transform mJumpForceTransform;
  [SerializeField] private Transform mStuffThatRotates;

  [Header("FMOD")]
  public FMODUnity.StudioEventEmitter fmodMovementEmitter;
  public FMODUnity.StudioEventEmitter fmodJumpEmitter;
  public string velocityParam;
  public string onGroundParam;
  public string isStrongParam;
  public string yPosParam;

  private IDisposable disposable;

  private Vector3 moveForcePosition => mFootTransform?.position ?? transform.position;
  private Vector3 jumpForcePosition => mJumpForceTransform?.position ?? transform.position;

  // Casts a raycast towards the floor to see if the player is currently on the ground (so that it can jump)
  private bool IsGrounded => Physics.Raycast(transform.position, Vector3.down, GroundedRaycastSize.Value, groundLayer);

  private bool IsRunning => Inputs.Play.GetKey(KeyCode.LeftShift);

  private void OnValidate() {
    if(rigidbody == null) {
      rigidbody = GetComponent<Rigidbody>();
    }
  }

  // Nifty Unity thing that allows you to draw debugging gizmos in the editor
  void OnDrawGizmos()
  {
    Gizmos.color = Color.magenta;
    Gizmos.DrawRay(transform.position, Vector3.down * GroundedRaycastSize.Value);
    Gizmos.color = Color.blue;
    Gizmos.DrawRay(moveForcePosition, rigidbody.velocity);
  }

  void Awake()
  {
    disposable = Drag.Subscribe((val) => {
      rigidbody.drag = val;
    });
  }

  void OnDestroy()
  {
    disposable?.Dispose();
  }

  void FixedUpdate()
  {
    var input = Inputs.Play;

    // Make input be based on the camera transform (WARNING: weird vector math)
    // Forward input has to be camera forward direction (blue arrow in unity inspector) projected to the xz plane (whose normal is Vector3.up)
    var cameraPlanarForward = Vector3.ProjectOnPlane(cameraReference.Value.transform.forward, Vector3.up).normalized;
    var forwardInput = cameraPlanarForward * input.GetAxis("Vertical");
    // Since the camera will not roll, the camera right direction (red arrow on unity inspector) is already on the xz plane;
    var horizontalInput = cameraReference.Value.transform.right * input.GetAxis("Horizontal");

    // Make sure the input is normalized (so that diagonal movement (1, 0, 1)  isn't faster than forward movement (0, 0, 1))
    var inputDir = Vector3.Normalize(forwardInput + horizontalInput);

    // Add the force related to moving
    float maxVelocity = MaxVelocity.Value;
    if (IsRunning) maxVelocity *= RunningMaxVelMultiplier;

    // how fast are you already moving in the direction of input?
    float directionalVelocity = Vector3.Dot(rigidbody.velocity, inputDir);

    if(directionalVelocity < maxVelocity) {
      float force = BaseMoveForce.Value;
      if (IsRunning) force *= RunningForceMultiplier.Value;
      rigidbody.AddForceAtPosition(inputDir * force, moveForcePosition, ForceMode.Impulse);
    }

    if(lookForwardOnStop.Value && rigidbody.velocity.sqrMagnitude < minVelocitySquaredThreshold.Value) {
        mStuffThatRotates.rotation = Quaternion.Slerp(
          mStuffThatRotates.rotation,
          Quaternion.LookRotation(cameraPlanarForward, mStuffThatRotates.up),
          lookForwardSmoothFactor.Value * Time.deltaTime);
    }

    fmodMovementEmitter.SetParameter(velocityParam, directionalVelocity/maxVelocity);
    // Debug.Log(velocityParam + " " + directionalVelocity/maxVelocity);
    fmodMovementEmitter.SetParameter(yPosParam, transform.position.y - 10.5f);
    // Debug.Log(yPosParam + " " + (transform.position.y - 10.5f));
    fmodMovementEmitter.SetParameter(onGroundParam, IsGrounded ? 1 : 0);
    // Debug.Log(onGroundParam + " " + (IsGrounded ? 1 : 0));
    fmodMovementEmitter.SetParameter(isStrongParam, IsRunning ? 1 : 0);

    // Does the raycast to check if the player can jump
    if (IsGrounded && input.GetKeyDown(JumpKey))
    {
      // Add the force related to jumping
      //rigidbody.velocity =
      fmodJumpEmitter.Play();
      rigidbody.AddForceAtPosition(Vector3.up * JumpForce, jumpForcePosition, ForceMode.VelocityChange);
    }
  }
}
